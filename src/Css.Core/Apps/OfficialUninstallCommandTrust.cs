using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Css.Core.Apps;

public enum OfficialUninstallCommandTrustDecision
{
    NotEvaluated,
    Trusted,
    TrustedWindowsInstaller,
    BlockedMissingInstallPath,
    BlockedShellWrapper,
    BlockedOutsideInstallDirectory,
    TrustedPublisherSignature,
    BlockedPublisherSignatureMismatch,
    BlockedSilentWindowsInstaller,
    BlockedUnsafeWindowsInstallerCommand,
    BlockedInvalidPath
}

public sealed class OfficialUninstallCommandTrustResult
{
    public required OfficialUninstallCommandTrustDecision Decision { get; init; }
    public bool IsTrusted => Decision is
        OfficialUninstallCommandTrustDecision.Trusted or
        OfficialUninstallCommandTrustDecision.TrustedWindowsInstaller or
        OfficialUninstallCommandTrustDecision.TrustedPublisherSignature;
    public required string Summary { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }

    public static OfficialUninstallCommandTrustResult NotEvaluated() =>
        new()
        {
            Decision = OfficialUninstallCommandTrustDecision.NotEvaluated,
            Summary = "\u8fd8\u6ca1\u6709\u68c0\u67e5\u547d\u4ee4\u53ef\u4fe1\u5ea6\u3002",
            Reasons = []
        };
}

public static class OfficialUninstallCommandTrustEvaluator
{
    private static readonly Regex WindowsInstallerProductUninstallPattern = new(
        @"(?ix)(^|\s)(/|-)(x|uninstall)\s*\{[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\}(?=\s|$)",
        RegexOptions.Compiled);

    private static readonly Regex WindowsInstallerSilentFlagPattern = new(
        @"(?ix)(^|\s)(/|-)(quiet|passive|q(n|b!?|r|f)?)(?=\s|$)",
        RegexOptions.Compiled);

