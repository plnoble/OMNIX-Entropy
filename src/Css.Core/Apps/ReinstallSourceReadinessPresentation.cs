using System;
using System.Collections.Generic;
using System.IO;
using Css.Core.Software;

namespace Css.Core.Apps;

public enum ReinstallSourceReadinessStatus
{
    Missing,
    ProductCodeHint,
    DirectoryHint,
    InstallerFileUnverified,
    SignatureMismatch,
    VerifiedPublisherSignedInstaller
}

public enum ReinstallSourceOrigin
{
    RegistryMetadata,
    UserSelected
}

public sealed class ReinstallSourceReadinessViewModel
{
    public required ReinstallSourceOrigin SourceOrigin { get; init; }
    public required ReinstallSourceReadinessStatus Status { get; init; }
    public required string StatusLabel { get; init; }
    public required string AgentConclusion { get; init; }
    public required string NextAction { get; init; }
    public required bool CanUseAsRecoveryEvidence { get; init; }
    public required bool CanExecuteDirectly { get; init; }
    public OfficialUninstallRecoveryEvidence? RecoveryEvidence { get; init; }
    public required IReadOnlyList<string> TechnicalDetails { get; init; }
}

public static class ReinstallSourceReadinessPresenter
{
    public static ReinstallSourceReadinessViewModel Create(
        SoftwareProfile profile,
        Func<string, bool> fileExists,
        Func<string, bool> directoryExists,
        Func<string, string?> signatureResolver) =>
        CreateCore(
            profile,
            profile.ReinstallSource,
            ReinstallSourceOrigin.RegistryMetadata,
            fileExists,
            directoryExists,
            signatureResolver);

    public static ReinstallSourceReadinessViewModel CreateForSelectedInstaller(
        SoftwareProfile profile,
        string selectedPath,
        Func<string, bool> fileExists,
        Func<string, string?> signatureResolver) =>
        CreateCore(
            profile,
            selectedPath,
            ReinstallSourceOrigin.UserSelected,
            fileExists,
            _ => false,
            signatureResolver);

    private static ReinstallSourceReadinessViewModel CreateCore(
        SoftwareProfile profile,
        string? source,
        ReinstallSourceOrigin sourceOrigin,
        Func<string, bool> fileExists,
        Func<string, bool> directoryExists,
        Func<string, string?> signatureResolver)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(fileExists);
        ArgumentNullException.ThrowIfNull(directoryExists);
        ArgumentNullException.ThrowIfNull(signatureResolver);

        var technicalDetails = BuildTechnicalDetails(profile, source);

        if (string.IsNullOrWhiteSpace(source))
        {
            return !string.IsNullOrWhiteSpace(profile.WindowsInstallerProductCode)
                ? Hint(
                    ReinstallSourceReadinessStatus.ProductCodeHint,
                    "\u53ea\u627e\u5230\u5b89\u88c5\u8bb0\u5f55\u7ebf\u7d22",
                    "Agent \u53ea\u627e\u5230 Windows Installer \u7684\u4ea7\u54c1\u8bb0\u5f55\u7ebf\u7d22\uff0c\u8fd8\u6ca1\u6709\u53ef\u9a8c\u8bc1\u7684\u5b89\u88c5\u5305\u3002",
                    "\u9700\u8981\u627e\u5230\u5b98\u65b9\u5b89\u88c5\u5305\u5e76\u9a8c\u8bc1\u7b7e\u540d\uff0c\u624d\u80fd\u4f5c\u4e3a\u5378\u8f7d\u540e\u6062\u590d\u65b9\u5f0f\u3002",
                    technicalDetails,
                    sourceOrigin)
                : Hint(
                    ReinstallSourceReadinessStatus.Missing,
                    "\u672a\u627e\u5230\u91cd\u88c5\u6765\u6e90",
                    "Agent \u8fd8\u6ca1\u627e\u5230\u53ef\u7528\u6765\u91cd\u65b0\u5b89\u88c5\u8fd9\u4e2a\u8f6f\u4ef6\u7684\u6765\u6e90\u3002",
                    "\u5148\u51c6\u5907\u5b98\u65b9\u5b89\u88c5\u5305\u6216 Windows \u8fd8\u539f\u70b9\uff0c\u518d\u8003\u8651\u5378\u8f7d\u3002",
                    technicalDetails,
                    sourceOrigin);
        }

