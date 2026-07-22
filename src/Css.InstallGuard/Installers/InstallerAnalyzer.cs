using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Css.Core.Software;
using Css.InstallGuard.Routing;

namespace Css.InstallGuard.Installers;

public enum InstallerKind
{
    Unknown,
    Msi,
    Msix,
    Exe,
    InnoSetup,
    Nsis,
    Burn
}

public sealed class InstallerDetectionResult
{
    public required string InstallerPath { get; init; }
    public required string FileName { get; init; }
    public required string SoftwareName { get; init; }
    public InstallerKind Kind { get; init; }
    public SoftwareCategory Category { get; init; }
    public required InstallRoute RecommendedRoute { get; init; }
    public bool RequiresUserConfirmation { get; init; } = true;
    public bool WillRunInstaller { get; init; }
    public IReadOnlyList<string> Evidence { get; init; } = [];
    public IReadOnlyList<string> CandidateInstallArguments { get; init; } = [];
}

/// <summary>
/// Read-only installer analyzer. It never executes the installer; it only infers
/// type/category/name from path hints and prepares a recommended install route.
/// </summary>
public static class InstallerAnalyzer
{
    public static InstallerDetectionResult AnalyzePackage(
        InstallerPackageEvidence package,
        SoftwareCategory? categoryOverride = null,
        InstallRoutingMemory? routingMemory = null)
    {
        ArgumentNullException.ThrowIfNull(package);
        var result = AnalyzePath(package.PackagePath, categoryOverride, routingMemory);
        var evidence = result.Evidence
            .Concat(package.KindEvidence)
            .Append("真实安装器类型: " + package.DetectedKind)
            .Append("类型可信度: " + package.KindConfidence)
            .ToArray();
        var automaticArguments = package.KindConfidence == InstallerKindConfidence.High
            && package.DetectedKind is InstallerKind.InnoSetup or InstallerKind.Nsis
                ? BuildCandidateArguments(
                    package.DetectedKind,
                    result.RecommendedRoute.TargetInstallPath)
                : [];

        return new InstallerDetectionResult
        {
            InstallerPath = result.InstallerPath,
            FileName = result.FileName,
            SoftwareName = result.SoftwareName,
            Kind = package.DetectedKind,
            Category = result.Category,
            RecommendedRoute = result.RecommendedRoute,
            RequiresUserConfirmation = true,
            WillRunInstaller = false,
            Evidence = evidence,
            CandidateInstallArguments = automaticArguments
        };
    }

    public static InstallerDetectionResult AnalyzePath(
        string installerPath,
        SoftwareCategory? categoryOverride = null,
        InstallRoutingMemory? routingMemory = null)
    {
        var fileName = Path.GetFileName(installerPath);
        var softwareName = GuessSoftwareName(fileName);
        var kind = DetectKind(fileName);
        var category = categoryOverride ?? Classify(fileName, softwareName);
        var route = InstallRoutingEngine.CreateDefault().Recommend(softwareName, category, routingMemory);

        var evidence = new List<string>
        {
            "文件名: " + fileName,
            "推断软件名: " + softwareName,
            "推断安装器类型: " + kind,
            "推荐路径: " + route.TargetInstallPath
        };

        return new InstallerDetectionResult
        {
            InstallerPath = installerPath,
            FileName = fileName,
            SoftwareName = softwareName,
            Kind = kind,
            Category = category,
            RecommendedRoute = route,
            RequiresUserConfirmation = true,
            WillRunInstaller = false,
            Evidence = evidence,
            CandidateInstallArguments = BuildCandidateArguments(kind, route.TargetInstallPath)
        };
    }

    private static InstallerKind DetectKind(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        if (ext.Equals(".msi", StringComparison.OrdinalIgnoreCase)) return InstallerKind.Msi;
        if (ext.Equals(".msix", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".appx", StringComparison.OrdinalIgnoreCase)) return InstallerKind.Msix;

        var lower = fileName.ToLowerInvariant();
        if (lower.Contains("inno")) return InstallerKind.InnoSetup;
        if (lower.Contains("nsis")) return InstallerKind.Nsis;
        if (lower.Contains("burn") || lower.Contains("bootstrapper")) return InstallerKind.Burn;
        if (ext.Equals(".exe", StringComparison.OrdinalIgnoreCase)) return InstallerKind.Exe;
        return InstallerKind.Unknown;
    }

    private static SoftwareCategory Classify(string fileName, string softwareName)
    {
        var text = (fileName + " " + softwareName).ToLowerInvariant();
        if (ContainsAny(text, "steam", "epic", "gog", "battle.net", "game")) return SoftwareCategory.Game;
        if (ContainsAny(text, "ollama", "claude", "openai", "comfyui", "stable diffusion", "lm studio", "cursor")) return SoftwareCategory.Ai;
        if (ContainsAny(text, "docker", "visual studio", "vscode", "git", "node", "python", "jetbrains", "sdk")) return SoftwareCategory.DevelopmentTool;
        return SoftwareCategory.Normal;
    }

    private static string GuessSoftwareName(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        name = Regex.Replace(name, "(setup|installer|install|bootstrapper|win32|win64|x64|x86|amd64)", " ", RegexOptions.IgnoreCase);
        name = Regex.Replace(name, "v?\\d+(\\.\\d+){1,4}", " ", RegexOptions.IgnoreCase);
        name = Regex.Replace(name, "[-_\\.]+", " ");
        name = Regex.Replace(name, "\\s+", " ").Trim();

        if (string.IsNullOrWhiteSpace(name)) name = "UnknownSoftware";
        return string.Join(" ", name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(CapitalizeKnown));
    }

    private static string CapitalizeKnown(string token)
    {
        if (token.Equals("ai", StringComparison.OrdinalIgnoreCase)) return "AI";
        if (token.Length <= 1) return token.ToUpperInvariant();
        return char.ToUpperInvariant(token[0]) + token[1..];
    }

    private static IReadOnlyList<string> BuildCandidateArguments(InstallerKind kind, string targetPath) =>
        kind switch
        {
            InstallerKind.Msi =>
            [
                $"TARGETDIR=\"{targetPath}\"",
                $"INSTALLDIR=\"{targetPath}\"",
                $"APPLICATIONFOLDER=\"{targetPath}\""
            ],
            InstallerKind.InnoSetup =>
            [
                $"/DIR=\"{targetPath}\""
            ],
            InstallerKind.Nsis =>
            [
                $"/D={targetPath}"
            ],
            InstallerKind.Burn =>
            [
                $"INSTALLFOLDER=\"{targetPath}\""
            ],
            _ => []
        };

    private static bool ContainsAny(string text, params string[] needles) =>
        needles.Any(n => text.Contains(n, StringComparison.OrdinalIgnoreCase));
}
