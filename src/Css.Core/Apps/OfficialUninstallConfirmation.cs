using System.Collections.Generic;
using System.Linq;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class OfficialUninstallConfirmationViewModel
{
    public required string SoftwareName { get; init; }
    public required string ExecutablePath { get; init; }
    public required string ArgumentsLine { get; init; }
    public required string SafetySummary { get; init; }
    public required string PrimaryButtonText { get; init; }
    public bool CanRunOfficialUninstaller { get; init; }
    public bool RequiresSnapshot { get; init; }
    public bool RequiresPostUninstallScan { get; init; }
    public required OfficialUninstallExecutionGateResult ExecutionGate { get; init; }
    public required OfficialUninstallPreflightChecklistViewModel PreflightChecklist { get; init; }
    public IReadOnlyList<string> ReadinessWarnings { get; init; } = [];
    public IReadOnlyList<string> Checklist { get; init; } = [];
}

public static class OfficialUninstallConfirmationBuilder
{
    private const string NotFound = "\u672a\u53d1\u73b0";
    private const string None = "\u65e0";

    public static OfficialUninstallConfirmationViewModel Create(SoftwareProfile profile)
    {
        var parsed = ParseCommand(profile.UninstallCommand);
        var warnings = new List<string>();
        if (parsed.ExecutablePath == NotFound)
            warnings.Add("\u672a\u53d1\u73b0\u5b98\u65b9\u5378\u8f7d\u547d\u4ee4\uff0cOMNIX-Entropy \u4e0d\u4f1a\u66ff\u4f60\u731c\u6d4b\u5378\u8f7d\u65b9\u5f0f\u3002");

        if (profile.RunningProcesses.Count > 0)
            warnings.Add("\u68c0\u6d4b\u5230\u8fd0\u884c\u4e2d\u8fdb\u7a0b\uff0c\u5efa\u8bae\u5148\u5173\u95ed\u8f6f\u4ef6\uff1a" + string.Join("\u3001", profile.RunningProcesses.Take(4)));

        if (profile.Services.Count > 0)
            warnings.Add("\u68c0\u6d4b\u5230\u540e\u53f0\u670d\u52a1\uff0c\u5378\u8f7d\u524d\u9700\u786e\u8ba4\u670d\u52a1\u72b6\u6001\uff1a" + string.Join("\u3001", profile.Services.Take(4)));

        if (profile.ScheduledTasks.Count > 0)
            warnings.Add("\u68c0\u6d4b\u5230\u8ba1\u5212\u4efb\u52a1\uff0c\u5378\u8f7d\u540e\u4f1a\u91cd\u65b0\u626b\u63cf\u662f\u5426\u6b8b\u7559\uff1a" + string.Join("\u3001", profile.ScheduledTasks.Take(4)));

        var preflightChecklist = OfficialUninstallPreflightChecklistBuilder.Create(
            profile,
            new OfficialUninstallExecutionReadiness
            {
                FeatureEnabled = false
            },
            _ => false);

        return new OfficialUninstallConfirmationViewModel
        {
            SoftwareName = profile.Name,
            ExecutablePath = parsed.ExecutablePath,
            ArgumentsLine = parsed.ArgumentsLine,
            SafetySummary = parsed.ExecutablePath == NotFound
                ? "\u5f53\u524d\u53ea\u5c55\u793a\u98ce\u9669\u548c\u4e0b\u4e00\u6b65\u5efa\u8bae\uff0c\u4e0d\u4f1a\u81ea\u52a8\u8fd0\u884c\u4efb\u4f55\u547d\u4ee4\u3002"
                : "\u8fd9\u662f\u5b98\u65b9\u5378\u8f7d\u5668\u7684\u51c6\u5907\u6458\u8981\uff1b\u6b64\u9875\u4e0d\u4f1a\u76f4\u63a5\u8fd0\u884c\uff0c\u9700\u8981\u5feb\u7167\u3001\u6062\u590d\u8bc1\u636e\u548c\u6700\u7ec8\u786e\u8ba4\u3002",
            PrimaryButtonText = parsed.ExecutablePath == NotFound ? "\u4e0d\u80fd\u7ee7\u7eed" : "\u5148\u5b8c\u6210\u6062\u590d\u51c6\u5907",
            CanRunOfficialUninstaller = false,
            RequiresSnapshot = true,
            RequiresPostUninstallScan = true,
            ExecutionGate = preflightChecklist.ExecutionGate,
            PreflightChecklist = preflightChecklist,
            ReadinessWarnings = warnings,
            Checklist =
            [
                "\u5173\u95ed\u8f6f\u4ef6\u548c\u76f8\u5173\u6258\u76d8\u7a97\u53e3\u3002",
                "\u786e\u8ba4\u5378\u8f7d\u524d\u5feb\u7167\u6216\u8fd8\u539f\u70b9\u72b6\u6001\u3002",
                "\u5378\u8f7d\u540e\u91cd\u65b0\u626b\u63cf\u8f6f\u4ef6\u753b\u50cf\u548c\u6b8b\u7559\u3002",
                "\u53ea\u628a\u4f4e\u98ce\u9669\u7f13\u5b58/\u65e5\u5fd7\u6b8b\u7559\u79fb\u52a8\u5230\u9694\u79bb\u533a\u3002"
            ]
        };
    }

    private static (string ExecutablePath, string ArgumentsLine) ParseCommand(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return (NotFound, None);

        var trimmed = command.Trim();
        if (trimmed.StartsWith('"'))
        {
            var closing = trimmed.IndexOf('"', 1);
            if (closing > 1)
            {
                var executable = trimmed[1..closing];
                var args = trimmed[(closing + 1)..].Trim();
                return (executable, string.IsNullOrWhiteSpace(args) ? None : args);
            }
        }

        var firstSpace = trimmed.IndexOf(' ');
        if (firstSpace < 0)
            return (trimmed, None);

        return (trimmed[..firstSpace], trimmed[(firstSpace + 1)..].Trim());
    }
}
