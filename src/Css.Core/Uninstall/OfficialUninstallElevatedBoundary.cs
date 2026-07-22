using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Css.Core.Apps;
using Css.Core.Operations;

namespace Css.Core.Uninstall;

public sealed record OfficialUninstallVisualGateReceipt
{
    public required string UiContractVersion { get; init; }
    public required string ScreenshotSha256 { get; init; }
    public required DateTimeOffset CapturedAtUtc { get; init; }
    public bool RecoveryTruthVisible { get; init; }
    public bool FinalConfirmationVisible { get; init; }
    public bool TechnicalDetailsCollapsedByDefault { get; init; }
    public bool NoExecutionControlDuringPreparation { get; init; }
}

public enum OfficialUninstallElevatedRequestStatus
{
    Refused,
    Ready
}

public sealed class OfficialUninstallElevatedRequestDraft
{
    public required OfficialUninstallElevatedRequestStatus Status { get; init; }
    public required IReadOnlyList<string> MissingRequirements { get; init; }
    public DateTimeOffset? PreparedAtUtc { get; init; }
    public string? RequestId { get; init; }
    public string? DescriptorSha256 { get; init; }
    public OperationDescriptor? Operation { get; init; }
    public bool CanSubmit => Status == OfficialUninstallElevatedRequestStatus.Ready
        && Operation is not null
        && PreparedAtUtc is { } preparedAt
        && preparedAt != default
        && !string.IsNullOrWhiteSpace(RequestId)
        && !string.IsNullOrWhiteSpace(DescriptorSha256);
}

public static class OfficialUninstallElevatedRequestComposer
{
    public const string RequiredUiContractVersion = "uninstall-final-confirmation-v1";

    private static readonly TimeSpan MaximumVisualProofAge = TimeSpan.FromHours(24);
    private static readonly TimeSpan MaximumConsentAge = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan MaximumClockSkew = TimeSpan.FromMinutes(5);

    public static OfficialUninstallElevatedRequestDraft Create(
        OfficialUninstallExecutionGateResult gate,
        OfficialUninstallVisualGateReceipt? visualGate,
        OfficialUninstallFinalUserConsent? finalConsent,
        string requestId,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(gate);

        var missing = new List<string>();
        var operation = ValidateGate(gate, missing);
        ValidateVisualGate(visualGate, now, missing);
        ValidateConsent(operation, visualGate, finalConsent, now, missing);
        if (string.IsNullOrWhiteSpace(requestId) || requestId.Length > 128)
            missing.Add("\u8bf7\u6c42\u7f16\u53f7\u65e0\u6548\uff0c\u65e0\u6cd5\u7ed1\u5b9a\u672c\u6b21\u786e\u8ba4\u3002");

        if (missing.Count > 0 || operation is null)
            return Refused(missing);

        if (!TryCloneConfirmed(operation, out var confirmed, out var cloneError))
        {
            missing.Add(cloneError!);
            return Refused(missing);
        }

        return new OfficialUninstallElevatedRequestDraft
        {
            Status = OfficialUninstallElevatedRequestStatus.Ready,
            MissingRequirements = [],
            PreparedAtUtc = finalConsent!.ConfirmedAtUtc.ToUniversalTime(),
            RequestId = requestId,
            Operation = confirmed,
            DescriptorSha256 = ComputeDescriptorSha256(confirmed!)
        };
    }

