using Css.Core.Software;

namespace Css.Rules.Winapp2;

public sealed record Winapp2RuleActivationInput
{
    public string? SourceName { get; init; }
    public string? SourceUriText { get; init; }
    public string? Version { get; init; }
    public string? LicenseName { get; init; }
    public string? LicenseUriText { get; init; }
    public string? Sha256 { get; init; }
    public bool UserConfirmedActivation { get; init; }
    public bool UserAcceptedLicense { get; init; }
}

public sealed class Winapp2RuleActivationRequest
{
    public Winapp2RulePackDescriptor? Descriptor { get; init; }
    public Winapp2RulePackActivationConsent? Consent { get; init; }
    public required IReadOnlyList<string> MissingRequirements { get; init; }
    public bool CanActivate => Descriptor is not null && Consent is not null && MissingRequirements.Count == 0;
    public bool IsExecutionAuthorized => false;
}

public static class Winapp2RuleActivationRequestBuilder
{
    public static Winapp2RuleActivationRequest Build(Winapp2RuleActivationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var missing = new List<string>();
        RequireText(input.SourceName, "请填写规则来源名称。", missing);
        RequireText(input.Version, "请填写规则版本。", missing);
        RequireText(input.LicenseName, "请填写许可证名称。", missing);
        var sourceUri = HttpsUri(input.SourceUriText, "规则来源必须是 HTTPS 地址。", missing);
        var licenseUri = HttpsUri(input.LicenseUriText, "许可证必须是 HTTPS 地址。", missing);
        var sha256 = input.Sha256?.Trim().ToUpperInvariant();
        if (!Winapp2RulePreferenceKey.IsValid(sha256))
            missing.Add("请提供 64 位 SHA-256 校验值。");
        if (!input.UserAcceptedLicense)
            missing.Add("请先确认已阅读并接受许可证。" );
        if (!input.UserConfirmedActivation)
            missing.Add("请确认要启用这份规则。" );

        if (missing.Count > 0)
        {
            return new Winapp2RuleActivationRequest
            {
                MissingRequirements = missing.Distinct(StringComparer.Ordinal).ToArray()
            };
        }

        var descriptor = new Winapp2RulePackDescriptor
        {
            SourceName = input.SourceName!.Trim(),
            SourceUri = sourceUri!,
            Version = input.Version!.Trim(),
            LicenseName = input.LicenseName!.Trim(),
            LicenseUri = licenseUri!,
            ExpectedSha256 = sha256!
        };
        return new Winapp2RuleActivationRequest
        {
            Descriptor = descriptor,
            Consent = new Winapp2RulePackActivationConsent
            {
                UserConfirmedActivation = true,
                UserAcceptedLicense = true,
                ReviewedSourceUri = descriptor.SourceUri,
                ReviewedLicenseUri = descriptor.LicenseUri,
                ReviewedVersion = descriptor.Version,
                ReviewedSha256 = descriptor.ExpectedSha256
            },
            MissingRequirements = []
        };
    }

    private static void RequireText(string? value, string message, ICollection<string> missing)
    {
        if (string.IsNullOrWhiteSpace(value)) missing.Add(message);
    }

    private static Uri? HttpsUri(string? value, string message, ICollection<string> missing)
    {
        if (Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
            && uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return uri;
        }
        missing.Add(message);
        return null;
    }
}

public sealed class Winapp2RulePreviewRow
{
    public required string SoftwareName { get; init; }
    public required string RuleName { get; init; }
    public required string RuleKey { get; init; }
    public required string VisibleText { get; init; }
    public required string SizeSummary { get; init; }
    public long SizeBytes { get; init; }
    public int FileCount { get; init; }
    public bool IsLowerBound { get; init; }
    public required CommunityRuleCacheEvidence Evidence { get; init; }
    public required IReadOnlyList<string> TechnicalDetails { get; init; }
    public bool IsExecutionAuthorized => false;
}

public sealed class Winapp2IgnoredRuleRow
{
    public required string RuleKey { get; init; }
    public required string VisibleText { get; init; }
    public required string RuleName { get; init; }
    public bool IsExecutionAuthorized => false;
}

public sealed class Winapp2RuleCenterViewModel
{
    public required string Title { get; init; }
    public required string StatusHeadline { get; init; }
    public required string StatusSummary { get; init; }
    public required string SourceSummary { get; init; }
    public required string LicenseSummary { get; init; }
    public required string VersionSummary { get; init; }
    public string? ActiveVersion { get; init; }
    public required string RollbackSummary { get; init; }
    public bool CanRollback { get; init; }
    public required string PreviewSummary { get; init; }
    public required IReadOnlyList<Winapp2RulePreviewRow> PreviewRows { get; init; }
    public required IReadOnlyList<Winapp2IgnoredRuleRow> IgnoredRules { get; init; }
    public bool IsExecutionAuthorized => false;
}

public static class Winapp2RuleCenterPresenter
{
    public static Winapp2RuleCenterViewModel Create(
        Winapp2RulePackStatus? status,
        IReadOnlyList<SoftwareProfile> profiles,
        Winapp2RulePreferences preferences,
        string? loadError = null)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        ArgumentNullException.ThrowIfNull(preferences);
        if (status is null)
        {
            return new Winapp2RuleCenterViewModel
            {
                Title = "扩展规则",
                StatusHeadline = string.IsNullOrWhiteSpace(loadError) ? "扩展规则未启用" : "扩展规则暂时不可用",
                StatusSummary = string.IsNullOrWhiteSpace(loadError)
                    ? "仍会使用 OMNIX 的基础扫描；未启用不影响基础扫描。"
                    : "基础扫描仍然可用；请检查规则状态后再试。",
                SourceSummary = "尚未选择规则来源。",
                LicenseSummary = "启用前必须查看来源和许可证。",
                VersionSummary = "暂无活动版本。",
                RollbackSummary = "没有可回退的版本。",
                CanRollback = false,
                PreviewSummary = "启用并重新扫描后，这里才会显示只读发现。",
                PreviewRows = [],
                IgnoredRules = IgnoredRows(preferences),
                ActiveVersion = null
            };
        }

