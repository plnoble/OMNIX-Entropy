using System.Collections.Generic;

namespace Css.Core.Apps;

public sealed class AppDrawerActionPreviewState
{
    public required bool CachePreviewVisible { get; init; }
    public required bool StartupPreviewVisible { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<string> Lines { get; init; }
    public required bool CanExecuteDirectly { get; init; }
    public required string StatusText { get; init; }
}

public static class AppDrawerActionPreviewPresenter
{
    public static AppDrawerActionPreviewState NoSelectionForCacheCleanup() =>
        NoSelection("\u8bf7\u5148\u9009\u62e9\u4e00\u4e2a\u5e94\u7528\uff0c\u518d\u67e5\u770b\u7f13\u5b58\u6e05\u7406\u65b9\u6848\u3002");

    public static AppDrawerActionPreviewState NoSelectionForStartupControl() =>
        NoSelection("\u8bf7\u5148\u9009\u62e9\u4e00\u4e2a\u5e94\u7528\uff0c\u518d\u67e5\u770b\u81ea\u542f\u52a8\u7ba1\u63a7\u65b9\u6848\u3002");

    public static AppDrawerActionPreviewState ShowCacheCleanup(AppDrawerViewModel drawer) =>
        new()
        {
            CachePreviewVisible = true,
            StartupPreviewVisible = false,
            Summary = drawer.CacheCleanupSummary,
            Lines = drawer.CacheCleanupPreviewLines,
            CanExecuteDirectly = drawer.CacheCleanupCanExecuteDirectly,
            StatusText = drawer.CacheCleanupCanExecuteDirectly
                ? "\u5df2\u751f\u6210\u7f13\u5b58\u6e05\u7406\u65b9\u6848\uff1b\u4e0b\u4e00\u6b65\u4ecd\u9700\u8981\u4f60\u786e\u8ba4\u548c\u5b89\u5168\u7ba1\u7ebf\u6821\u9a8c\u3002"
                : "\u5df2\u751f\u6210\u7f13\u5b58\u6e05\u7406\u9884\u89c8\uff1b\u6ca1\u6709\u6267\u884c\u6e05\u7406\uff0c\u6ca1\u6709\u5220\u9664\u6587\u4ef6\u3002"
        };

    public static AppDrawerActionPreviewState ShowStartupControl(AppDrawerViewModel drawer) =>
        new()
        {
            CachePreviewVisible = false,
            StartupPreviewVisible = true,
            Summary = drawer.StartupControlSummary,
            Lines = drawer.StartupControlPreviewLines,
            CanExecuteDirectly = drawer.StartupControlCanExecuteDirectly,
            StatusText = drawer.StartupControlCanExecuteDirectly
                ? "\u5df2\u751f\u6210\u81ea\u542f\u52a8\u7ba1\u63a7\u65b9\u6848\uff1b\u4e0b\u4e00\u6b65\u4ecd\u9700\u8981\u5feb\u7167\u3001\u786e\u8ba4\u548c\u56de\u6eda\u4fe1\u606f\u3002"
                : "\u5df2\u751f\u6210\u81ea\u542f\u52a8\u9884\u89c8\uff1b\u6ca1\u6709\u7981\u7528\u542f\u52a8\u9879\u3001\u670d\u52a1\u6216\u8ba1\u5212\u4efb\u52a1\u3002"
        };

    private static AppDrawerActionPreviewState NoSelection(string statusText) =>
        new()
        {
            CachePreviewVisible = false,
            StartupPreviewVisible = false,
            Summary = "",
            Lines = [],
            CanExecuteDirectly = false,
            StatusText = statusText
        };
}