    private static OperationDescriptor? ValidateGate(
        OfficialUninstallExecutionGateResult gate,
        ICollection<string> missing)
    {
        var operation = gate.Operation;
        if (!gate.CanRequestExecution || gate.BlockingReasons.Count > 0 || operation is null)
        {
            missing.Add("\u5b98\u65b9\u5378\u8f7d\u7684\u6062\u590d\u3001\u5feb\u7167\u6216\u547d\u4ee4\u68c0\u67e5\u5c1a\u672a\u901a\u8fc7\u3002");
            return null;
        }

        if (!string.Equals(operation.Kind, "uninstall.official.run", StringComparison.Ordinal)
            || operation.Source != OperationSource.Manual
            || operation.Risk != RiskLevel.High
            || !operation.IsDestructive
            || !operation.RequiresElevation
            || !operation.RequiresSnapshot
            || !operation.RollbackRequired
            || operation.ConfirmationAccepted
            || string.IsNullOrWhiteSpace(operation.SnapshotId)
            || string.IsNullOrWhiteSpace(operation.EvidenceSummary)
            || string.IsNullOrWhiteSpace(operation.ConfirmationText))
        {
            missing.Add("\u5378\u8f7d\u8bf7\u6c42\u7684\u5b89\u5168\u5c5e\u6027\u4e0d\u5b8c\u6574\u6216\u5df2\u88ab\u63d0\u524d\u786e\u8ba4\u3002");
            return null;
        }

        if (!HasRequiredArguments(operation))
        {
            missing.Add("\u5378\u8f7d\u8bf7\u6c42\u7f3a\u5c11\u7ecf\u9a8c\u8bc1\u7684\u547d\u4ee4\u3001\u5feb\u7167\u6216\u6062\u590d\u8bc1\u636e\u3002");
            return null;
        }

        return operation;
    }

    private static void ValidateVisualGate(
        OfficialUninstallVisualGateReceipt? receipt,
        DateTimeOffset now,
        ICollection<string> missing)
    {
        if (receipt is null)
        {
            missing.Add("\u7f3a\u5c11\u6700\u7ec8\u754c\u9762\u7684\u53ef\u89c6\u9a8c\u8bc1\u8bc1\u636e\u3002");
            return;
        }

        if (!string.Equals(
                receipt.UiContractVersion,
                RequiredUiContractVersion,
                StringComparison.Ordinal)
            || !IsSha256(receipt.ScreenshotSha256)
            || !receipt.RecoveryTruthVisible
            || !receipt.FinalConfirmationVisible
            || !receipt.TechnicalDetailsCollapsedByDefault
            || !receipt.NoExecutionControlDuringPreparation)
        {
            missing.Add("\u6700\u7ec8\u754c\u9762\u6ca1\u6709\u5b8c\u6574\u5c55\u793a\u6062\u590d\u771f\u76f8\u3001\u786e\u8ba4\u533a\u6216\u5b89\u5168\u9ed8\u8ba4\u72b6\u6001\u3002");
        }

        var age = now.ToUniversalTime() - receipt.CapturedAtUtc.ToUniversalTime();
        if (age > MaximumVisualProofAge || age < -MaximumClockSkew)
            missing.Add("\u6700\u7ec8\u754c\u9762\u9a8c\u8bc1\u8bc1\u636e\u5df2\u8fc7\u671f\u6216\u65f6\u95f4\u65e0\u6548\u3002");
    }

    private static void ValidateConsent(
        OperationDescriptor? operation,
        OfficialUninstallVisualGateReceipt? visualGate,
        OfficialUninstallFinalUserConsent? consent,
        DateTimeOffset now,
        ICollection<string> missing)
    {
        if (consent is null)
        {
            missing.Add("\u7f3a\u5c11\u7528\u6237\u7684\u6700\u7ec8\u786e\u8ba4\u3002");
            return;
        }

        if (operation is null
            || !string.Equals(
                operation.ConfirmationText,
                consent.ConfirmationText,
                StringComparison.Ordinal)
            || !consent.OfficialCommandConfirmed
            || !consent.AppsClosedConfirmed
            || !consent.NoAutomaticUndoAcknowledged
            || !consent.PostUninstallRescanConfirmed
            || !consent.ExecutionRequested)
        {
            missing.Add("\u6700\u7ec8\u786e\u8ba4\u4e0e\u5f53\u524d\u5378\u8f7d\u8bf7\u6c42\u4e0d\u4e00\u81f4\u6216\u786e\u8ba4\u9879\u4e0d\u5b8c\u6574\u3002");
        }

        var age = now.ToUniversalTime() - consent.ConfirmedAtUtc.ToUniversalTime();
        if (age > MaximumConsentAge || age < -MaximumClockSkew)
            missing.Add("\u6700\u7ec8\u786e\u8ba4\u5df2\u8fc7\u671f\u6216\u65f6\u95f4\u65e0\u6548\u3002");
        if (visualGate is not null
            && consent.ConfirmedAtUtc.ToUniversalTime() < visualGate.CapturedAtUtc.ToUniversalTime())
        {
            missing.Add("\u6700\u7ec8\u786e\u8ba4\u5fc5\u987b\u53d1\u751f\u5728\u754c\u9762\u9a8c\u8bc1\u4e4b\u540e\u3002");
        }
    }

