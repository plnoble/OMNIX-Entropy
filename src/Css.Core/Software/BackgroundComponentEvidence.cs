using System.Security.Cryptography;
using System.Text;

namespace Css.Core.Software;

public enum BackgroundComponentKind
{
    StartupEntry,
    Service,
    ScheduledTask
}

public enum BackgroundComponentActivationState
{
    Unknown,
    Enabled,
    Disabled,
    Automatic,
    Manual,
    Boot,
    System
}

public enum BackgroundComponentRuntimeState
{
    NotApplicable,
    Unknown,
    Running,
    Stopped
}

public enum BackgroundRollbackEvidenceRequirement
{
    OriginalRegistryValueTypeAndBytes,
    StartupApprovalState,
    ServiceConfiguration,
    ServiceRecoveryConfiguration,
    ScheduledTaskDefinition,
    AccessControl
}

public sealed class BackgroundComponentIdentity
{
    public required BackgroundComponentKind Kind { get; init; }
    public required string StableId { get; init; }
    public required string DisplayName { get; init; }
    public required string SourceLocator { get; init; }
}

public enum StartupApprovalEvidenceStatus
{
    NotObserved,
    Missing,
    PresentBinary,
    PresentUnsupportedType,
    Unreadable
}

/// <summary>
/// Presence and drift evidence for a Windows StartupApproved value. The binary
/// payload is fingerprinted and discarded because its state encoding is not a
/// public execution contract for this product.
/// </summary>
public sealed class StartupApprovalObservation
{
    public required string ApprovalKeyLocator { get; init; }
    public required string ValueName { get; init; }
    public required StartupApprovalEvidenceStatus Status { get; init; }
    public string? PayloadFingerprint { get; init; }
    public int PayloadLength { get; init; }
    public BackgroundComponentActivationState EffectiveActivationState =>
        BackgroundComponentActivationState.Unknown;
    public bool IsStateDecoded => false;
    public bool IsReadOnlyEvidence => true;
    public bool CanAuthorizeChange => false;
}

public static class StartupApprovalObservationFactory
{
    public static StartupApprovalObservation FromRegistryValue(
        string approvalKeyLocator,
        string valueName,
        object? value)
    {
        var key = Required(approvalKeyLocator, nameof(approvalKeyLocator));
        var name = Required(valueName, nameof(valueName));
        if (value is null)
            return Create(key, name, StartupApprovalEvidenceStatus.Missing);

        if (value is byte[] bytes)
        {
            return Create(
                key,
                name,
                StartupApprovalEvidenceStatus.PresentBinary,
                Convert.ToHexString(SHA256.HashData(bytes)),
                bytes.Length);
        }

        return Create(key, name, StartupApprovalEvidenceStatus.PresentUnsupportedType);
    }

    public static StartupApprovalObservation Unreadable(
        string approvalKeyLocator,
        string valueName) =>
        Create(
            Required(approvalKeyLocator, nameof(approvalKeyLocator)),
            Required(valueName, nameof(valueName)),
            StartupApprovalEvidenceStatus.Unreadable);

    private static StartupApprovalObservation Create(
        string key,
        string name,
        StartupApprovalEvidenceStatus status,
        string? fingerprint = null,
        int payloadLength = 0) =>
        new()
        {
            ApprovalKeyLocator = key,
            ValueName = name,
            Status = status,
            PayloadFingerprint = fingerprint,
            PayloadLength = payloadLength
        };

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A non-empty startup approval identity value is required.", parameterName);
        return value.Trim();
    }
}

/// <summary>
/// A read-only inventory observation. It intentionally does not contain enough
/// authority or original configuration material to restore or modify Windows.
/// </summary>
public sealed class BackgroundComponentObservation
{
    public required BackgroundComponentIdentity Identity { get; init; }
    public required DateTimeOffset ObservedAtUtc { get; init; }
    public required string ObservationFingerprint { get; init; }
    public BackgroundComponentActivationState ActivationState { get; init; }
    public BackgroundComponentRuntimeState RuntimeState { get; init; }
    public StartupApprovalObservation? StartupApproval { get; init; }
    public required IReadOnlyList<BackgroundRollbackEvidenceRequirement> RequiredRollbackEvidence { get; init; }
    public bool IsReadOnlyEvidence => true;
    public bool IsRollbackReady => false;
    public bool CanCreateChangeOperation => false;
}

