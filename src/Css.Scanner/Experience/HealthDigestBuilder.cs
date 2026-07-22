using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Css.Core.Apps;
using Css.Core.Recommendations;
using Css.Core.Software;
using Css.Scanner.Disk;

namespace Css.Scanner.Experience;

public static class HealthDigestBuilder
{
    private static readonly Regex LocalPathPattern = new(
        @"(?:[A-Za-z]:\\|\\\\)[^\s，。；,;]*",
        RegexOptions.Compiled);

    public static HealthDigest Create(
        string driveRoot,
        ScanSnapshot snapshot,
        HealthCheckSummary health,
        IReadOnlyList<SoftwareProfile> profiles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(driveRoot);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(health);
        ArgumentNullException.ThrowIfNull(profiles);

        var cleanCount = health.KeyFindings.Count(item =>
            HealthFindingRiskPolicy.IsLowRiskClean(item.Action, item.Risk));
        var higherRiskCleanCount = health.KeyFindings.Count(item =>
            HealthFindingRiskPolicy.IsHigherRiskClean(item.Action, item.Risk));
        var growthCount = health.KeyFindings.Count(item =>
            item.Kind == HealthFindingKind.SustainedGrowth);
        var personalCount = health.KeyFindings.Count(item =>
            item.Kind == HealthFindingKind.PersonalStorage);
        var cDriveOwnership = CDriveApplicationOwnershipCatalog.Create(profiles);
        var disk = health.Dimensions.FirstOrDefault(item =>
            item.Name.Contains("磁盘", StringComparison.OrdinalIgnoreCase));
        var headline = health.OverallScore switch
        {
            >= 85 => "电脑状态良好",
            >= 70 => "有一些可以优化的地方",
            _ => "有几项问题需要优先看看"
        };
        var diskText = disk is null ? "磁盘摘要不完整" : Sanitize(disk.Result, 260);

        return new HealthDigest
        {
            ScanIdentity = CreateIdentity(driveRoot, snapshot.CapturedAt),
            CapturedAt = snapshot.CapturedAt,
            OverallScore = health.OverallScore,
            Headline = headline,
            Summary = $"{diskText}；低风险清理 {cleanCount} 项，风险偏高清理 {higherRiskCleanCount} 项，持续增长 {growthCount} 项，个人文件候选 {personalCount} 项；C 盘应用线索：{cDriveOwnership.BeginnerSummary}。",
            KeyFindings = health.KeyFindings
                .Select(item => Sanitize(item.Text, 600))
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Take(5)
                .ToArray()
        };
    }

    private static string CreateIdentity(string driveRoot, DateTimeOffset capturedAt)
    {
        var input = driveRoot.Trim().ToUpperInvariant()
            + "|" + capturedAt.ToUnixTimeMilliseconds();
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }

    private static string Sanitize(string? value, int maximumLength)
    {
        var text = LocalPathPattern.Replace(value ?? string.Empty, "某个本机位置").Trim();
        return text.Length <= maximumLength ? text : text[..maximumLength] + "...";
    }
}
