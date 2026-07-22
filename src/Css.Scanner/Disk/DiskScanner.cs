using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Css.Rules;
using Css.Rules.Models;

namespace Css.Scanner.Disk;

/// <summary>
/// Orchestrates the four-phase C: root-cause scan: big-rocks probe → directory crawl →
/// classification → result assembly. Pure read-only — safe to run without elevation or UAC.
/// This is the first shippable feature (pain point #2).
/// </summary>
public sealed class DiskScanner
{
    private readonly RootDirCrawler _crawler = new();
    private readonly BigRocksProbe _bigRocks = new();

    /// <summary>
    /// Scans <paramref name="driveRoot"/> (e.g. "C:\\"). <paramref name="rulesPath"/> points to
    /// rules.scan.json; if null, classification still runs with an empty ruleset (everything = Other/Other+unexpected).
    /// </summary>
    public async Task<DriveScanResult> ScanAsync(
        string driveRoot,
        string? rulesPath = null,
        IProgress<string>? progress = null,
        CancellationToken ct = default)
    {
        var drive = new DriveInfo(Path.GetPathRoot(driveRoot) ?? driveRoot);

        ScanRules rules = new();
        if (rulesPath is not null && File.Exists(rulesPath))
        {
            try { rules = new ScanRuleLoader().Load(rulesPath); }
            catch (Exception ex) { progress?.Report("规则加载失败: " + ex.Message); }
        }

        progress?.Report("探测大块头系统文件...");
        var bigRocks = _bigRocks.Probe(Environment.GetFolderPath(Environment.SpecialFolder.Windows));

        progress?.Report("爬取目录...");
        var topLevel = await _crawler.CrawlTopLevelAsync(driveRoot, progress, ct);

        progress?.Report("分类...");
        var classifier = new CategoryClassifier(rules);
        classifier.Classify(topLevel, driveRoot);

        return new DriveScanResult
        {
            Drive = driveRoot,
            TotalBytes = drive.TotalSize,
            FreeBytes = drive.AvailableFreeSpace,
            TopLevel = topLevel,
            BigRocks = bigRocks
        };
    }
}