    private static bool HasRequiredArguments(OperationDescriptor operation)
    {
        return TryNonEmptyString(operation, "executablePath")
            && TryString(operation, "arguments")
            && TryNonEmptyString(operation, "snapshotManifestPath")
            && TrySha256(operation, "snapshotSha256")
            && operation.Arguments.TryGetValue("snapshotCanRestoreApplication", out var rollback)
            && rollback is false
            && TryNonEmptyString(operation, "recoveryMethod")
            && TryNonEmptyString(operation, "recoveryReference");
    }

    private static bool TryCloneConfirmed(
        OperationDescriptor source,
        out OperationDescriptor? confirmed,
        out string? error)
    {
        var arguments = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var pair in source.Arguments)
        {
            if (!TryCloneValue(pair.Value, out var value))
            {
                confirmed = null;
                error = "\u5378\u8f7d\u8bf7\u6c42\u5305\u542b\u4e0d\u80fd\u5b89\u5168\u56fa\u5b9a\u7684\u53c2\u6570\u3002";
                return false;
            }
            arguments[pair.Key] = value;
        }

        confirmed = new OperationDescriptor
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
        error = null;
        return true;
    }

    private static bool TryCloneValue(object? source, out object? value)
    {
        switch (source)
        {
            case null:
            case string:
            case bool:
            case byte:
            case sbyte:
            case short:
            case ushort:
            case int:
            case uint:
            case long:
            case ulong:
            case float:
            case double:
            case decimal:
                value = source;
                return true;
            default:
                value = null;
                return false;
        }
    }

    public static string ComputeDescriptorSha256(OperationDescriptor operation)
    {
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
            Append(canonical, pair.Value?.GetType().FullName);
            Append(canonical, Convert.ToString(pair.Value, CultureInfo.InvariantCulture));
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())));
    }

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

    private static bool TryNonEmptyString(OperationDescriptor operation, string key) =>
        operation.Arguments.TryGetValue(key, out var value)
        && value is string text
        && !string.IsNullOrWhiteSpace(text);

    private static bool TryString(OperationDescriptor operation, string key) =>
        operation.Arguments.TryGetValue(key, out var value) && value is string;

    private static bool TrySha256(OperationDescriptor operation, string key) =>
        operation.Arguments.TryGetValue(key, out var value)
        && value is string text
        && IsSha256(text);

    private static bool IsSha256(string value) =>
        value.Length == 64 && value.All(Uri.IsHexDigit);

    private static OfficialUninstallElevatedRequestDraft Refused(
        IReadOnlyCollection<string> missing) =>
        new()
        {
            Status = OfficialUninstallElevatedRequestStatus.Refused,
            MissingRequirements = missing.Count > 0
                ? missing.ToArray()
                : ["\u5378\u8f7d\u8bf7\u6c42\u6ca1\u6709\u901a\u8fc7\u6700\u7ec8\u5b89\u5168\u68c0\u67e5\u3002"]
        };
}

public sealed class OfficialUninstallElevatedResponseEnvelope
{
    public required string RequestId { get; init; }
    public required OperationResult Result { get; init; }
}

public static class OfficialUninstallElevatedResponsePresenter
{
    public static OfficialUninstallElevatedResponseViewModel Create(
        OfficialUninstallElevatedRequestDraft request,
        OfficialUninstallElevatedResponseEnvelope response)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(response);

