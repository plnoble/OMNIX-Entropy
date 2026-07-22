using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Css.Core.Operations;
using Css.Core.Software;
using Css.Core.Timeline;

namespace Css.Core.Startup;

public enum StartupRegistryValueKind
{
    String,
    ExpandString
}

public sealed record StartupEntryState
{
    public required string SourceLocator { get; init; }
    public required string ValueName { get; init; }
    public required StartupRegistryValueKind ValueKind { get; init; }
    public required string ValueData { get; init; }
    public required string KeyAclSha256 { get; init; }
    public required string ObservationStableId { get; init; }
    public required string ObservationFingerprint { get; init; }
    public StartupApprovalObservation? StartupApproval { get; init; }
    public required DateTimeOffset CapturedAtUtc { get; init; }
    public required string StateFingerprint { get; init; }
}

public static class StartupEntryStateFactory
{
    public const int MaximumValueDataLength = 32 * 1024;

    public static StartupEntryState Create(
        BackgroundComponentObservation observation,
        StartupRegistryValueKind valueKind,
        string valueData,
        string keyAclSha256,
        DateTimeOffset capturedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(observation);
        if (!StartupEntryControlPolicy.IsSupportedObservation(observation))
            throw new ArgumentException("The startup observation is outside the supported current-user Run scope.", nameof(observation));
        if (!Enum.IsDefined(valueKind))
            throw new ArgumentOutOfRangeException(nameof(valueKind));
        if (string.IsNullOrWhiteSpace(valueData)
            || valueData.Length > MaximumValueDataLength
            || valueData.Contains('\0'))
        {
            throw new ArgumentException("The startup value data is empty or unsafe.", nameof(valueData));
        }
        if (!StartupEntryControlPolicy.IsSha256(keyAclSha256))
            throw new ArgumentException("The registry ACL fingerprint is invalid.", nameof(keyAclSha256));

        var state = new StartupEntryState
        {
            SourceLocator = observation.Identity.SourceLocator,
            ValueName = observation.Identity.DisplayName,
            ValueKind = valueKind,
            ValueData = valueData,
            KeyAclSha256 = keyAclSha256.ToUpperInvariant(),
            ObservationStableId = observation.Identity.StableId,
            ObservationFingerprint = observation.ObservationFingerprint,
            StartupApproval = observation.StartupApproval,
            CapturedAtUtc = capturedAtUtc.ToUniversalTime(),
            StateFingerprint = string.Empty
        };
        return state with { StateFingerprint = ComputeFingerprint(state) };
    }

    public static bool Verify(StartupEntryState? state)
    {
        if (state is null
            || !StartupEntryControlPolicy.IsSupportedLocator(state.SourceLocator)
            || !StartupEntryControlPolicy.IsSafeValueName(state.ValueName)
            || !Enum.IsDefined(state.ValueKind)
            || string.IsNullOrWhiteSpace(state.ValueData)
            || state.ValueData.Length > MaximumValueDataLength
            || state.ValueData.Contains('\0')
            || !StartupEntryControlPolicy.IsSha256(state.KeyAclSha256)
            || !StartupEntryControlPolicy.IsSha256(state.ObservationStableId)
            || !StartupEntryControlPolicy.IsSha256(state.ObservationFingerprint)
            || !StartupEntryControlPolicy.IsSha256(state.StateFingerprint)
            || !StartupEntryControlPolicy.IsSupportedApproval(state.StartupApproval))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(state.StateFingerprint),
            Convert.FromHexString(ComputeFingerprint(state)));
    }

    private static string ComputeFingerprint(StartupEntryState state)
    {
        var approval = state.StartupApproval;
        var material = string.Join(
            "\n",
            state.SourceLocator.ToUpperInvariant(),
            state.ValueName.ToUpperInvariant(),
            state.ValueKind.ToString(),
            state.ValueData,
            state.KeyAclSha256.ToUpperInvariant(),
            state.ObservationStableId.ToUpperInvariant(),
            state.ObservationFingerprint.ToUpperInvariant(),
            approval?.ApprovalKeyLocator.ToUpperInvariant() ?? string.Empty,
            approval?.ValueName.ToUpperInvariant() ?? string.Empty,
            approval?.Status.ToString() ?? string.Empty,
            approval?.PayloadFingerprint?.ToUpperInvariant() ?? string.Empty,
            approval?.PayloadLength.ToString() ?? "0",
            state.CapturedAtUtc.ToUniversalTime().ToString("O"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material)));
    }
}