        if (SafeExists(directoryExists, source))
        {
            return Hint(
                ReinstallSourceReadinessStatus.DirectoryHint,
                "\u627e\u5230\u4e00\u4e2a\u6765\u6e90\u76ee\u5f55\u7ebf\u7d22",
                "Agent \u627e\u5230\u4e86\u4e00\u4e2a\u53ef\u80fd\u7684\u6765\u6e90\u76ee\u5f55\u7ebf\u7d22\uff0c\u4f46\u76ee\u5f55\u672c\u8eab\u4e0d\u662f\u53ef\u9a8c\u8bc1\u7684\u5b89\u88c5\u5305\u3002",
                "\u9700\u8981\u5728\u5176\u4e2d\u627e\u5230\u5b98\u65b9\u5b89\u88c5\u5305\u5e76\u9a8c\u8bc1\u53d1\u5e03\u8005\u7b7e\u540d\u3002",
                technicalDetails,
                sourceOrigin);
        }

        if (!SafeExists(fileExists, source))
        {
            return Hint(
                ReinstallSourceReadinessStatus.Missing,
                "\u8bb0\u5f55\u7684\u91cd\u88c5\u6765\u6e90\u5df2\u4e0d\u53ef\u7528",
                "Agent \u627e\u5230\u4e86\u5386\u53f2\u6765\u6e90\u7ebf\u7d22\uff0c\u4f46\u5bf9\u5e94\u6587\u4ef6\u73b0\u5728\u4e0d\u5b58\u5728\u3002",
                "\u8bf7\u91cd\u65b0\u83b7\u53d6\u5b98\u65b9\u5b89\u88c5\u5305\uff0c\u4e0d\u8981\u4f9d\u8d56\u8fd9\u6761\u65e7\u8bb0\u5f55\u3002",
                technicalDetails,
                sourceOrigin);
        }

        if (!IsInstallerFile(source))
        {
            return Hint(
                ReinstallSourceReadinessStatus.InstallerFileUnverified,
                "\u627e\u5230\u7684\u6587\u4ef6\u4e0d\u662f\u5df2\u8bc6\u522b\u7684\u5b89\u88c5\u5305",
                "Agent \u627e\u5230\u4e86\u4e00\u4e2a\u6587\u4ef6\u7ebf\u7d22\uff0c\u4f46\u5b83\u4e0d\u662f\u5df2\u8bc6\u522b\u7684 EXE \u6216 MSI \u5b89\u88c5\u5305\u3002",
                "\u4e0d\u8981\u4f7f\u7528\u8fd9\u4e2a\u6587\u4ef6\u4f5c\u4e3a\u6062\u590d\u4fdd\u969c\u3002",
                technicalDetails,
                sourceOrigin);
        }

        var signature = SafeResolveSignature(signatureResolver, source);
        technicalDetails = [.. technicalDetails, "\u5b89\u88c5\u5305\u7b7e\u540d: " + (signature ?? "\u672a\u8bfb\u5230")];
        if (string.IsNullOrWhiteSpace(signature))
        {
            return Hint(
                ReinstallSourceReadinessStatus.InstallerFileUnverified,
                "\u627e\u5230\u5b89\u88c5\u5305\uff0c\u4f46\u7b7e\u540d\u672a\u901a\u8fc7\u9a8c\u8bc1",
                "Agent \u627e\u5230\u4e86\u4e00\u4e2a\u5b89\u88c5\u5305\u7ebf\u7d22\uff0c\u4f46\u65e0\u6cd5\u786e\u8ba4\u5b83\u6765\u81ea\u8f6f\u4ef6\u53d1\u5e03\u8005\u3002",
                "\u8bf7\u4ece\u5b98\u65b9\u6e20\u9053\u91cd\u65b0\u83b7\u53d6\u5b89\u88c5\u5305\uff0c\u6682\u65f6\u4e0d\u8981\u4f7f\u7528\u8fd9\u4e2a\u6587\u4ef6\u3002",
                technicalDetails,
                sourceOrigin);
        }

        if (!PublisherMatches(profile.Publisher, signature))
        {
            return Hint(
                ReinstallSourceReadinessStatus.SignatureMismatch,
                "\u5b89\u88c5\u5305\u7b7e\u540d\u4e0e\u8f6f\u4ef6\u53d1\u5e03\u8005\u4e0d\u5339\u914d",
                "Agent \u627e\u5230\u4e86\u5b89\u88c5\u5305\uff0c\u4f46\u7b7e\u540d\u8eab\u4efd\u4e0e\u5f53\u524d\u8f6f\u4ef6\u7684\u53d1\u5e03\u8005\u5bf9\u4e0d\u4e0a\u3002",
                "\u4e0d\u8981\u4f7f\u7528\u8fd9\u4e2a\u5b89\u88c5\u5305\uff1b\u8bf7\u4ece\u8f6f\u4ef6\u5b98\u65b9\u6e20\u9053\u91cd\u65b0\u83b7\u53d6\u3002",
                technicalDetails,
                sourceOrigin);
        }