        var descriptor = status.ActiveDescriptor;
        var rows = profiles
            .SelectMany(profile => profile.CommunityCacheEvidence.Select(evidence => (profile, evidence)))
            .Where(item => item.evidence.RulePackSha256.Equals(
                descriptor.ExpectedSha256,
                StringComparison.OrdinalIgnoreCase))
            .Where(item => !preferences.IsIgnored(item.evidence))
            .Select(item => PreviewRow(item.profile, item.evidence))
            .OrderByDescending(row => row.SizeBytes)
            .ThenBy(row => row.SoftwareName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(row => row.RuleName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
        var largest = rows.Length == 0 ? 0 : rows.Max(row => row.SizeBytes);
        var eligibleCount = rows.Count(row =>
            row.Evidence.CandidateAssessment?.Disposition
                == CommunityRuleCandidateDisposition.EligibleForSafePreview);
        var previewSummary = rows.Length == 0
            ? "当前扫描没有额外只读发现；这不代表电脑没有缓存。"
            : $"发现 {rows.Length} 条只读发现；最大一条至少 {FormatBytes(largest)}。"
                + (eligibleCount > 0
                    ? $"其中 {eligibleCount} 条通过第一轮筛选，可进入安全预演。"
                    : "目前没有发现通过第一轮安全筛选。")
                + "规则可能重叠，不会把它们相加，也不会直接清理。";

        return new Winapp2RuleCenterViewModel
        {
            Title = "扩展规则",
            StatusHeadline = "扩展规则已启用",
            StatusSummary = "它只扩大缓存发现范围，不会直接删除文件或修改注册表。",
            SourceSummary = $"来源：{descriptor.SourceName}（{descriptor.SourceUri.Host}）",
            LicenseSummary = $"许可证：{descriptor.LicenseName}（{descriptor.LicenseUri.Host}）",
            VersionSummary = $"当前版本：{descriptor.Version}；启用于 {status.ActivatedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm}",
            ActiveVersion = descriptor.Version,
            RollbackSummary = status.CanRollback
                ? $"可回退到上一版本 {status.PreviousDescriptor!.Version}；回退前会再次确认当前版本。"
                : "还没有可回退的上一版本。",
            CanRollback = status.CanRollback,
            PreviewSummary = previewSummary,
            PreviewRows = rows,
            IgnoredRules = IgnoredRows(preferences)
        };
    }

    private static Winapp2RulePreviewRow PreviewRow(
        SoftwareProfile profile,
        CommunityRuleCacheEvidence evidence)
    {
        var size = "至少 " + FormatBytes(evidence.SizeBytes);
        var stale = evidence.StaleSizeBytes > 0
            ? $"；{evidence.StaleThresholdDays} 天以上至少 {FormatBytes(evidence.StaleSizeBytes)}"
            : string.Empty;
        var decision = evidence.CandidateAssessment?.Summary ?? "只读预览";
        return new Winapp2RulePreviewRow
        {
            SoftwareName = profile.Name,
            RuleName = evidence.RuleName,
            RuleKey = Winapp2RulePreferenceKey.Create(evidence),
            VisibleText = $"{profile.Name}：{size}{stale}；{decision}",
            SizeSummary = size + stale,
            SizeBytes = evidence.SizeBytes,
            FileCount = evidence.FileCount,
            IsLowerBound = evidence.IsSizeLowerBound,
            Evidence = evidence,
            TechnicalDetails =
            [
                $"Rule: {evidence.RuleName}",
                $"Pack: {evidence.RulePackSource}; {evidence.RulePackVersion}; SHA-256 {evidence.RulePackSha256}",
                $"Files: {evidence.FileCount}; bytes {evidence.SizeBytes}; lower bound {evidence.IsSizeLowerBound}",
                $"Candidate set complete: {evidence.CandidateFilesComplete}; exact files {evidence.CandidateFiles.Count}; registry targets {evidence.RegistryTargetCount}; remove-self {evidence.IncludesRemoveSelf}",
                $"Candidate assessment: {evidence.CandidateAssessment?.Disposition}; reasons {string.Join(",", evidence.CandidateAssessment?.Reasons ?? [])}; execution authorized false",
                .. evidence.SamplePaths.Take(3).Select(path => "Sample: " + path)
            ]
        };
    }

    private static IReadOnlyList<Winapp2IgnoredRuleRow> IgnoredRows(Winapp2RulePreferences preferences) =>
        preferences.IgnoredRules
            .Select(item => new Winapp2IgnoredRuleRow
            {
                RuleKey = item.RuleKey,
                RuleName = item.RuleName,
                VisibleText = $"{item.RuleName}（{item.RulePackSource} {item.RulePackVersion}）"
            })
            .ToArray();

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = Math.Max(0, bytes);
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return unit == 0 ? $"{Math.Max(0, bytes)} B" : $"{value:0.0} {units[unit]}";
    }
}