public static class StartupEntryControlPolicy
{
    public const string SupportedSourceLocator =
        @"HKCU64\Software\Microsoft\Windows\CurrentVersion\Run";
    public const string SupportedApprovalLocator =
        @"HKCU64\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

    public static bool IsSupportedObservation(BackgroundComponentObservation observation) =>
        observation.Identity.Kind == BackgroundComponentKind.StartupEntry
        && IsSupportedLocator(observation.Identity.SourceLocator)
        && IsSafeValueName(observation.Identity.DisplayName)
        && IsSha256(observation.Identity.StableId)
        && IsSha256(observation.ObservationFingerprint)
        && IsSupportedApproval(observation.StartupApproval);

    public static bool HasSingleSupportedObservation(SoftwareProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (profile.Category == SoftwareCategory.SystemTool)
            return false;

        var observations = (profile.BackgroundComponents ?? [])
            .Where(item => item.Identity.Kind == BackgroundComponentKind.StartupEntry)
            .ToArray();
        return observations.Length == 1 && IsSupportedObservation(observations[0]);
    }

    public static bool IsSupportedLocator(string? locator) =>
        string.Equals(locator?.Trim(), SupportedSourceLocator, StringComparison.OrdinalIgnoreCase);

    public static bool IsSupportedApproval(StartupApprovalObservation? approval) =>
        approval is not null
        && string.Equals(
            approval.ApprovalKeyLocator,
            SupportedApprovalLocator,
            StringComparison.OrdinalIgnoreCase)
        && IsSafeValueName(approval.ValueName)
        && approval.Status is StartupApprovalEvidenceStatus.Missing
            or StartupApprovalEvidenceStatus.PresentBinary
        && (approval.Status != StartupApprovalEvidenceStatus.PresentBinary
            || (approval.PayloadLength is > 0 and <= 4096
                && IsSha256(approval.PayloadFingerprint)));