        return new ReinstallSourceReadinessViewModel
        {
            SourceOrigin = sourceOrigin,
            Status = ReinstallSourceReadinessStatus.VerifiedPublisherSignedInstaller,
            StatusLabel = "\u5df2\u627e\u5230\u53ef\u9a8c\u8bc1\u7684\u91cd\u88c5\u5b89\u88c5\u5305",
            AgentConclusion = sourceOrigin == ReinstallSourceOrigin.UserSelected
                ? "Agent \u5df2\u9a8c\u8bc1\u4f60\u9009\u62e9\u7684\u5b89\u88c5\u5305\uff1a\u6587\u4ef6\u5b58\u5728\uff0c\u6570\u5b57\u7b7e\u540d\u4e0e\u8f6f\u4ef6\u53d1\u5e03\u8005\u5339\u914d\u3002"
                : "Agent \u5df2\u786e\u8ba4\u8fd9\u662f\u4e00\u4e2a\u5b58\u5728\u7684\u5b89\u88c5\u5305\uff0c\u4e14\u6570\u5b57\u7b7e\u540d\u4e0e\u8f6f\u4ef6\u53d1\u5e03\u8005\u5339\u914d\u3002",
            NextAction = "\u5378\u8f7d\u524d\u4ecd\u9700\u8981\u786e\u8ba4\u4e2a\u4eba\u6570\u636e\u5df2\u5907\u4efd\uff1b\u5f53\u524d\u53ea\u662f\u6062\u590d\u8bc1\u636e\uff0c\u5c1a\u672a\u6267\u884c\u3002",
            CanUseAsRecoveryEvidence = true,
            CanExecuteDirectly = false,
            RecoveryEvidence = new OfficialUninstallRecoveryEvidence
            {
                Method = OfficialUninstallRecoveryMethod.ReinstallSource,
                Reference = source,
                CanRecoverApplication = true,
                UserDataBackupConfirmed = false
            },
            TechnicalDetails = technicalDetails
        };
    }

    private static ReinstallSourceReadinessViewModel Hint(
        ReinstallSourceReadinessStatus status,
        string statusLabel,
        string conclusion,
        string nextAction,
        IReadOnlyList<string> technicalDetails,
        ReinstallSourceOrigin sourceOrigin) =>
        new()
        {
            SourceOrigin = sourceOrigin,
            Status = status,
            StatusLabel = statusLabel,
            AgentConclusion = conclusion,
            NextAction = nextAction,
            CanUseAsRecoveryEvidence = false,
            CanExecuteDirectly = false,
            RecoveryEvidence = null,
            TechnicalDetails = technicalDetails
        };

    private static IReadOnlyList<string> BuildTechnicalDetails(SoftwareProfile profile, string? source) =>
    [
        "\u91cd\u88c5\u6765\u6e90: " + (source ?? "\u672a\u8bb0\u5f55"),
        "Windows Installer: " + (profile.IsWindowsInstaller ? "\u662f" : "\u5426"),
        "MSI \u4ea7\u54c1\u4ee3\u7801: " + (profile.WindowsInstallerProductCode ?? "\u672a\u8bb0\u5f55")
    ];

    private static bool SafeExists(Func<string, bool> exists, string path)
    {
        try
        {
            return exists(path);
        }
        catch
        {
            return false;
        }
    }

    private static string? SafeResolveSignature(Func<string, string?> resolver, string path)
    {
        try
        {
            return resolver(path);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsInstallerFile(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".msi", StringComparison.OrdinalIgnoreCase);
    }

    private static bool PublisherMatches(string? publisher, string? signature)
    {
        var normalizedPublisher = NormalizeIdentity(publisher);
        var normalizedSignature = NormalizeIdentity(signature);
        return normalizedPublisher.Length >= 4
            && normalizedSignature.Length >= normalizedPublisher.Length
            && normalizedSignature.Contains(normalizedPublisher, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeIdentity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return new string(value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
    }
}
