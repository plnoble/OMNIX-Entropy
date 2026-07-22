namespace Css.Core.Apps;

internal static class ApplicationOwnershipSummaryFormatter
{
    public static string Create(
        int ordinaryCount,
        int systemCount,
        int ownershipPendingCount)
    {
        var readOnlyParts = new List<string>();
        if (systemCount > 0)
            readOnlyParts.Add($"系统组件 {systemCount} 个");
        if (ownershipPendingCount > 0)
            readOnlyParts.Add($"归属待确认 {ownershipPendingCount} 个");

        var ordinary = $"普通应用 {ordinaryCount} 个";
        return readOnlyParts.Count == 0
            ? ordinary
            : $"{ordinary}；另有{string.Join("、", readOnlyParts)}，仅供查看";
    }
}
