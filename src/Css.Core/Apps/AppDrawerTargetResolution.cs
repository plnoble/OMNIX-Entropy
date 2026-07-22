using Css.Core.Software;

namespace Css.Core.Apps;

public enum AppDrawerTargetStatus
{
    NoTarget,
    Found,
    NotFound,
    Ambiguous,
    InventoryUnavailable
}

public sealed class AppDrawerTargetResolution
{
    public required AppDrawerTargetStatus Status { get; init; }
    public required string Headline { get; init; }
    public required string Explanation { get; init; }
    public required string SafetyBoundary { get; init; }
    public SoftwareProfile? Profile { get; init; }
    public bool CanOpen => Status == AppDrawerTargetStatus.Found && Profile is not null;
}

public static class AppDrawerTargetResolver
{
    public static AppDrawerTargetResolution Resolve(
        string? targetAppName,
        IReadOnlyList<SoftwareProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        var normalizedTarget = Normalize(targetAppName);
        if (normalizedTarget is null)
        {
            return Result(
                AppDrawerTargetStatus.NoTarget,
                "这条发现没有唯一对应的应用",
                "它可能来自系统区域、个人文件或多个软件共用的位置，请继续查看 C 盘详情。",
                null);
        }

        var matches = profiles
            .Where(profile => string.Equals(
                Normalize(profile.Name),
                normalizedTarget,
                StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .ToArray();
        if (matches.Length == 0)
        {
            return Result(
                AppDrawerTargetStatus.NotFound,
                "当前应用列表里没有找到它",
                "软件可能已经卸载，或者应用画像还没有更新。可以重新扫描应用，但不能根据目录名猜测。",
                null);
        }

        if (matches.Length > 1)
        {
            return Result(
                AppDrawerTargetStatus.Ambiguous,
                "发现了多个同名应用",
                "OMNIX-Entropy 无法证明增长属于其中哪一个，请在应用管理页手动选择，不会替你猜。",
                null);
        }

        return Result(
            AppDrawerTargetStatus.Found,
            "已经找到对应应用",
            $"将打开 {matches[0].Name} 的应用抽屉，只展示结论和处理预案。",
            matches[0]);
    }

    public static AppDrawerTargetResolution InventoryUnavailable() =>
        Result(
            AppDrawerTargetStatus.InventoryUnavailable,
            "应用画像暂时不可用",
            "只读应用扫描没有完成，因此无法安全定位对应应用。请稍后在应用管理页重新扫描。",
            null);

    private static AppDrawerTargetResolution Result(
        AppDrawerTargetStatus status,
        string headline,
        string explanation,
        SoftwareProfile? profile) =>
        new()
        {
            Status = status,
            Headline = headline,
            Explanation = explanation,
            SafetyBoundary = "这只是 OMNIX-Entropy 内部导航，不会卸载、迁移、清理缓存或修改自启动。",
            Profile = profile
        };

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var trimmed = value.Trim();
        return trimmed.Length <= 256 ? trimmed : null;
    }
}
