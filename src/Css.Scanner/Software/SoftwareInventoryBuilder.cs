using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Css.Core.Software;

namespace Css.Scanner.Software;

public static class SoftwareInventoryBuilder
{
    private static readonly string[] CacheChildNames =
    [
        "Cache",
        "Caches",
        "Code Cache",
        "GPUCache",
        "ShaderCache",
        "DawnCache"
    ];

    private static readonly string[] LogChildNames =
    [
        "Log",
        "Logs"
    ];

    private static readonly string[] BrowserProfileFolderNames =
    [
        "Default",
        "Profile 1",
        "Profile 2",
        "Profile 3",
        "Profile 4",
        "Profile 5",
        "Profile 6",
        "Profile 7",
        "Profile 8",
        "System Profile",
        "Guest Profile"
    ];

    public static IReadOnlyList<SoftwareProfile> Build(
        IEnumerable<InstalledSoftwareRecord> installedRecords,
        IEnumerable<StartupEntry> startupEntries,
        IEnumerable<ServiceEntry> services,
        IEnumerable<ScheduledTaskEntry> scheduledTasks,
        Func<string, string?>? signatureResolver = null,
        IEnumerable<ProcessEntry>? runningProcesses = null,
        Func<string, long>? installSizeResolver = null,
        IEnumerable<string>? userDataRoots = null,
        Func<string, bool>? pathExists = null,
        Func<string, long>? cacheSizeResolver = null,
        DateTimeOffset? observedAtUtc = null)
    {
        var startups = startupEntries.ToList();
        var serviceList = services.ToList();
        var taskList = scheduledTasks.ToList();
        var processList = (runningProcesses ?? []).ToList();
        var dataRoots = (userDataRoots ?? [])
            .Where(root => !string.IsNullOrWhiteSpace(root))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var exists = pathExists ?? Directory.Exists;
        var observationTime = (observedAtUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();

        return installedRecords
            .Where(r => IsUsableDisplayName(r.DisplayName))
            .GroupBy(r => DedupeKey(r), StringComparer.OrdinalIgnoreCase)
            .Select(g => BuildProfile(
                g.First(),
                startups,
                serviceList,
                taskList,
                processList,
                signatureResolver,
                installSizeResolver,
                dataRoots,
                exists,
                cacheSizeResolver,
                observationTime))
            .OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static SoftwareProfile BuildProfile(
        InstalledSoftwareRecord record,
        IReadOnlyList<StartupEntry> startups,
        IReadOnlyList<ServiceEntry> services,
        IReadOnlyList<ScheduledTaskEntry> tasks,
        IReadOnlyList<ProcessEntry> processes,
        Func<string, string?>? signatureResolver,
        Func<string, long>? installSizeResolver,
        IReadOnlyList<string> userDataRoots,
        Func<string, bool> pathExists,
        Func<string, long>? cacheSizeResolver,
        DateTimeOffset observedAtUtc)
    {
        var iconReference = DisplayIconReferenceParser.Parse(record.DisplayIcon);
        var installPath = PromoteInstallRoot(
            NormalizeInstallPath(record.InstallLocation)
            ?? ExtractExecutableDirectory(iconReference?.Path ?? record.DisplayIcon)
            ?? ExtractExecutableDirectory(record.UninstallCommand),
            record.DisplayName);
        var executableHint = ExtractExecutablePath(iconReference?.Path ?? record.DisplayIcon)
            ?? ExtractExecutablePath(record.UninstallCommand);
        var relatedStartupRecords = startups
            .Where(s => IsRelated(record.DisplayName, installPath, s.Command))
            .ToList();
        var relatedStartups = relatedStartupRecords
            .Select(s => s.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var relatedServiceRecords = services
            .Where(s => IsRelated(record.DisplayName, installPath, s.PathName) || IsNameRelated(record.DisplayName, s.DisplayName))
            .ToList();
        var relatedServices = relatedServiceRecords
            .Select(s => s.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var relatedTaskRecords = tasks
            .Where(t => IsRelated(record.DisplayName, installPath, t.ActionPath) || IsNameRelated(record.DisplayName, t.Name))
            .ToList();
        var relatedTasks = relatedTaskRecords
            .Select(t => t.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var relatedProcesses = processes
            .Where(p => IsRelated(record.DisplayName, installPath, p.Path ?? p.Name) || IsNameRelated(record.DisplayName, p.Name))
            .Select(p => p.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var backgroundComponents = relatedStartupRecords
            .Select(item => BackgroundComponentObservationFactory.Startup(
                item.Name,
                item.SourceLocator,
                item.Command,
                observedAtUtc,
                item.ApprovalEvidence))
            .Concat(relatedServiceRecords.Select(item => BackgroundComponentObservationFactory.Service(
                item.Name,
                @"HKLM\SYSTEM\CurrentControlSet\Services\" + item.Name,
                item.PathName,
                item.StartMode,
                item.RuntimeState,
                observedAtUtc)))
            .Concat(relatedTaskRecords.Select(item => BackgroundComponentObservationFactory.ScheduledTask(
                item.Name,
                item.Name,
                item.ActionPath,
                item.IsEnabled,
                observedAtUtc)))
            .GroupBy(item => item.Identity.StableId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        var cDriveWrites = new List<string>();
        if (IsCDrivePath(installPath)) cDriveWrites.Add(installPath!);
        var userData = FindUserDataCandidates(
            record.DisplayName,
            installPath,
            userDataRoots,
            pathExists,
            cacheSizeResolver);
        cDriveWrites.AddRange(userData.CDriveWritePaths);

        var categoryAssessment = Classify(record.DisplayName, record.Publisher, installPath);

        return new SoftwareProfile
        {
            Name = record.DisplayName.Trim(),
            Publisher = record.Publisher,
            SignatureSubject = executableHint is null ? null : signatureResolver?.Invoke(executableHint),
            Category = categoryAssessment.Category,
            CategoryAssessment = categoryAssessment,
            InstallPath = installPath,
            UninstallCommand = record.UninstallCommand,
            DisplayIconPath = iconReference?.Path,
            DisplayIconIndex = iconReference?.ResourceIndex ?? 0,
            ReinstallSource = record.InstallSource,
            IsWindowsInstaller = record.IsWindowsInstaller,
            WindowsInstallerProductCode = record.WindowsInstallerProductCode,
            InstallDate = record.InstallDate,
            InstalledSizeBytes = installPath is null || installSizeResolver is null
                ? 0
                : Math.Max(0, installSizeResolver(installPath)),
            CacheSizeBytes = userData.CacheSizeBytes,
            DataPaths = userData.DataPaths,
            CachePaths = userData.CachePaths,
            LogPaths = userData.LogPaths,
            CDriveWritePaths = cDriveWrites
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            RunningProcesses = relatedProcesses,
            StartupEntries = relatedStartups,
            Services = relatedServices,
            ScheduledTasks = relatedTasks,
            BackgroundComponents = backgroundComponents
        };
    }

    private static UserDataCandidateSet FindUserDataCandidates(
        string displayName,
        string? installPath,
        IReadOnlyList<string> userDataRoots,
        Func<string, bool> pathExists,
        Func<string, long>? cacheSizeResolver)
    {
        var result = new UserDataCandidateSet();
        if (userDataRoots.Count == 0)
            return result;

        foreach (var root in userDataRoots)
        {
            foreach (var relativeRoot in BuildUserDataRelativeRoots(displayName, installPath))
            {
                var appRoot = Path.Combine(root, relativeRoot);
                AddUserDataRootIfExists(result, appRoot, pathExists, cacheSizeResolver);
            }
        }

        return result;
    }

    private static IReadOnlyList<string> BuildUserDataRelativeRoots(string displayName, string? installPath)
    {
        var roots = BuildUserDataFolderNames(displayName, installPath).ToList();

        foreach (var nestedRoot in BuildNestedUserDataRelativeRoots(displayName, installPath))
        {
            AddDistinct(roots, nestedRoot);
        }

        return roots;
    }

    private static IEnumerable<string> BuildNestedUserDataRelativeRoots(string displayName, string? installPath)
    {
        var displayParts = BuildDisplayNameParts(displayName);
        if (displayParts.Count == 2)
            yield return Path.Combine(displayParts[0], displayParts[1]);

        var installSegments = BuildMeaningfulInstallSegments(installPath);
        for (var i = 0; i < installSegments.Count - 1; i++)
        {
            yield return Path.Combine(installSegments[i], installSegments[i + 1]);
        }
    }

    private static IReadOnlyList<string> BuildDisplayNameParts(string displayName)
    {
        var parts = new List<string>();
        foreach (var part in displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            AddFolderName(parts, part);
        }

        return parts;
    }

    private static IReadOnlyList<string> BuildMeaningfulInstallSegments(string? installPath)
    {
        var segments = new List<string>();
        if (string.IsNullOrWhiteSpace(installPath))
            return segments;

        foreach (var part in installPath.Split('\\', StringSplitOptions.RemoveEmptyEntries))
        {
            if (IsMeaningfulInstallSegment(part))
                AddFolderName(segments, part);
        }

        return segments;
    }

    private static void AddUserDataRootIfExists(
        UserDataCandidateSet result,
        string appRoot,
        Func<string, bool> pathExists,
        Func<string, long>? cacheSizeResolver)
    {
        if (!SafePathExists(pathExists, appRoot))
            return;

        AddDataPath(result, appRoot);
        AddCacheAndLogChildren(result, appRoot, pathExists, cacheSizeResolver);

        var userDataRoot = Path.Combine(appRoot, "User Data");
        if (!SafePathExists(pathExists, userDataRoot))
            return;

        AddDataPath(result, userDataRoot);
        AddCacheAndLogChildren(result, userDataRoot, pathExists, cacheSizeResolver);

        foreach (var profileFolderName in BrowserProfileFolderNames)
        {
            var profileRoot = Path.Combine(userDataRoot, profileFolderName);
            if (!SafePathExists(pathExists, profileRoot))
                continue;

            AddDataPath(result, profileRoot);
            AddCacheAndLogChildren(result, profileRoot, pathExists, cacheSizeResolver);
        }
    }

    private static void AddDataPath(UserDataCandidateSet result, string path)
    {
        AddDistinct(result.DataPaths, path);
        if (IsCDrivePath(path))
            AddDistinct(result.CDriveWritePaths, path);
    }

    private static void AddCacheAndLogChildren(
        UserDataCandidateSet result,
        string dataRoot,
        Func<string, bool> pathExists,
        Func<string, long>? cacheSizeResolver)
    {
        AddChildCandidates(
            dataRoot,
            CacheChildNames,
            pathExists,
            path =>
            {
                if (TryAddDistinct(result.CachePaths, path))
                    result.CacheSizeBytes += SafeSize(cacheSizeResolver, path);

                if (IsCDrivePath(path))
                    AddDistinct(result.CDriveWritePaths, path);
            });

        AddChildCandidates(
            dataRoot,
            LogChildNames,
            pathExists,
            path =>
            {
                AddDistinct(result.LogPaths, path);
                if (IsCDrivePath(path))
                    AddDistinct(result.CDriveWritePaths, path);
            });
    }

    private static IReadOnlyList<string> BuildUserDataFolderNames(string displayName, string? installPath)
    {
        var names = new List<string>();
        AddFolderName(names, displayName);

        if (!string.IsNullOrWhiteSpace(installPath))
        {
            foreach (var part in installPath.Split('\\', StringSplitOptions.RemoveEmptyEntries))
            {
                if (IsMeaningfulInstallSegment(part))
                    AddFolderName(names, part);
            }
        }

        return names
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void AddFolderName(List<string> names, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value
                .Select(ch => invalid.Contains(ch) ? ' ' : ch)
                .ToArray())
            .Trim();
        if (cleaned.Length > 1)
            names.Add(cleaned);
    }

    private static bool IsMeaningfulInstallSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment) || segment.EndsWith(":", StringComparison.Ordinal))
            return false;

        return !segment.Equals("Install", StringComparison.OrdinalIgnoreCase)
            && !segment.Equals("Application", StringComparison.OrdinalIgnoreCase)
            && !segment.Equals("bin", StringComparison.OrdinalIgnoreCase)
            && !segment.Equals("Program Files", StringComparison.OrdinalIgnoreCase)
            && !segment.Equals("Program Files (x86)", StringComparison.OrdinalIgnoreCase)
            && !segment.Equals("Software", StringComparison.OrdinalIgnoreCase)
            && !segment.Equals("Game", StringComparison.OrdinalIgnoreCase)
            && !segment.Equals("Agent", StringComparison.OrdinalIgnoreCase)
            && !segment.Equals("Development", StringComparison.OrdinalIgnoreCase);
    }

    private static void AddChildCandidates(
        string appRoot,
        IReadOnlyList<string> childNames,
        Func<string, bool> pathExists,
        Action<string> add)
    {
        foreach (var childName in childNames)
        {
            var path = Path.Combine(appRoot, childName);
            if (SafePathExists(pathExists, path))
                add(path);
        }
    }

    private static bool SafePathExists(Func<string, bool> pathExists, string path)
    {
        try
        {
            return pathExists(path);
        }
        catch
        {
            return false;
        }
    }

    private static long SafeSize(Func<string, long>? sizeResolver, string path)
    {
        if (sizeResolver is null)
            return 0;

        try
        {
            return Math.Max(0, sizeResolver(path));
        }
        catch
        {
            return 0;
        }
    }

    private static void AddDistinct(List<string> values, string value)
    {
        TryAddDistinct(values, value);
    }

    private static bool TryAddDistinct(List<string> values, string value)
    {
        if (!values.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            values.Add(value);
            return true;
        }

        return false;
    }

    private sealed class UserDataCandidateSet
    {
        public List<string> DataPaths { get; } = new();
        public List<string> CachePaths { get; } = new();
        public List<string> LogPaths { get; } = new();
        public List<string> CDriveWritePaths { get; } = new();
        public long CacheSizeBytes { get; set; }
    }

    private static string DedupeKey(InstalledSoftwareRecord record) =>
        (record.DisplayName.Trim() + "|" + (NormalizeInstallPath(record.InstallLocation) ?? "")).ToUpperInvariant();

    private static bool IsUsableDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName)) return false;
        var trimmed = displayName.Trim();
        if (trimmed.StartsWith("${", StringComparison.Ordinal) && trimmed.EndsWith("}", StringComparison.Ordinal))
            return false;
        if (trimmed.StartsWith("%", StringComparison.Ordinal) && trimmed.EndsWith("%", StringComparison.Ordinal))
            return false;
        return true;
    }

    private static SoftwareCategoryAssessment Classify(string name, string? publisher, string? path)
    {
        var sources = new[]
        {
            new CategoryTextSource(SoftwareCategoryEvidenceSource.ProductName, name),
            new CategoryTextSource(SoftwareCategoryEvidenceSource.Publisher, publisher),
            new CategoryTextSource(SoftwareCategoryEvidenceSource.InstallLocation, path)
        };
        var rules = new[]
        {
            new CategoryRule(SoftwareCategory.Game,
                ["steam", "epic games", "gog", "battle.net", "game"]),
            new CategoryRule(SoftwareCategory.Ai,
                ["marvis", "antigravity", "ollama", "claude", "openai", "comfyui", "stable diffusion", "lm studio", "cursor", "codex", "opencode"]),
            new CategoryRule(SoftwareCategory.DevelopmentTool,
                ["docker", "visual studio", "git", "node.js", "python", "jetbrains", "sdk", "windows subsystem"]),
            new CategoryRule(SoftwareCategory.SystemTool,
                ["driver", "runtime", "redistributable", "windows update", "microsoft edge webview"])
        };

        foreach (var rule in rules)
        {
            var evidence = sources
                .SelectMany(source => rule.Terms
                    .Where(term => Contains(source.Text, term))
                    .Take(1)
                    .Select(term => new SoftwareCategoryEvidence
                    {
                        Source = source.Source,
                        MatchedRule = term
                    }))
                .ToList();
            if (evidence.Count == 0)
                continue;

            return new SoftwareCategoryAssessment
            {
                Category = rule.Category,
                Confidence = ConfidenceFor(evidence),
                Evidence = evidence
            };
        }

        return new SoftwareCategoryAssessment
        {
            Category = SoftwareCategory.Normal,
            Confidence = SoftwareCategoryConfidence.Low,
            IsFallback = true
        };
    }

    private static SoftwareCategoryConfidence ConfidenceFor(
        IReadOnlyList<SoftwareCategoryEvidence> evidence)
    {
        if (evidence.Any(item => item.Source == SoftwareCategoryEvidenceSource.ProductName))
            return SoftwareCategoryConfidence.High;
        if (evidence.Any(item => item.Source == SoftwareCategoryEvidenceSource.Publisher))
            return SoftwareCategoryConfidence.Medium;
        return SoftwareCategoryConfidence.Low;
    }

    private static bool Contains(string? text, string needle) =>
        !string.IsNullOrWhiteSpace(text)
        && text.Contains(needle, StringComparison.OrdinalIgnoreCase);

    private sealed record CategoryTextSource(
        SoftwareCategoryEvidenceSource Source,
        string? Text);

    private sealed record CategoryRule(
        SoftwareCategory Category,
        IReadOnlyList<string> Terms);

    private static bool IsRelated(string displayName, string? installPath, string command)
    {
        if (!string.IsNullOrWhiteSpace(installPath)
            && command.Contains(installPath, StringComparison.OrdinalIgnoreCase))
            return true;

        return IsNameRelated(displayName, command);
    }

    private static bool IsNameRelated(string displayName, string text)
    {
        var compactName = Compact(displayName);
        var compactText = Compact(text);
        return compactName.Length >= 4 && compactText.Contains(compactName, StringComparison.OrdinalIgnoreCase);
    }

    private static string Compact(string value) =>
        new(value.Where(char.IsLetterOrDigit).ToArray());

    private static string? NormalizeInstallPath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = TrimCommandDecorations(value);
        return trimmed.TrimEnd('\\');
    }