    public static bool IsSafeValueName(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Length <= 260
        && !value.Any(char.IsControl);

    public static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public sealed record StartupEntryCaptureResult
{
    public required bool Success { get; init; }
    public StartupEntryState? State { get; init; }
    public string? Error { get; init; }

    public static StartupEntryCaptureResult Completed(StartupEntryState state) =>
        new() { Success = true, State = state };

    public static StartupEntryCaptureResult Refused(string error) =>
        new() { Success = false, Error = error };
}

public sealed record StartupEntryMutationResult
{
    public required bool Success { get; init; }
    public required string Summary { get; init; }

    public static StartupEntryMutationResult Completed(string summary) =>
        new() { Success = true, Summary = summary };

    public static StartupEntryMutationResult Refused(string summary) =>
        new() { Success = false, Summary = summary };
}

public interface IStartupEntryControlStore
{
    Task<StartupEntryCaptureResult> CaptureAsync(
        BackgroundComponentObservation observation,
        CancellationToken cancellationToken = default);

    Task<StartupEntryMutationResult> DisableAsync(
        StartupEntryState expected,
        CancellationToken cancellationToken = default);

    Task<StartupEntryMutationResult> RestoreAsync(
        StartupEntryState expected,
        CancellationToken cancellationToken = default);
}

public enum StartupControlPreparationStatus
{
    Ready,
    NotEligible,
    Unsupported,
    Ambiguous,
    Unavailable,
    Stale
}

public sealed class StartupControlPreparation
{
    public required StartupControlPreparationStatus Status { get; init; }
    public required string SoftwareName { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<string> Lines { get; init; }
    public StartupEntryState? State { get; init; }
    public bool CanContinue => Status == StartupControlPreparationStatus.Ready && State is not null;
}

public static class StartupControlPreparationService
{
    private static readonly TimeSpan MaximumObservationAge = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan MaximumFutureSkew = TimeSpan.FromSeconds(30);

    public static async Task<StartupControlPreparation> PrepareAsync(
        SoftwareProfile profile,
        IStartupEntryControlStore store,
        DateTimeOffset? nowUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(store);
        var now = (nowUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();

        if (profile.Category == SoftwareCategory.SystemTool)
            return Refused(profile, StartupControlPreparationStatus.NotEligible,
                "这是系统相关应用，OMNIX 不会直接关闭它的启动组件。");

        var startupObservations = (profile.BackgroundComponents ?? [])
            .Where(item => item.Identity.Kind == BackgroundComponentKind.StartupEntry)
            .ToArray();
        if (startupObservations.Length == 0)
            return Refused(profile, StartupControlPreparationStatus.Unsupported,
                "当前只有名称级线索，不能确定要关闭哪一个启动项。");
        if (startupObservations.Length != 1)
            return Refused(profile, StartupControlPreparationStatus.Ambiguous,
                "这个应用关联了多个启动项，当前版本不会替你批量处理。");

        var observation = startupObservations[0];
        if (!StartupEntryControlPolicy.IsSupportedObservation(observation))
            return Refused(profile, StartupControlPreparationStatus.Unsupported,
                "这个启动项不在当前用户普通 Run 项的安全范围内。");

        StartupEntryCaptureResult capture;
        try
        {
            capture = await store.CaptureAsync(observation, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Refused(profile, StartupControlPreparationStatus.Unavailable,
                "暂时无法读取这个启动项的恢复证据，请稍后重试。");
        }

        var state = capture.State;
        if (!capture.Success || state is null)
            return Refused(profile, StartupControlPreparationStatus.Unavailable,
                "启动项恢复证据不完整，当前不会修改。");
        if (!StartupEntryStateFactory.Verify(state)
            || !string.Equals(state.ObservationStableId, observation.Identity.StableId, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(state.ObservationFingerprint, observation.ObservationFingerprint, StringComparison.OrdinalIgnoreCase)
            || state.CapturedAtUtc < now - MaximumObservationAge
            || state.CapturedAtUtc > now + MaximumFutureSkew)
        {
            return Refused(profile, StartupControlPreparationStatus.Stale,
                "启动项状态与刚才的扫描不一致，请重新扫描后再决定。");
        }

        return new StartupControlPreparation
        {
            Status = StartupControlPreparationStatus.Ready,
            SoftwareName = SafeSoftwareName(profile.Name),
            Summary = $"可以为 {SafeSoftwareName(profile.Name)} 生成一个可还原的自启动关闭方案。",
            Lines =
            [
                "只处理当前用户的 1 个普通自启动项。",
                "不会停止正在运行的软件，也不会修改服务或计划任务。",
                "执行前会保存原始值和验证指纹，完成后可在后悔药中心还原。"
            ],
            State = state
        };
    }

    private static StartupControlPreparation Refused(
        SoftwareProfile profile,
        StartupControlPreparationStatus status,
        string summary) =>
        new()
        {
            Status = status,
            SoftwareName = SafeSoftwareName(profile.Name),
            Summary = summary,
            Lines =
            [
                "当前不会直接修改注册表。",
                "你仍可以在 Windows 的“启动应用”页面查看和处理。"
            ]
        };

    private static string SafeSoftwareName(string? name)
    {
        var value = name?.Trim() ?? string.Empty;
        return value.Length is > 0 and <= 120
            && !value.Any(char.IsControl)
            && !value.Contains(':')
            && !value.Contains('\\')
            && !value.Contains('/')
                ? value
                : "这个应用";
    }
}

public sealed class StartupRollbackManifest
{
    public int SchemaVersion { get; init; } = 1;
    public required string ManifestId { get; init; }
    public required string SoftwareName { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required StartupEntryState State { get; init; }
}

public sealed record StartupRollbackManifestEvidence(
    string SnapshotId,
    string ManifestPath,
    string Sha256);

public sealed record VerifiedStartupRollbackManifest(
    StartupRollbackManifest Manifest,
    string ManifestPath,
    string Sha256);

public sealed class StartupRollbackManifestStore
{
    private const int MaximumManifestBytes = 64 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };
    private readonly string _root;

    public StartupRollbackManifestStore(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
            throw new ArgumentException("A startup rollback root is required.", nameof(root));
        _root = Path.GetFullPath(root);
    }

    public async Task<StartupRollbackManifestEvidence> CreateAsync(
        StartupControlPreparation preparation,
        DateTimeOffset? nowUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preparation);
        if (!preparation.CanContinue || !StartupEntryStateFactory.Verify(preparation.State))
            throw new InvalidOperationException("A ready startup preparation is required.");

        var manifest = new StartupRollbackManifest
        {
            ManifestId = "startup-snapshot-" + Guid.NewGuid().ToString("N"),
            SoftwareName = preparation.SoftwareName,
            CreatedAtUtc = (nowUtc ?? DateTimeOffset.UtcNow).ToUniversalTime(),
            State = preparation.State!
        };
        ValidateManifest(manifest);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions);
        if (bytes.Length is <= 0 or > MaximumManifestBytes)
            throw new InvalidDataException("The startup rollback manifest is too large.");
        var sha256 = Convert.ToHexString(SHA256.HashData(bytes));

        Directory.CreateDirectory(_root);
        EnsureNoReparseChain(_root);
        var target = Path.Combine(_root, manifest.ManifestId + "-" + sha256 + ".json");
        var temporary = target + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await using (var stream = new FileStream(
                             temporary,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             4096,
                             FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await stream.WriteAsync(bytes, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
            File.Move(temporary, target, overwrite: false);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }

        return new StartupRollbackManifestEvidence(manifest.ManifestId, target, sha256);
    }

    public Task<VerifiedStartupRollbackManifest> LoadVerifiedAsync(
        StartupRollbackManifestEvidence evidence,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        return LoadVerifiedCoreAsync(
            evidence.ManifestPath,
            evidence.Sha256,
            evidence.SnapshotId,
            cancellationToken);
    }

    public Task<VerifiedStartupRollbackManifest> LoadVerifiedAsync(
        string manifestPath,
        CancellationToken cancellationToken = default) =>
        LoadVerifiedCoreAsync(manifestPath, null, null, cancellationToken);

    public async Task<bool> DeleteUncommittedAsync(
        StartupRollbackManifestEvidence evidence,
        CancellationToken cancellationToken = default)
    {
        var verified = await LoadVerifiedAsync(evidence, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        File.Delete(verified.ManifestPath);
        return !File.Exists(verified.ManifestPath);
    }

    private async Task<VerifiedStartupRollbackManifest> LoadVerifiedCoreAsync(
        string manifestPath,
        string? expectedSha256,
        string? expectedSnapshotId,
        CancellationToken cancellationToken)
    {
        var fullPath = ConfineManifestPath(manifestPath);
        var info = new FileInfo(fullPath);
        if (!info.Exists
            || info.Length is <= 0 or > MaximumManifestBytes
            || info.Attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            throw new InvalidDataException("The startup rollback manifest is missing or unsafe.");
        }
        EnsureNoReparseChain(info.DirectoryName!);

        var bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
        var sha256 = Convert.ToHexString(SHA256.HashData(bytes));
        var filenameSha256 = ExtractFilenameSha256(fullPath);
        if (!FixedEquals(sha256, filenameSha256)
            || (expectedSha256 is not null && !FixedEquals(sha256, expectedSha256)))
        {
            throw new InvalidDataException("The startup rollback manifest fingerprint does not match.");
        }

        StartupRollbackManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<StartupRollbackManifest>(bytes, JsonOptions)
                ?? throw new InvalidDataException("The startup rollback manifest is empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("The startup rollback manifest is invalid.", exception);
        }
        ValidateManifest(manifest);
        if ((expectedSnapshotId is not null
             && !string.Equals(manifest.ManifestId, expectedSnapshotId, StringComparison.Ordinal))
            || !Path.GetFileNameWithoutExtension(fullPath)
                .Equals(manifest.ManifestId + "-" + sha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("The startup rollback manifest identity does not match.");
        }
        return new VerifiedStartupRollbackManifest(manifest, fullPath, sha256);
    }

    private string ConfineManifestPath(string manifestPath)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
            throw new InvalidDataException("A startup rollback manifest path is required.");
        var fullPath = Path.GetFullPath(manifestPath);
        var root = _root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The startup rollback manifest escaped its root.");
        return fullPath;
    }

    private static void ValidateManifest(StartupRollbackManifest manifest)
    {
        if (manifest.SchemaVersion != 1
            || string.IsNullOrWhiteSpace(manifest.ManifestId)
            || !manifest.ManifestId.StartsWith("startup-snapshot-", StringComparison.Ordinal)
            || manifest.ManifestId.Length > 96
            || string.IsNullOrWhiteSpace(manifest.SoftwareName)
            || manifest.SoftwareName.Length > 120
            || manifest.SoftwareName.Any(char.IsControl)
            || !StartupEntryStateFactory.Verify(manifest.State))
        {
            throw new InvalidDataException("The startup rollback manifest is incomplete.");
        }
    }

    private static string ExtractFilenameSha256(string path)
    {
        var stem = Path.GetFileNameWithoutExtension(path);
        if (stem.Length <= 65 || stem[^65] != '-')
            throw new InvalidDataException("The startup rollback manifest filename is invalid.");
        var value = stem[^64..];
        if (!StartupEntryControlPolicy.IsSha256(value))
            throw new InvalidDataException("The startup rollback manifest filename fingerprint is invalid.");
        return value;
    }

    private static bool FixedEquals(string left, string right) =>
        StartupEntryControlPolicy.IsSha256(left)
        && StartupEntryControlPolicy.IsSha256(right)
        && CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(left),
            Convert.FromHexString(right));

    private static void EnsureNoReparseChain(string path)
    {
        var current = new DirectoryInfo(Path.GetFullPath(path));
        while (current is not null)
        {
            if (current.Exists && current.Attributes.HasFlag(FileAttributes.ReparsePoint))
                throw new InvalidDataException("The startup rollback path contains a redirect.");
            current = current.Parent;
        }
    }
}

public static class StartupEntryControlOperationPolicy
{
    public const string DisableKind = "startup.disable.hkcu.run";
    public const string RestoreKind = "startup.restore.hkcu.run";
    public const string ManifestPathArgument = "manifestPath";
    public const string ManifestSha256Argument = "manifestSha256";

    public static OperationDescriptor CreateDisablePlan(
        StartupControlPreparation preparation,
        StartupRollbackManifestEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(evidence);
        if (!preparation.CanContinue || !StartupEntryStateFactory.Verify(preparation.State))
            throw new InvalidOperationException("A ready startup preparation is required.");
        if (string.IsNullOrWhiteSpace(evidence.SnapshotId)
            || string.IsNullOrWhiteSpace(evidence.ManifestPath)
            || !StartupEntryControlPolicy.IsSha256(evidence.Sha256))
        {
            throw new InvalidOperationException("Startup rollback evidence is incomplete.");
        }

        return new OperationDescriptor
        {
            Kind = DisableKind,
            Title = $"关闭 {preparation.SoftwareName} 的自启动",
            Source = OperationSource.Manual,
            Risk = RiskLevel.Medium,
            IsDestructive = true,
            RequiresElevation = false,
            RequiresSnapshot = true,
            SnapshotId = evidence.SnapshotId,
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = "已确认 1 个当前用户普通自启动项，并保存了原始值和恢复证据。",
            EstimatedImpactBytes = 0,
            ConfirmationText = "关闭这个普通自启动项；不停止软件，不改服务或计划任务。",
            AffectedRegistryKeys = [preparation.State!.SourceLocator],
            Arguments = new Dictionary<string, object?>
            {
                [ManifestPathArgument] = evidence.ManifestPath,
                [ManifestSha256Argument] = evidence.Sha256
            }
        };
    }

    public static OperationResult ValidateCandidate(OperationDescriptor descriptor)
    {
        if (!descriptor.Kind.Equals(DisableKind, StringComparison.OrdinalIgnoreCase)
            || descriptor.Risk != RiskLevel.Medium
            || !descriptor.IsDestructive
            || descriptor.RequiresElevation
            || !descriptor.RequiresSnapshot
            || string.IsNullOrWhiteSpace(descriptor.SnapshotId)
            || !descriptor.RollbackRequired
            || string.IsNullOrWhiteSpace(descriptor.EvidenceSummary)
            || descriptor.AffectedRegistryKeys.Count != 1
            || !StartupEntryControlPolicy.IsSupportedLocator(descriptor.AffectedRegistryKeys[0])
            || !TryString(descriptor, ManifestPathArgument, out _)
            || !TryString(descriptor, ManifestSha256Argument, out var hash)
            || !StartupEntryControlPolicy.IsSha256(hash))
        {
            return OperationResult.Fail("The startup operation is incomplete or outside the supported scope.");
        }
        return OperationResult.Ok("Startup operation candidate accepted.");
    }

    public static OperationDescriptor ConfirmForExecution(OperationDescriptor descriptor)
    {
        var gate = ValidateCandidate(descriptor);
        if (!gate.Success)
            throw new InvalidOperationException(gate.Error);
        return new OperationDescriptor
        {
            Kind = descriptor.Kind,
            Title = descriptor.Title,
            Source = descriptor.Source,
            Risk = descriptor.Risk,
            IsDestructive = descriptor.IsDestructive,
            RequiresElevation = descriptor.RequiresElevation,
            RequiresSnapshot = descriptor.RequiresSnapshot,
            SnapshotId = descriptor.SnapshotId,
            RollbackRequired = descriptor.RollbackRequired,
            ConfirmationAccepted = true,
            EvidenceSummary = descriptor.EvidenceSummary,
            EstimatedImpactBytes = descriptor.EstimatedImpactBytes,
            ConfirmationText = descriptor.ConfirmationText,
            AffectedPaths = descriptor.AffectedPaths,
            AffectedRegistryKeys = descriptor.AffectedRegistryKeys,
            AffectedServices = descriptor.AffectedServices,
            Arguments = descriptor.Arguments
        };
    }

    internal static bool TryString(OperationDescriptor descriptor, string key, out string value)
    {
        value = string.Empty;
        if (!descriptor.Arguments.TryGetValue(key, out var raw) || raw is not string text || string.IsNullOrWhiteSpace(text))
            return false;
        value = text;
        return true;
    }
}

public sealed class StartupEntryControlOperationHandler
{
    private static readonly TimeSpan MaximumSnapshotAge = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MaximumFutureSkew = TimeSpan.FromSeconds(30);
    private readonly IStartupEntryControlStore _store;
    private readonly StartupRollbackManifestStore _manifests;
    private readonly ActionTimelineStore _timeline;
    private readonly Func<DateTimeOffset> _clock;

    public StartupEntryControlOperationHandler(
        IStartupEntryControlStore store,
        StartupRollbackManifestStore manifests,
        ActionTimelineStore timeline,
        Func<DateTimeOffset>? clock = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _manifests = manifests ?? throw new ArgumentNullException(nameof(manifests));
        _timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<OperationResult> ExecuteAsync(
        OperationDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        var gate = StartupEntryControlOperationPolicy.ValidateCandidate(descriptor);
        if (!gate.Success)
            return gate;
        if (!descriptor.ConfirmationAccepted)
            return OperationResult.Fail("Startup changes require explicit user confirmation.");
        if (!StartupEntryControlOperationPolicy.TryString(
                descriptor,
                StartupEntryControlOperationPolicy.ManifestPathArgument,
                out var manifestPath)
            || !StartupEntryControlOperationPolicy.TryString(
                descriptor,
                StartupEntryControlOperationPolicy.ManifestSha256Argument,
                out var manifestSha256))
        {
            return OperationResult.Fail("Startup rollback evidence is missing.");
        }

        VerifiedStartupRollbackManifest verified;
        try
        {
            verified = await _manifests.LoadVerifiedAsync(
                new StartupRollbackManifestEvidence(descriptor.SnapshotId!, manifestPath, manifestSha256),
                cancellationToken);
        }
        catch
        {
            return OperationResult.Fail("Startup rollback evidence could not be verified.");
        }

        var now = _clock().ToUniversalTime();
        if (verified.Manifest.CreatedAtUtc < now - MaximumSnapshotAge
            || verified.Manifest.CreatedAtUtc > now + MaximumFutureSkew
            || !string.Equals(
                verified.Manifest.State.SourceLocator,
                descriptor.AffectedRegistryKeys.Single(),
                StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult.Fail("Startup rollback evidence is stale or does not match the operation.");
        }

        var disabled = await _store.DisableAsync(verified.Manifest.State, cancellationToken);
        if (!disabled.Success)
            return OperationResult.Fail(disabled.Summary);

        try
        {
            await _timeline.AddAsync(new ActionTimelineEntry
            {
                OccurredAt = now,
                Source = descriptor.Source,
                Title = descriptor.Title,
                EvidenceSummary = descriptor.EvidenceSummary!,
                AffectedRegistryKeys = descriptor.AffectedRegistryKeys,
                RestoreState = RestoreState.Restorable,
                RestoreOperationKind = StartupEntryControlOperationPolicy.RestoreKind,
                RestoreManifestPaths = [verified.ManifestPath]
            }, cancellationToken);
        }
        catch
        {
            var restored = await _store.RestoreAsync(verified.Manifest.State, CancellationToken.None);
            return OperationResult.Fail(restored.Success
                ? "The startup entry was restored because the action timeline could not be written."
                : "The startup change could not be journaled and automatic restore also failed; review it immediately.");
        }

        return OperationResult.Ok("已关闭 1 个普通自启动项；没有停止软件，也没有修改服务或计划任务。", verified.ManifestPath);
    }

}
