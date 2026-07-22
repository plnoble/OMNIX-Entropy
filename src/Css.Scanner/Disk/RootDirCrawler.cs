using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Css.Core.Operations;

namespace Css.Scanner.Disk;

/// <summary>
/// Recursively sums directory sizes on a target drive. Skips reparse points (junctions/symlinks)
/// to avoid double-counting and infinite loops, ignores inaccessible system folders, streams
/// partial progress, and is cancellable. Designed for the bottom-up phase of the C: scan.
/// </summary>
public sealed class RootDirCrawler
{
    private readonly EnumerationOptions _opts = new()
    {
        AttributesToSkip = FileAttributes.ReparsePoint | FileAttributes.Device,
        IgnoreInaccessible = true,
        RecurseSubdirectories = false,
        ReturnSpecialDirectories = false,
        BufferSize = 8192
    };

    /// <summary>
    /// Crawls <paramref name="root"/> and returns one <see cref="CategoryNode"/> per immediate child,
    /// each with recursive SizeBytes and LastWriteUtc. Progress reports completed top-level entries.
    /// </summary>
    public async Task<List<CategoryNode>> CrawlTopLevelAsync(
        string root,
        IProgress<string>? progress = null,
        CancellationToken ct = default)
    {
        var result = new List<CategoryNode>();
        IEnumerable<FileSystemInfo> top;
        try
        {
            var di = new DirectoryInfo(root);
            top = di.EnumerateFileSystemInfos("*", _opts);
        }
        catch (UnauthorizedAccessException) { return result; }
        catch (DirectoryNotFoundException) { return result; }

        foreach (var entry in top)
        {
            ct.ThrowIfCancellationRequested();
            var node = await Task.Run(() => BuildNode(entry, ct), ct);
            result.Add(node);
            progress?.Report(node.Name);
        }
        return result;
    }

    /// <summary>Recursively builds a size node for a single filesystem entry.</summary>
    private CategoryNode BuildNode(FileSystemInfo entry, CancellationToken ct)
    {
        var node = new CategoryNode
        {
            Name = entry.Name,
            Path = entry.FullName,
            LastWriteUtc = entry.LastWriteTimeUtc
        };

        if (entry is FileInfo fi)
        {
            node = new CategoryNode
            {
                Name = entry.Name,
                Path = entry.FullName,
                IsFile = true,
                LastWriteUtc = entry.LastWriteTimeUtc,
                SizeBytes = fi.Length
            };
            return node;
        }

        long size = 0;
        DateTime latest = entry.LastWriteTimeUtc;
        var children = new List<CategoryNode>();
        IEnumerable<FileSystemInfo> entries;
        try
        {
            entries = ((DirectoryInfo)entry).EnumerateFileSystemInfos("*", _opts);
        }
        catch (UnauthorizedAccessException) { node.SizeBytes = 0; return node; }
        catch (DirectoryNotFoundException) { node.SizeBytes = 0; return node; }

        foreach (var child in entries)
        {
            ct.ThrowIfCancellationRequested();
            if ((child.Attributes & FileAttributes.ReparsePoint) != 0) continue;
            var childNode = BuildNode(child, ct);
            children.Add(childNode);
            size += childNode.SizeBytes;
            if (childNode.LastWriteUtc is { } cw && cw > latest) latest = cw;
        }

        node.SizeBytes = size;
        node.LastWriteUtc = latest;
        // Keep only the largest children to bound treemap memory; full tree is expensive.
        // For top-level nodes we keep all; deep drilldown handled lazily by the UI.
        node.Children.AddRange(children);
        return node;
    }
}