public sealed class BackgroundComponentInventorySnapshot
{
    public required string SnapshotId { get; init; }
    public required string SnapshotFingerprint { get; init; }
    public required string SoftwareName { get; init; }
    public required DateTimeOffset ObservedAtUtc { get; init; }
    public required IReadOnlyList<BackgroundComponentObservation> Observations { get; init; }
    public bool IsReadOnlyEvidence => true;
    public bool IsRollbackReady => false;
    public bool CanCreateChangeOperation => false;
}

public static class BackgroundComponentInventorySnapshotBuilder
{
    public static BackgroundComponentInventorySnapshot Create(
        SoftwareProfile profile,
        DateTimeOffset? emptySnapshotObservedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var observations = (profile.BackgroundComponents ?? [])
            .OrderBy(item => item.Identity.StableId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var observedAtUtc = observations.Length == 0
            ? (emptySnapshotObservedAtUtc ?? DateTimeOffset.UtcNow).ToUniversalTime()
            : observations.Max(item => item.ObservedAtUtc).ToUniversalTime();
        var material = string.Join(
            "\n",
            profile.Name.Trim().ToUpperInvariant(),
            observedAtUtc.ToString("O"),
            string.Join("\n", observations.Select(item =>
                item.Identity.StableId + ":" + item.ObservationFingerprint)));
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material)));

        return new BackgroundComponentInventorySnapshot
        {
            SnapshotId = "background-observation-" + fingerprint[..16].ToLowerInvariant(),
            SnapshotFingerprint = fingerprint,
            SoftwareName = profile.Name,
            ObservedAtUtc = observedAtUtc,
            Observations = observations
        };
    }
}

public static class BackgroundComponentObservationFactory
{
    public static BackgroundComponentObservation Startup(
        string name,
        string sourceLocator,
        string command,
        DateTimeOffset observedAtUtc,
        StartupApprovalObservation? startupApproval = null) =>
        Create(
            BackgroundComponentKind.StartupEntry,
            name,
            sourceLocator,
            string.Join(
                "\n",
                command,
                StartupApprovalMaterial(startupApproval)),
            BackgroundComponentActivationState.Unknown,
            BackgroundComponentRuntimeState.NotApplicable,
            observedAtUtc,
            [
                BackgroundRollbackEvidenceRequirement.OriginalRegistryValueTypeAndBytes,
                BackgroundRollbackEvidenceRequirement.StartupApprovalState,
                BackgroundRollbackEvidenceRequirement.AccessControl
            ],
            startupApproval);

    public static BackgroundComponentObservation Service(
        string name,
        string sourceLocator,
        string pathName,
        string? startMode,
        string? runtimeState,
        DateTimeOffset observedAtUtc) =>
        Create(
            BackgroundComponentKind.Service,
            name,
            sourceLocator,
            string.Join("\n", pathName, startMode ?? "", runtimeState ?? ""),
            ParseServiceActivation(startMode),
            ParseServiceRuntime(runtimeState),
            observedAtUtc,
            [
                BackgroundRollbackEvidenceRequirement.ServiceConfiguration,
                BackgroundRollbackEvidenceRequirement.ServiceRecoveryConfiguration,
                BackgroundRollbackEvidenceRequirement.AccessControl
            ]);

    public static BackgroundComponentObservation ScheduledTask(
        string name,
        string sourceLocator,
        string actionPath,
        bool? isEnabled,
        DateTimeOffset observedAtUtc) =>
        Create(
            BackgroundComponentKind.ScheduledTask,
            name,
            sourceLocator,
            string.Join("\n", actionPath, isEnabled?.ToString() ?? ""),
            isEnabled switch
            {
                true => BackgroundComponentActivationState.Enabled,
                false => BackgroundComponentActivationState.Disabled,
                _ => BackgroundComponentActivationState.Unknown
            },
            BackgroundComponentRuntimeState.NotApplicable,
            observedAtUtc,
            [
                BackgroundRollbackEvidenceRequirement.ScheduledTaskDefinition,
                BackgroundRollbackEvidenceRequirement.AccessControl
            ]);

