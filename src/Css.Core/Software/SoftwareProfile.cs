using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Css.Core.Software;

public enum SoftwareCategory
{
    Unknown,
    Normal,
    Game,
    Ai,
    DevelopmentTool,
    SystemTool
}

public enum SoftwareCategoryConfidence
{
    Unknown,
    Low,
    Medium,
    High
}

public enum SoftwareCategoryEvidenceSource
{
    ProductName,
    Publisher,
    InstallLocation
}

public sealed class SoftwareCategoryEvidence
{
    public required SoftwareCategoryEvidenceSource Source { get; init; }
    public required string MatchedRule { get; init; }
}

public sealed class SoftwareCategoryAssessment
{
    public SoftwareCategory Category { get; init; } = SoftwareCategory.Unknown;
    public SoftwareCategoryConfidence Confidence { get; init; } = SoftwareCategoryConfidence.Unknown;
    public bool IsFallback { get; init; }
    public IReadOnlyList<SoftwareCategoryEvidence> Evidence { get; init; } = [];
}

/// <summary>
/// Shared software inventory model used by cleanup, install routing, migration,
/// uninstall residue detection, and AI explanations.
/// </summary>
public sealed class SoftwareProfile
{
    public SoftwareProfile()
    {
    }

    [SetsRequiredMembers]
    public SoftwareProfile(SoftwareProfile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Name = source.Name;
        Publisher = source.Publisher;
        SignatureSubject = source.SignatureSubject;
        Category = source.Category;
        CategoryAssessment = source.CategoryAssessment;
        DisplayVersion = source.DisplayVersion;
        InventorySource = source.InventorySource;
        InstallPath = source.InstallPath;
        UninstallCommand = source.UninstallCommand;
        DisplayIconPath = source.DisplayIconPath;
        DisplayIconIndex = source.DisplayIconIndex;
        ReinstallSource = source.ReinstallSource;
        IsWindowsInstaller = source.IsWindowsInstaller;
        WindowsInstallerProductCode = source.WindowsInstallerProductCode;
        InstallDate = source.InstallDate;
        InstalledSizeBytes = source.InstalledSizeBytes;
        DataSizeBytes = source.DataSizeBytes;
        CDriveDataSizeBytes = source.CDriveDataSizeBytes;
        CacheSizeBytes = source.CacheSizeBytes;
        RecentGrowthBytes = source.RecentGrowthBytes;
        DataPaths = source.DataPaths;
        CachePaths = source.CachePaths;
        LogPaths = source.LogPaths;
        CDriveWritePaths = source.CDriveWritePaths;
        RunningProcesses = source.RunningProcesses;
        StartupEntries = source.StartupEntries;
        Services = source.Services;
        ScheduledTasks = source.ScheduledTasks;
        BackgroundComponents = source.BackgroundComponents;
        SystemFootprints = source.SystemFootprints;
        CommunityCacheEvidence = source.CommunityCacheEvidence;
    }

    public required string Name { get; init; }
    public string? Publisher { get; init; }
    public string? SignatureSubject { get; init; }
    public SoftwareCategory Category { get; init; } = SoftwareCategory.Unknown;
    public SoftwareCategoryAssessment CategoryAssessment { get; init; } = new();
    public string? DisplayVersion { get; init; }
    public string? InventorySource { get; init; }
    public string? InstallPath { get; init; }
    public string? UninstallCommand { get; init; }
    public string? DisplayIconPath { get; init; }
    public int DisplayIconIndex { get; init; }
    public string? ReinstallSource { get; init; }
    public bool IsWindowsInstaller { get; init; }
    public string? WindowsInstallerProductCode { get; init; }
    public DateOnly? InstallDate { get; init; }
    public long InstalledSizeBytes { get; init; }
    public long DataSizeBytes { get; init; }
    public long CDriveDataSizeBytes { get; init; }
    public long CacheSizeBytes { get; init; }
    public long RecentGrowthBytes { get; init; }
    public IReadOnlyList<string> DataPaths { get; init; } = [];
    public IReadOnlyList<string> CachePaths { get; init; } = [];
    public IReadOnlyList<string> LogPaths { get; init; } = [];
    public IReadOnlyList<string> CDriveWritePaths { get; init; } = [];
    public IReadOnlyList<string> RunningProcesses { get; init; } = [];
    public IReadOnlyList<string> StartupEntries { get; init; } = [];
    public IReadOnlyList<string> Services { get; init; } = [];
    public IReadOnlyList<string> ScheduledTasks { get; init; } = [];
    public IReadOnlyList<BackgroundComponentObservation> BackgroundComponents { get; init; } = [];
    public IReadOnlyList<SoftwareSystemFootprintObservation> SystemFootprints { get; init; } = [];
    public IReadOnlyList<CommunityRuleCacheEvidence> CommunityCacheEvidence { get; init; } = [];
}
