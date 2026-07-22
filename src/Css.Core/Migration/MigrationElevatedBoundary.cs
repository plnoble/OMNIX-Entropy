using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Css.Core.Apps;
using Css.Core.Operations;

namespace Css.Core.Migration;

public enum MigrationElevatedRequestStatus
{
    Refused,
    Ready
}

public sealed record MigrationFinalUserConsent
{
    public required string ConfirmationText { get; init; }
    public bool PlanReviewedConfirmed { get; init; }
    public bool AppComponentsClosedConfirmed { get; init; }
    public bool RollbackAcknowledged { get; init; }
    public bool MonitoringConfirmed { get; init; }
    public bool ExecutionRequested { get; init; }
    public DateTimeOffset ConfirmedAtUtc { get; init; }
}

public sealed class MigrationElevatedRequestDraft
{
    public required MigrationElevatedRequestStatus Status { get; init; }
    public required IReadOnlyList<string> MissingRequirements { get; init; }
    public DateTimeOffset? PreparedAtUtc { get; init; }
    public string? RequestId { get; init; }
    public string? DescriptorSha256 { get; init; }
    public OperationDescriptor? Operation { get; init; }

    public bool CanSubmit => Status == MigrationElevatedRequestStatus.Ready
        && PreparedAtUtc is { } preparedAt
        && preparedAt != default
        && !string.IsNullOrWhiteSpace(RequestId)
        && IsSha256(DescriptorSha256)
        && Operation is not null;

