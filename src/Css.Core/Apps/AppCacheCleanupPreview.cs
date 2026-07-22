using System.Collections.Generic;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class AppCacheCleanupPreviewViewModel
{
    public required string Summary { get; init; }
    public required IReadOnlyList<string> Lines { get; init; }
    public required bool CanExecuteDirectly { get; init; }
}

public static class AppCacheCleanupPreviewPresenter
{
    public static AppCacheCleanupPreviewViewModel Create(SoftwareProfile profile)
    {
        var hasCacheCandidates = profile.CachePaths.Count > 0 || profile.CacheSizeBytes > 0;
        if (!hasCacheCandidates)
        {
            return new AppCacheCleanupPreviewViewModel
            {
                Summary = "\u6682\u672a\u53d1\u73b0\u660e\u786e\u7684\u7f13\u5b58\u5360\u7528\uff0c\u5efa\u8bae\u5148\u89c2\u5bdf\u6216\u91cd\u65b0\u626b\u63cf\u3002",
                Lines =
                [
                    "\u6682\u65f6\u4e0d\u751f\u6210\u6e05\u7406\u64cd\u4f5c\uff1a\u6ca1\u6709\u8db3\u591f\u8bc1\u636e\u8bf4\u660e\u54ea\u4e9b\u662f\u53ef\u6e05\u7406\u7f13\u5b58\u3002",
                    "\u53ef\u4ee5\u5148\u70b9\u51fb\u201c\u6280\u672f\u8be6\u60c5\u201d\u67e5\u770b\u540e\u53f0\u8bc1\u636e\uff0c\u6216\u7b49\u4e0b\u6b21\u626b\u63cf\u540e\u518d\u5224\u65ad\u3002",
                    "\u8fd9\u91cc\u4e0d\u4f1a\u76f4\u63a5\u5220\u9664\u6587\u4ef6\uff0c\u4e5f\u4e0d\u4f1a\u6539\u7cfb\u7edf\u8bbe\u7f6e\u3002"
                ],
                CanExecuteDirectly = false
            };
        }

        var impact = profile.CacheSizeBytes > 0 ? FormatBytes(profile.CacheSizeBytes) : "\u672a\u77e5\u5927\u5c0f";
        var countText = profile.CachePaths.Count > 0
            ? profile.CachePaths.Count + " \u4e2a\u7f13\u5b58\u4f4d\u7f6e"
            : "\u7f13\u5b58\u4f4d\u7f6e\u5f85\u590d\u6838";

        return new AppCacheCleanupPreviewViewModel
        {
            Summary = "\u53d1\u73b0\u53ef\u68c0\u67e5\u7f13\u5b58\uff1a" + countText + "\uff0c\u9884\u8ba1\u5f71\u54cd " + impact + "\u3002",
            Lines =
            [
                "\u53ea\u751f\u6210\u65b9\u6848\uff1aAgent \u5148\u533a\u5206\u7f13\u5b58\u3001\u65e5\u5fd7\u548c\u4e0d\u5efa\u8bae\u52a8\u7684\u6570\u636e\u3002",
                "\u9700\u8981\u4f60\u518d\u786e\u8ba4\uff1a\u4f4e\u98ce\u9669\u9879\u76ee\u624d\u80fd\u8fdb\u5165\u672c\u5730\u5b89\u5168\u7ba1\u7ebf\u3002",
                "\u771f\u8981\u5904\u7406\u65f6\uff1a\u9ed8\u8ba4\u5148\u79fb\u5230 OMNIX-Entropy \u9694\u79bb\u533a\uff0c\u4e0d\u662f\u6c38\u4e45\u5220\u9664\u3002",
                "\u53ef\u4ee5\u540e\u6094\uff1a\u540e\u6094\u836f\u4e2d\u5fc3\u4f1a\u663e\u793a\u65f6\u95f4\u7ebf\u548c\u8fd8\u539f\u5165\u53e3\u3002",
                "\u4e0d\u5728\u4e3b\u754c\u9762\u5806\u8def\u5f84\uff1a\u6280\u672f\u660e\u7ec6\u53ea\u5728\u4f60\u70b9\u51fb\u201c\u6280\u672f\u8be6\u60c5\u201d\u540e\u5c55\u5f00\u3002"
            ],
            CanExecuteDirectly = false
        };
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0 ? bytes + " B" : $"{value:0.0} {units[unit]}";
    }
}
