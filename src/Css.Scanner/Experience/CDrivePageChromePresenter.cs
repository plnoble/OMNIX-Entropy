using System;

namespace Css.Scanner.Experience;

public sealed class CDrivePageChromeViewModel
{
    public required string ScanTargetLabel { get; init; }
    public required string ScanTargetHint { get; init; }
    public required bool IsDrivePathEditable { get; init; }
    public required string TechnicalReportToggleText { get; init; }
    public required string TechnicalReportHint { get; init; }
    public required bool IsTechnicalReportVisibleByDefault { get; init; }
}

public static class CDrivePageChromePresenter
{
    public static CDrivePageChromeViewModel Create(string driveRoot)
    {
        var driveLetter = ResolveDriveLetter(driveRoot);

        return new CDrivePageChromeViewModel
        {
            ScanTargetLabel = driveLetter.Equals("C", StringComparison.OrdinalIgnoreCase)
                ? "\u7cfb\u7edf\u76d8 C \u76d8"
                : $"{driveLetter} \u76d8",
            ScanTargetHint = "\u81ea\u52a8\u8bc6\u522b\u672c\u673a\u76d8\u7b26\uff1bV1 \u9ed8\u8ba4\u4f53\u68c0\u7cfb\u7edf\u76d8\uff0c\u4e0d\u9700\u8981\u624b\u52a8\u8f93\u5165\u8def\u5f84\u3002",
            IsDrivePathEditable = false,
            TechnicalReportToggleText = "\u663e\u793a\u6280\u672f\u62a5\u544a",
            TechnicalReportHint = "\u6280\u672f\u62a5\u544a\u53ea\u7ed9\u8fdb\u9636\u68c0\u67e5\u7528\uff0c\u9ed8\u8ba4\u6536\u8d77\uff0c\u4e0d\u5f71\u54cd Agent \u7ed9\u51fa\u5904\u7406\u5efa\u8bae\u3002",
            IsTechnicalReportVisibleByDefault = false
        };
    }

    private static string ResolveDriveLetter(string driveRoot)
    {
        if (!string.IsNullOrWhiteSpace(driveRoot) && driveRoot.Length >= 1 && char.IsLetter(driveRoot[0]))
            return char.ToUpperInvariant(driveRoot[0]).ToString();

        return "C";
    }
}