    private static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public static class MigrationElevatedRequestComposer
{
    private static readonly TimeSpan MaximumConsentAge = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan MaximumClockSkew = TimeSpan.FromSeconds(30);

    public static MigrationElevatedRequestDraft Create(
        MigrationExecutionGateResult gate,
        MigrationFinalUserConsent? consent,
        string requestId,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(gate);
        var missing = new List<string>();
        var operation = ValidateGate(gate, missing);
        ValidateConsent(operation, consent, now, missing);
        if (!IsToken(requestId))
            missing.Add("The migration request id is invalid.");

        if (operation is null || missing.Count > 0)
            return Refused(missing);

        var confirmed = CloneConfirmed(operation);
        return new MigrationElevatedRequestDraft
        {
            Status = MigrationElevatedRequestStatus.Ready,
            MissingRequirements = [],
            PreparedAtUtc = consent!.ConfirmedAtUtc.ToUniversalTime(),
            RequestId = requestId,
            DescriptorSha256 = ComputeDescriptorSha256(confirmed),
            Operation = confirmed
        };
    }

    public static string ComputeDescriptorSha256(OperationDescriptor operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        var canonical = new StringBuilder();
        Append(canonical, operation.Kind);
        Append(canonical, operation.Title);
        Append(canonical, operation.Source.ToString());
        Append(canonical, operation.Risk.ToString());
        Append(canonical, operation.IsDestructive);
        Append(canonical, operation.RequiresElevation);
        Append(canonical, operation.RequiresSnapshot);
        Append(canonical, operation.SnapshotId);
        Append(canonical, operation.RollbackRequired);
        Append(canonical, operation.ConfirmationAccepted);
        Append(canonical, operation.EvidenceSummary);
        Append(canonical, operation.EstimatedImpactBytes);
        Append(canonical, operation.ConfirmationText);
        AppendList(canonical, operation.AffectedPaths);
        AppendList(canonical, operation.AffectedRegistryKeys);
        AppendList(canonical, operation.AffectedServices);
        foreach (var pair in operation.Arguments.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            Append(canonical, pair.Key);
            switch (pair.Value)
            {
                case string text:
                    Append(canonical, "string");
                    Append(canonical, text);
                    break;
                case bool boolean:
                    Append(canonical, "boolean");
                    Append(canonical, boolean);
                    break;
                case string[] array:
                    Append(canonical, "string-list");
                    AppendList(canonical, array);
                    break;
                case IReadOnlyList<string> list:
                    Append(canonical, "string-list");
                    AppendList(canonical, list);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Migration argument '{pair.Key}' has an unsupported type.");
            }
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())));
    }

    private static OperationDescriptor? ValidateGate(
        MigrationExecutionGateResult gate,
        ICollection<string> missing)
    {
        var operation = gate.Operation;
        if (!gate.CanRequestExecution || gate.BlockingReasons.Count > 0 || operation is null)
        {
            missing.Add("The migration preflight is incomplete.");
            return null;
        }

        if (!string.Equals(operation.Kind, "migration.execute", StringComparison.Ordinal)
            || operation.Source != OperationSource.Manual
            || operation.Risk != RiskLevel.High
            || !operation.IsDestructive
            || !operation.RequiresElevation
            || !operation.RequiresSnapshot
            || !operation.RollbackRequired
            || operation.ConfirmationAccepted
            || string.IsNullOrWhiteSpace(operation.SnapshotId)
            || string.IsNullOrWhiteSpace(operation.EvidenceSummary)
            || string.IsNullOrWhiteSpace(operation.ConfirmationText)
            || operation.AffectedPaths.Count is < 1 or > 32)
        {
            missing.Add("The migration operation safety contract is incomplete.");
            return null;
        }

        if (!TryString(operation, "destinationRoot")
            || !TryString(operation, "rollbackManifestPath")
            || !TrySha256(operation, "rollbackManifestSha256")
            || !TryString(operation, "snapshotEvidencePath")
            || !TrySha256(operation, "snapshotEvidenceSha256")
            || !TryStringList(operation, "affectedProcesses")
            || !TryStringList(operation, "scheduledTasks")
            || !TryStringList(operation, "startupEntries")
            || !TryStringList(operation, "monitorPaths"))
        {
            missing.Add("The migration operation evidence is incomplete.");
            return null;
        }

        return operation;
    }

    private static void ValidateConsent(
        OperationDescriptor? operation,
        MigrationFinalUserConsent? consent,
        DateTimeOffset now,
        ICollection<string> missing)
    {
        if (consent is null)
        {
            missing.Add("Final migration confirmation is missing.");
            return;
        }

        if (operation is null
            || !string.Equals(operation.ConfirmationText, consent.ConfirmationText, StringComparison.Ordinal)
            || !consent.PlanReviewedConfirmed
            || !consent.AppComponentsClosedConfirmed
            || !consent.RollbackAcknowledged
            || !consent.MonitoringConfirmed
            || !consent.ExecutionRequested)
        {
            missing.Add("Final migration confirmation does not match the current plan.");
        }

        var age = now.ToUniversalTime() - consent.ConfirmedAtUtc.ToUniversalTime();
        if (age > MaximumConsentAge || age < -MaximumClockSkew)
            missing.Add("Final migration confirmation is stale or future-dated.");
    }

    private static OperationDescriptor CloneConfirmed(OperationDescriptor source)
    {
        var arguments = source.Arguments.ToDictionary(
            pair => pair.Key,
            pair => pair.Value switch
            {
                string text => (object?)text,
                bool boolean => boolean,
                string[] array => array.ToArray(),
                IReadOnlyList<string> list => list.ToArray(),
                _ => throw new InvalidOperationException(
                    $"Migration argument '{pair.Key}' has an unsupported type.")
            },
            StringComparer.Ordinal);

        return new OperationDescriptor
        {
            Kind = source.Kind,
            Title = source.Title,
            Source = source.Source,
            Risk = source.Risk,
            IsDestructive = source.IsDestructive,
            RequiresElevation = source.RequiresElevation,
            RequiresSnapshot = source.RequiresSnapshot,
            SnapshotId = source.SnapshotId,
            RollbackRequired = source.RollbackRequired,
            ConfirmationAccepted = true,
            EvidenceSummary = source.EvidenceSummary,
            EstimatedImpactBytes = source.EstimatedImpactBytes,
            ConfirmationText = source.ConfirmationText,
            AffectedPaths = source.AffectedPaths.ToArray(),
            AffectedRegistryKeys = source.AffectedRegistryKeys.ToArray(),
            AffectedServices = source.AffectedServices.ToArray(),
            Arguments = arguments
        };
    }

    private static bool TryString(OperationDescriptor operation, string key) =>
        operation.Arguments.TryGetValue(key, out var value)
        && value is string text
        && !string.IsNullOrWhiteSpace(text);

    private static bool TrySha256(OperationDescriptor operation, string key) =>
        operation.Arguments.TryGetValue(key, out var value)
        && value is string { Length: 64 } text
        && text.All(Uri.IsHexDigit);

    private static bool TryStringList(OperationDescriptor operation, string key) =>
        operation.Arguments.TryGetValue(key, out var value)
        && value is IReadOnlyList<string> list
        && list.Count <= 256
        && list.All(item => !string.IsNullOrWhiteSpace(item) && item.Length <= 4096);

    private static void AppendList(StringBuilder builder, IReadOnlyList<string> values)
    {
        Append(builder, values.Count);
        foreach (var value in values)
            Append(builder, value);
    }

    private static void Append(StringBuilder builder, object? value)
    {
        var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        builder.Append(text.Length).Append(':').Append(text).Append(';');
    }

    private static bool IsToken(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Length <= 128
        && value.IndexOfAny(['\\', '/']) < 0;

    private static MigrationElevatedRequestDraft Refused(IReadOnlyCollection<string> missing) =>
        new()
        {
            Status = MigrationElevatedRequestStatus.Refused,
            MissingRequirements = missing.Count > 0
                ? missing.ToArray()
                : ["The migration request did not pass final safety checks."]
        };
}

public sealed class MigrationElevatedResponseEnvelope
{
    public required string RequestId { get; init; }
    public required OperationResult Result { get; init; }
}
