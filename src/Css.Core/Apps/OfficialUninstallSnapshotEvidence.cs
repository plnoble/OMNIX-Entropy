using System;
using System.Collections.Generic;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class OfficialUninstallSnapshotEvidence
{
    public required string SnapshotId { get; init; }
    public required string ManifestPath { get; init; }
    public required string SoftwareName { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required string Sha256 { get; init; }
    public required bool CanRestoreApplication { get; init; }
}

public sealed class OfficialUninstallSnapshotValidationResult
{
    public required bool IsValid { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }
}

public static class OfficialUninstallSnapshotEvidenceValidator
{
    private static readonly TimeSpan MaximumAge = TimeSpan.FromHours(1);
    private static readonly TimeSpan MaximumFutureSkew = TimeSpan.FromMinutes(5);

    public static OfficialUninstallSnapshotValidationResult Validate(
        SoftwareProfile profile,
        OfficialUninstallSnapshotEvidence? evidence,
        Func<string, string?> hashResolver,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(hashResolver);

        var reasons = new List<string>();
        if (evidence is null)
        {
            reasons.Add("\u9700\u8981\u771f\u5b9e\u7684\u5378\u8f7d\u524d\u5feb\u7167\u8bc1\u636e\uff0c\u4e0d\u80fd\u53ea\u586b\u5199\u4e00\u4e2a\u5feb\u7167\u7f16\u53f7\u3002");
            return Invalid(reasons);
        }

        if (string.IsNullOrWhiteSpace(evidence.SnapshotId)
            || string.IsNullOrWhiteSpace(evidence.ManifestPath)
            || string.IsNullOrWhiteSpace(evidence.Sha256))
        {
            reasons.Add("\u5feb\u7167\u8bc1\u636e\u4e0d\u5b8c\u6574\u3002");
        }

        if (!string.Equals(
                profile.Name.Trim(),
                evidence.SoftwareName.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("\u5feb\u7167\u8bc1\u636e\u5c5e\u4e8e\u53e6\u4e00\u4e2a\u8f6f\u4ef6\u3002");
        }

        if (evidence.CanRestoreApplication)
            reasons.Add("\u8bc1\u636e\u5feb\u7167\u4e0d\u80fd\u58f0\u79f0\u5b83\u53ef\u4ee5\u6062\u590d\u5df2\u5378\u8f7d\u7684\u8f6f\u4ef6\u3002");

        var age = now.ToUniversalTime() - evidence.CreatedAtUtc.ToUniversalTime();
        if (age > MaximumAge || age < -MaximumFutureSkew)
            reasons.Add("\u5feb\u7167\u8bc1\u636e\u5df2\u8fc7\u671f\u6216\u65f6\u95f4\u5f02\u5e38\uff0c\u9700\u8981\u91cd\u65b0\u521b\u5efa\u3002");

        var actualHash = ResolveHash(hashResolver, evidence.ManifestPath);
        if (string.IsNullOrWhiteSpace(actualHash))
        {
            reasons.Add("\u5feb\u7167\u8bc1\u636e\u6587\u4ef6\u4e0d\u5b58\u5728\u6216\u65e0\u6cd5\u8bfb\u53d6\u3002");
        }
        else if (!string.Equals(actualHash, evidence.Sha256, StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("\u5feb\u7167\u8bc1\u636e\u54c8\u5e0c\u4e0d\u5339\u914d\uff0c\u6587\u4ef6\u53ef\u80fd\u5df2\u88ab\u4fee\u6539\u3002");
        }

        return reasons.Count == 0
            ? new OfficialUninstallSnapshotValidationResult { IsValid = true, Reasons = [] }
            : Invalid(reasons);
    }

    private static OfficialUninstallSnapshotValidationResult Invalid(IReadOnlyList<string> reasons) =>
        new() { IsValid = false, Reasons = reasons };

    private static string? ResolveHash(Func<string, string?> resolver, string path)
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
}