    private static readonly HashSet<string> ShellWrapperNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "cmd.exe",
        "powershell.exe",
        "pwsh.exe",
        "wscript.exe",
        "cscript.exe",
        "mshta.exe",
        "rundll32.exe",
        "regsvr32.exe"
    };

    public static OfficialUninstallCommandTrustResult Evaluate(
        string executablePath,
        string? installPath,
        string? arguments = null,
        string? expectedPublisher = null,
        string? executableSignatureSubject = null)
    {
        if (string.IsNullOrWhiteSpace(installPath))
        {
            return Blocked(
                OfficialUninstallCommandTrustDecision.BlockedMissingInstallPath,
                "\u65e0\u6cd5\u5224\u65ad\u5378\u8f7d\u5668\u662f\u5426\u53ef\u4fe1\uff0c\u56e0\u4e3a\u8fd8\u4e0d\u77e5\u9053\u8f6f\u4ef6\u88c5\u5728\u54ea\u91cc\u3002",
                "\u5148\u8bc6\u522b\u8f6f\u4ef6\u5b89\u88c5\u76ee\u5f55\u3002");
        }

        try
        {
            var executableFullPath = Path.GetFullPath(executablePath);
            var fileName = Path.GetFileName(executableFullPath);
            if (ShellWrapperNames.Contains(fileName))
            {
                return Blocked(
                    OfficialUninstallCommandTrustDecision.BlockedShellWrapper,
                    "\u5378\u8f7d\u547d\u4ee4\u6307\u5411\u811a\u672c\u5916\u58f3\uff0c\u5df2\u963b\u6b62\u3002",
                    "\u811a\u672c\u5916\u58f3\u53ef\u80fd\u6267\u884c\u4efb\u610f\u811a\u672c\u6216\u6279\u5904\u7406\u547d\u4ee4\u3002");
            }

            if (IsWindowsInstallerExecutable(executablePath, executableFullPath))
                return EvaluateWindowsInstaller(arguments);

            var installRoot = Path.GetFullPath(installPath);
            if (!IsInsideDirectory(installRoot, executableFullPath))
            {
                if (HasPublisherSignatureMatch(expectedPublisher, executableSignatureSubject))
                {
                    return new OfficialUninstallCommandTrustResult
                    {
                        Decision = OfficialUninstallCommandTrustDecision.TrustedPublisherSignature,
                        Summary = "\u5378\u8f7d\u5668\u4e0d\u5728\u5b89\u88c5\u76ee\u5f55\u5185\uff0c\u4f46\u7b7e\u540d\u4e0e\u53d1\u5e03\u8005\u5339\u914d\u3002",
                        Reasons = ["\u53d1\u5e03\u8005\u540d\u79f0\u4e0e\u6587\u4ef6\u7b7e\u540d\u4e3b\u4f53\u5339\u914d\u3002"]
                    };
                }

                if (!string.IsNullOrWhiteSpace(expectedPublisher) ||
                    !string.IsNullOrWhiteSpace(executableSignatureSubject))
                {
                    return Blocked(
                        OfficialUninstallCommandTrustDecision.BlockedPublisherSignatureMismatch,
                        "\u5378\u8f7d\u5668\u4e0d\u5728\u5b89\u88c5\u76ee\u5f55\u5185\uff0c\u7b7e\u540d\u4e5f\u548c\u53d1\u5e03\u8005\u4e0d\u5339\u914d\u3002",
                        "\u5916\u90e8\u5378\u8f7d\u5668\u9700\u8981\u5339\u914d\u7684\u53d1\u5e03\u8005\u7b7e\u540d\u8bc1\u636e\u3002");
                }

                return Blocked(
                    OfficialUninstallCommandTrustDecision.BlockedOutsideInstallDirectory,
                    "\u5378\u8f7d\u5668\u4e0d\u5728\u8fd9\u4e2a\u8f6f\u4ef6\u7684\u5b89\u88c5\u76ee\u5f55\u5185\u3002",
                    "\u7ee7\u7eed\u524d\u9700\u8981\u989d\u5916\u7b7e\u540d\u6216\u6765\u6e90\u68c0\u67e5\u3002");
            }

            return new OfficialUninstallCommandTrustResult
            {
                Decision = OfficialUninstallCommandTrustDecision.Trusted,
                Summary = "\u5378\u8f7d\u5668\u5728\u8fd9\u4e2a\u8f6f\u4ef6\u7684\u5b89\u88c5\u76ee\u5f55\u5185\u3002",
                Reasons = ["\u8def\u5f84\u4f4d\u4e8e\u5b89\u88c5\u76ee\u5f55\u4e0b\u3002"]
            };
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return Blocked(
                OfficialUninstallCommandTrustDecision.BlockedInvalidPath,
                "\u65e0\u6cd5\u89e3\u6790\u5378\u8f7d\u5668\u8def\u5f84\u3002",
                "\u8def\u5f84\u683c\u5f0f\u65e0\u6548\u3002");
        }
    }

    private static OfficialUninstallCommandTrustResult EvaluateWindowsInstaller(string? arguments)
    {
        var safeArguments = arguments ?? string.Empty;
        if (WindowsInstallerSilentFlagPattern.IsMatch(safeArguments))
        {
            return Blocked(
                OfficialUninstallCommandTrustDecision.BlockedSilentWindowsInstaller,
                "\u8fd9\u662f\u9759\u9ed8\u6216\u7b80\u5316\u754c\u9762\u7684 MSI \u5378\u8f7d\u547d\u4ee4\uff0c\u5df2\u963b\u6b62\u3002",
                "\u9759\u9ed8 MSI \u53c2\u6570\u4f1a\u8df3\u8fc7\u6b63\u5e38\u5378\u8f7d\u786e\u8ba4\u754c\u9762\u3002");
        }

        if (!WindowsInstallerProductUninstallPattern.IsMatch(safeArguments))
        {
            return Blocked(
                OfficialUninstallCommandTrustDecision.BlockedUnsafeWindowsInstallerCommand,
                "\u8fd9\u4e0d\u662f\u7b80\u5355\u7684 MSI \u4ea7\u54c1\u5378\u8f7d\u547d\u4ee4\uff0c\u5df2\u963b\u6b62\u3002",
                "\u53ea\u5141\u8bb8\u9700\u8981\u7528\u6237\u786e\u8ba4\u7684\u4ea7\u54c1\u4ee3\u7801\u5378\u8f7d\u547d\u4ee4\u3002");
        }

        return new OfficialUninstallCommandTrustResult
        {
            Decision = OfficialUninstallCommandTrustDecision.TrustedWindowsInstaller,
            Summary = "\u8fd9\u662f\u4ea4\u4e92\u5f0f Windows Installer \u4ea7\u54c1\u5378\u8f7d\u547d\u4ee4\u3002",
            Reasons = ["msiexec \u662f Windows Installer \u4e3b\u673a\uff0c\u53c2\u6570\u8981\u6c42\u5378\u8f7d\u4ea7\u54c1\u3002"]
        };
    }

    private static bool IsWindowsInstallerExecutable(string executablePath, string executableFullPath)
    {
        var fileName = Path.GetFileName(executableFullPath);
        if (!string.Equals(fileName, "msiexec.exe", StringComparison.OrdinalIgnoreCase))
            return false;

        if (string.Equals(executablePath.Trim(), "msiexec.exe", StringComparison.OrdinalIgnoreCase))
            return true;

        var windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (!string.IsNullOrWhiteSpace(windowsDirectory))
        {
            if (IsInsideDirectory(Path.Combine(windowsDirectory, "System32"), executableFullPath) ||
                IsInsideDirectory(Path.Combine(windowsDirectory, "SysWOW64"), executableFullPath))
            {
                return true;
            }
        }

        return executableFullPath.EndsWith(
                @"\Windows\System32\msiexec.exe",
                StringComparison.OrdinalIgnoreCase) ||
            executableFullPath.EndsWith(
                @"\Windows\SysWOW64\msiexec.exe",
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasPublisherSignatureMatch(string? expectedPublisher, string? executableSignatureSubject)
    {
        var publisher = NormalizeTrustText(expectedPublisher);
        var signature = NormalizeTrustText(executableSignatureSubject);

        return publisher.Length >= 4 &&
            signature.Length >= publisher.Length &&
            signature.Contains(publisher, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeTrustText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return new string(value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
    }

    private static OfficialUninstallCommandTrustResult Blocked(
        OfficialUninstallCommandTrustDecision decision,
        string summary,
        string reason) =>
        new()
        {
            Decision = decision,
            Summary = summary,
            Reasons = [reason]
        };

    private static bool IsInsideDirectory(string directory, string path)
    {
        var root = Path.GetFullPath(directory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var child = Path.GetFullPath(path);
        return child.StartsWith(root, StringComparison.OrdinalIgnoreCase);
    }
}