    private static string? ExtractExecutableDirectory(string? command)
    {
        var executable = ExtractExecutablePath(command);
        if (string.IsNullOrWhiteSpace(executable)) return null;
        return Path.GetDirectoryName(executable)?.TrimEnd('\\');
    }

    private static string? PromoteInstallRoot(string? path, string displayName)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var parts = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        var compactName = Compact(displayName);
        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (!parts[i].Equals("Install", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!Compact(parts[i + 1]).Equals(compactName, StringComparison.OrdinalIgnoreCase))
                continue;

            return string.Join("\\", parts.Take(i + 1)).TrimEnd('\\');
        }

        return path;
    }

    private static string? ExtractExecutablePath(string? command)
    {
        if (string.IsNullOrWhiteSpace(command)) return null;
        var trimmed = TrimCommandDecorations(command);
        var exeIndex = trimmed.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        if (exeIndex >= 0) return trimmed[..(exeIndex + 4)];
        return null;
    }

    private static string TrimCommandDecorations(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.StartsWith('"'))
        {
            var end = trimmed.IndexOf('"', 1);
            if (end > 1) return trimmed[1..end];
        }

        var comma = trimmed.IndexOf(',');
        if (comma > 0) trimmed = trimmed[..comma];
        return trimmed.Trim('"', ' ');
    }

    private static bool IsCDrivePath(string? path) =>
        !string.IsNullOrWhiteSpace(path) && path.StartsWith("C:\\", StringComparison.OrdinalIgnoreCase);
}