        if (!request.CanSubmit
            || !string.Equals(request.RequestId, response.RequestId, StringComparison.Ordinal)
            || response.Result.Payload is not OfficialUninstallHandlerPayload payload)
        {
            return InvalidResponse();
        }

        if (!payload.UninstallerStarted)
        {
            return new OfficialUninstallElevatedResponseViewModel
            {
                State = OfficialUninstallElevatedResponseState.NotStarted,
                Title = "\u5b98\u65b9\u5378\u8f7d\u5668\u6ca1\u6709\u542f\u52a8",
                Conclusion = "\u672c\u6b21\u6ca1\u6709\u5f00\u59cb\u5378\u8f7d\uff0c\u4e5f\u6ca1\u6709\u628a\u5b83\u8bf4\u6210\u5df2\u7ecf\u5b8c\u6210\u3002",
                AgentAdvice = "\u8bf7\u5148\u786e\u8ba4\u662f\u5426\u53d6\u6d88\u4e86\u7cfb\u7edf\u63d0\u793a\uff0c\u518d\u91cd\u65b0\u751f\u6210\u65b9\u6848\u3002"
            };
        }

        if (!payload.UninstallerCompleted)
        {
            return new OfficialUninstallElevatedResponseViewModel
            {
                State = OfficialUninstallElevatedResponseState.UninstallFailed,
                Title = "\u5b98\u65b9\u5378\u8f7d\u6ca1\u6709\u6210\u529f\u5b8c\u6210",
                Conclusion = "\u5378\u8f7d\u5668\u5df2\u7ed3\u675f\uff0c\u4f46\u672c\u6b21\u4e0d\u80fd\u786e\u8ba4\u8f6f\u4ef6\u5df2\u79fb\u9664\u3002",
                AgentAdvice = "\u4e0d\u8981\u624b\u52a8\u5220\u6587\u4ef6\uff1b\u5148\u91cd\u65b0\u68c0\u67e5\u8f6f\u4ef6\u662f\u5426\u4ecd\u5728\u3002"
            };
        }

        var postScan = OfficialUninstallPostScanPresenter.Create(
            request.Operation!.Title,
            payload.PostScan);
        return new OfficialUninstallElevatedResponseViewModel
        {
            State = OfficialUninstallElevatedResponseState.PostScanReady,
            Title = "\u5b98\u65b9\u5378\u8f7d\u5668\u5df2\u7ed3\u675f",
            Conclusion = payload.PostScan.Success
                ? "\u5df2\u6536\u5230\u4e0e\u672c\u6b21\u8bf7\u6c42\u5bf9\u5e94\u7684\u5378\u8f7d\u540e\u590d\u67e5\u7ed3\u679c\u3002"
                : "\u5378\u8f7d\u5668\u5df2\u7ed3\u675f\uff0c\u4f46\u590d\u67e5\u5c1a\u672a\u6210\u529f\u3002",
            AgentAdvice = "\u4e0b\u9762\u53ea\u5c55\u793a\u7ed3\u8bba\u548c\u5efa\u8bae\uff1b\u6280\u672f\u8bc1\u636e\u9700\u8981\u53e6\u884c\u6253\u5f00\u3002",
            PostScan = postScan
        };
    }

    private static OfficialUninstallElevatedResponseViewModel InvalidResponse() =>
        new()
        {
            State = OfficialUninstallElevatedResponseState.InvalidResponse,
            Title = "\u65e0\u6cd5\u786e\u8ba4\u672c\u6b21\u5378\u8f7d\u7ed3\u679c",
            Conclusion = "\u8fd4\u56de\u7ed3\u679c\u4e0e\u539f\u8bf7\u6c42\u4e0d\u5bf9\u5e94\u6216\u683c\u5f0f\u4e0d\u5b8c\u6574\u3002",
            AgentAdvice = "\u6211\u4e0d\u4f1a\u6839\u636e\u8fd9\u4efd\u7ed3\u679c\u5224\u65ad\u5378\u8f7d\u662f\u5426\u6210\u529f\uff0c\u9700\u8981\u91cd\u65b0\u626b\u63cf\u3002"
        };
}