    private static BackgroundComponentObservation Create(
        BackgroundComponentKind kind,
        string name,
        string sourceLocator,
        string observedConfiguration,
        BackgroundComponentActivationState activationState,
        BackgroundComponentRuntimeState runtimeState,
        DateTimeOffset observedAtUtc,
        IReadOnlyList<BackgroundRollbackEvidenceRequirement> rollbackRequirements,
        StartupApprovalObservation? startupApproval = null)
    {
        var normalizedName = Required(name, nameof(name));
        var normalizedSource = Required(sourceLocator, nameof(sourceLocator));
        var identityMaterial = string.Join(
            "\n",
            kind.ToString(),
            normalizedSource.ToUpperInvariant(),
            normalizedName.ToUpperInvariant());
        var observationMaterial = string.Join(
            "\n",
            identityMaterial,
            observedConfiguration ?? "",
            activationState.ToString(),
            runtimeState.ToString());

        return new BackgroundComponentObservation
        {
            Identity = new BackgroundComponentIdentity
            {
                Kind = kind,
                StableId = Sha256(identityMaterial),
                DisplayName = normalizedName,
                SourceLocator = normalizedSource
            },
            ObservedAtUtc = observedAtUtc.ToUniversalTime(),
            ObservationFingerprint = Sha256(observationMaterial),
            ActivationState = activationState,
            RuntimeState = runtimeState,
            StartupApproval = startupApproval,
            RequiredRollbackEvidence = rollbackRequirements
        };
    }

    private static string StartupApprovalMaterial(StartupApprovalObservation? observation) =>
        observation is null
            ? "startup-approval:not-observed"
            : string.Join(
                "\n",
                observation.ApprovalKeyLocator.ToUpperInvariant(),
                observation.ValueName.ToUpperInvariant(),
                observation.Status.ToString(),
                observation.PayloadLength.ToString(),
                observation.PayloadFingerprint ?? "");

    private static BackgroundComponentActivationState ParseServiceActivation(string? value) =>
        value?.Trim().ToUpperInvariant() switch
        {
            "AUTO" or "AUTOMATIC" => BackgroundComponentActivationState.Automatic,
            "MANUAL" or "DEMAND" => BackgroundComponentActivationState.Manual,
            "DISABLED" => BackgroundComponentActivationState.Disabled,
            "BOOT" => BackgroundComponentActivationState.Boot,
            "SYSTEM" => BackgroundComponentActivationState.System,
            _ => BackgroundComponentActivationState.Unknown
        };

    private static BackgroundComponentRuntimeState ParseServiceRuntime(string? value) =>
        value?.Trim().ToUpperInvariant() switch
        {
            "RUNNING" => BackgroundComponentRuntimeState.Running,
            "STOPPED" => BackgroundComponentRuntimeState.Stopped,
            _ => BackgroundComponentRuntimeState.Unknown
        };

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A non-empty component identity value is required.", parameterName);
        return value.Trim();
    }

    private static string Sha256(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

public sealed class BackgroundComponentChangeReadiness
{
    public required bool CanCreateChangeOperation { get; init; }
    public required int StructuredObservationCount { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }
}

public static class BackgroundComponentChangeReadinessPolicy
{
    public static BackgroundComponentChangeReadiness Evaluate(SoftwareProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var observations = profile.BackgroundComponents ?? [];
        var reasons = new List<string>();

        if (profile.Category == SoftwareCategory.SystemTool)
            reasons.Add("系统相关组件默认不进入直接修改方案。");

        if (observations.Count == 0)
        {
            reasons.Add(profile.StartupEntries.Count + profile.Services.Count + profile.ScheduledTasks.Count > 0
                ? "当前只有名称级线索，没有可唯一重验的结构化身份。"
                : "尚未观察到启动项、服务或计划任务组件。");
        }
        else
        {
            var duplicateIdentity = observations
                .GroupBy(item => item.Identity.StableId, StringComparer.OrdinalIgnoreCase)
                .Any(group => group.Count() != 1);
            if (duplicateIdentity)
                reasons.Add("结构化身份存在冲突，不能确定要处理哪一个组件。");

            if (observations.Any(item => item.ActivationState == BackgroundComponentActivationState.Unknown))
                reasons.Add("至少一个组件的启用状态仍未知。");

            reasons.Add("当前快照只记录观察指纹，没有捕获恢复所需的原始配置和访问控制证据。");
        }

        return new BackgroundComponentChangeReadiness
        {
            CanCreateChangeOperation = false,
            StructuredObservationCount = observations.Count,
            Summary = observations.Count == 0
                ? "当前证据只能解释，不能修改。"
                : "已形成结构化只读观察，但尚不能安全修改。",
            Reasons = reasons
        };
    }
}
