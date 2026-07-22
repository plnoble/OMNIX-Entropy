using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Css.Core.Migration;
using Css.Core.Operations;

namespace Css.Ipc.Migration;

public sealed class MigrationPipeProtocolException : Exception
{
    public MigrationPipeProtocolException(
        MigrationTransportStatus status,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Status = status;
    }

    public MigrationTransportStatus Status { get; }
}

public static class MigrationPipeCodec
{
    public const int MaximumPayloadBytes = 128 * 1024;
    public const int SchemaVersion = 1;
    public const string RequestMessageType = "migration-execution-request";
    public const string ResponseMessageType = "migration-execution-response";

    private const int MaximumTextLength = 32 * 1024;
    private const int MaximumCollectionCount = 256;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 32
    };

    public static byte[] SerializeRequest(MigrationAuthenticatedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        ValidateReadyMessage(message);
        return SerializeBounded(new RequestEnvelope
        {
            SchemaVersion = SchemaVersion,
            MessageType = RequestMessageType,
            ProtocolVersion = message.ProtocolVersion,
            SessionId = message.SessionId,
            MessageId = message.MessageId,
            Nonce = message.Nonce,
            RequestId = message.RequestId,
            DescriptorSha256 = message.DescriptorSha256,
            PreparedAtUtcTicks = message.Request.PreparedAtUtc!.Value
                .ToUniversalTime().UtcDateTime.Ticks,
            SentAtUtcTicks = message.SentAtUtc.ToUniversalTime().UtcDateTime.Ticks,
            AuthenticationTag = Convert.ToBase64String(message.AuthenticationTag),
            Operation = ToWireOperation(message.Request.Operation!)
        });
    }

    public static MigrationAuthenticatedMessage DeserializeRequest(ReadOnlySpan<byte> payload)
    {
        var wire = DeserializeBounded<RequestEnvelope>(payload);
        if (wire.SchemaVersion != SchemaVersion
            || !string.Equals(wire.MessageType, RequestMessageType, StringComparison.Ordinal))
        {
            throw Protocol("The migration request schema or message type is not supported.");
        }

        try
        {
            var operation = FromWireOperation(wire.Operation);
            var request = new MigrationElevatedRequestDraft
            {
                Status = MigrationElevatedRequestStatus.Ready,
                MissingRequirements = [],
                PreparedAtUtc = new DateTimeOffset(wire.PreparedAtUtcTicks, TimeSpan.Zero),
                RequestId = RequireToken(wire.RequestId, "request id"),
                DescriptorSha256 = RequireSha256(wire.DescriptorSha256),
                Operation = operation
            };
            var message = new MigrationAuthenticatedMessage
            {
                ProtocolVersion = RequireToken(wire.ProtocolVersion, "protocol version"),
                SessionId = RequireToken(wire.SessionId, "session id"),
                MessageId = RequireToken(wire.MessageId, "message id"),
                Nonce = RequireToken(wire.Nonce, "nonce"),
                RequestId = request.RequestId,
                DescriptorSha256 = request.DescriptorSha256,
                SentAtUtc = new DateTimeOffset(wire.SentAtUtcTicks, TimeSpan.Zero),
                AuthenticationTag = Convert.FromBase64String(wire.AuthenticationTag),
                Request = request
            };
            ValidateReadyMessage(message);
            return message;
        }
        catch (MigrationPipeProtocolException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is FormatException or ArgumentException or ArgumentOutOfRangeException)
        {
            throw Protocol("The migration request contains invalid values.", exception);
        }
    }

    public static byte[] SerializeResponse(
        MigrationAuthenticatedMessage request,
        MigrationTransportResult result)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(result);
        var body = new ResponseBody
        {
            SchemaVersion = SchemaVersion,
            MessageType = ResponseMessageType,
            ProtocolVersion = request.ProtocolVersion,
            SessionId = request.SessionId,
            RequestMessageId = request.MessageId,
            RequestId = request.RequestId,
            Status = result.Status.ToString(),
            Result = ToWireResult(result)
        };
        var bodyBytes = JsonSerializer.SerializeToUtf8Bytes(body, JsonOptions);
        using var hmac = new HMACSHA256(request.AuthenticationTag);
        return SerializeBounded(new ResponseEnvelope
        {
            Body = body,
            AuthenticationTag = Convert.ToBase64String(hmac.ComputeHash(bodyBytes))
        });
    }

    public static MigrationTransportResult DeserializeResponse(
        ReadOnlySpan<byte> payload,
        MigrationAuthenticatedMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var envelope = DeserializeBounded<ResponseEnvelope>(payload);
        var body = envelope.Body ?? throw Protocol("The migration response body is missing.");
        byte[] actualTag;
        try
        {
            actualTag = Convert.FromBase64String(envelope.AuthenticationTag);
        }
        catch (FormatException exception)
        {
            throw ResponseRejected("The migration response tag is invalid.", exception);
        }

        var bodyBytes = JsonSerializer.SerializeToUtf8Bytes(body, JsonOptions);
        using var hmac = new HMACSHA256(request.AuthenticationTag);
        var expectedTag = hmac.ComputeHash(bodyBytes);
        try
        {
            if (actualTag.Length != expectedTag.Length
                || !CryptographicOperations.FixedTimeEquals(actualTag, expectedTag))
            {
                throw ResponseRejected("The migration response tag does not match.");
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(actualTag);
            CryptographicOperations.ZeroMemory(expectedTag);
        }

        if (body.SchemaVersion != SchemaVersion
            || !string.Equals(body.MessageType, ResponseMessageType, StringComparison.Ordinal)
            || !string.Equals(body.ProtocolVersion, request.ProtocolVersion, StringComparison.Ordinal)
            || !string.Equals(body.SessionId, request.SessionId, StringComparison.Ordinal)
            || !string.Equals(body.RequestMessageId, request.MessageId, StringComparison.Ordinal)
            || !string.Equals(body.RequestId, request.RequestId, StringComparison.Ordinal)
            || !Enum.TryParse<MigrationTransportStatus>(body.Status, out var status))
        {
            throw ResponseRejected("The migration response correlation or schema is invalid.");
        }

        if (status != MigrationTransportStatus.Completed)
            return new MigrationTransportResult { Status = status };
        if (body.Result is null)
            throw ResponseRejected("A completed migration response has no result.");

        return new MigrationTransportResult
        {
            Status = MigrationTransportStatus.Completed,
            Response = FromWireResult(body.RequestId, body.Result)
        };
    }

    private static WireOperation ToWireOperation(OperationDescriptor operation)
    {
        ValidateCollection(operation.AffectedPaths, "affected paths");
        ValidateCollection(operation.AffectedRegistryKeys, "affected registry keys");
        ValidateCollection(operation.AffectedServices, "affected services");
        if (operation.Arguments.Count > MaximumCollectionCount)
            throw Protocol("The migration operation has too many arguments.");

        var arguments = new Dictionary<string, WireArgument>(StringComparer.Ordinal);
        foreach (var pair in operation.Arguments)
        {
            var key = RequireText(pair.Key, "argument key", 256);
            arguments.Add(key, pair.Value switch
            {
                string text => new WireArgument
                {
                    Type = "string",
                    StringValue = RequireText(text, "argument value", MaximumTextLength)
                },
                bool boolean => new WireArgument
                {
                    Type = "boolean",
                    BooleanValue = boolean
                },
                string[] array => StringListArgument(array),
                IReadOnlyList<string> list => StringListArgument(list),
                _ => throw Protocol("The migration operation contains an unsupported argument type.")
            });
        }

        return new WireOperation
        {
            Kind = RequireText(operation.Kind, "operation kind", 256),
            Title = RequireText(operation.Title, "operation title", 1024),
            Source = operation.Source.ToString(),
            Risk = operation.Risk.ToString(),
            IsDestructive = operation.IsDestructive,
            RequiresElevation = operation.RequiresElevation,
            RequiresSnapshot = operation.RequiresSnapshot,
            SnapshotId = OptionalText(operation.SnapshotId, "snapshot id", 256),
            RollbackRequired = operation.RollbackRequired,
            ConfirmationAccepted = operation.ConfirmationAccepted,
            EvidenceSummary = OptionalText(operation.EvidenceSummary, "evidence summary", 4096),
            EstimatedImpactBytes = operation.EstimatedImpactBytes,
            ConfirmationText = OptionalText(operation.ConfirmationText, "confirmation text", 4096),
            AffectedPaths = operation.AffectedPaths.ToArray(),
            AffectedRegistryKeys = operation.AffectedRegistryKeys.ToArray(),
            AffectedServices = operation.AffectedServices.ToArray(),
            Arguments = arguments
        };
    }

    private static OperationDescriptor FromWireOperation(WireOperation? wire)
    {
        if (wire is null)
            throw Protocol("The migration operation is missing.");
        ValidateCollection(wire.AffectedPaths, "affected paths");
        ValidateCollection(wire.AffectedRegistryKeys, "affected registry keys");
        ValidateCollection(wire.AffectedServices, "affected services");
        if (wire.Arguments is null || wire.Arguments.Count > MaximumCollectionCount)
            throw Protocol("The migration argument collection is invalid.");
        if (!Enum.TryParse<OperationSource>(wire.Source, out var source)
            || !Enum.TryParse<RiskLevel>(wire.Risk, out var risk))
        {
            throw Protocol("The migration operation source or risk is invalid.");
        }

        var arguments = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var pair in wire.Arguments)
        {
            var key = RequireText(pair.Key, "argument key", 256);
            var value = pair.Value ?? throw Protocol("A migration argument is missing.");
            arguments.Add(key, value.Type switch
            {
                "string" when value.StringValue is not null
                              && value.BooleanValue is null
                              && value.StringListValue is null =>
                    RequireText(value.StringValue, "argument value", MaximumTextLength),
                "boolean" when value.BooleanValue.HasValue
                               && value.StringValue is null
                               && value.StringListValue is null =>
                    value.BooleanValue.Value,
                "string-list" when value.StringListValue is not null
                                   && value.StringValue is null
                                   && value.BooleanValue is null =>
                    ValidateAndCopyList(value.StringListValue, "argument list"),
                _ => throw Protocol("A migration argument has an invalid type or shape.")
            });
        }

        return new OperationDescriptor
        {
            Kind = RequireText(wire.Kind, "operation kind", 256),
            Title = RequireText(wire.Title, "operation title", 1024),
            Source = source,
            Risk = risk,
            IsDestructive = wire.IsDestructive,
            RequiresElevation = wire.RequiresElevation,
            RequiresSnapshot = wire.RequiresSnapshot,
            SnapshotId = OptionalText(wire.SnapshotId, "snapshot id", 256),
            RollbackRequired = wire.RollbackRequired,
            ConfirmationAccepted = wire.ConfirmationAccepted,
            EvidenceSummary = OptionalText(wire.EvidenceSummary, "evidence summary", 4096),
            EstimatedImpactBytes = wire.EstimatedImpactBytes,
            ConfirmationText = OptionalText(wire.ConfirmationText, "confirmation text", 4096),
            AffectedPaths = wire.AffectedPaths.ToArray(),
            AffectedRegistryKeys = wire.AffectedRegistryKeys.ToArray(),
            AffectedServices = wire.AffectedServices.ToArray(),
            Arguments = arguments
        };
    }

    private static WireArgument StringListArgument(IReadOnlyList<string> values) =>
        new()
        {
            Type = "string-list",
            StringListValue = ValidateAndCopyList(values, "argument list")
        };

    private static string[] ValidateAndCopyList(IReadOnlyList<string> values, string name)
    {
        ValidateCollection(values, name);
        return values.Select(value => RequireText(value, name, 4096)).ToArray();
    }

    private static WireResult? ToWireResult(MigrationTransportResult result)
    {
        if (result.Status != MigrationTransportStatus.Completed)
            return null;
        var response = result.Response
            ?? throw ResponseRejected("The completed migration response is missing.");
        if (response.Result.Payload is not MigrationExecutionResult payload)
            throw ResponseRejected("The migration response payload has an invalid type.");
        return new WireResult
        {
            Success = response.Result.Success,
            ExecutionStatus = payload.Status.ToString(),
            MovedPathCount = Math.Max(0, payload.MovedPathCount),
            RollbackAttempted = payload.RollbackAttempted,
            RollbackSucceeded = payload.RollbackSucceeded
        };
    }

    private static MigrationElevatedResponseEnvelope FromWireResult(
        string requestId,
        WireResult wire)
    {
        if (!Enum.TryParse<MigrationExecutionStatus>(wire.ExecutionStatus, out var status)
            || wire.MovedPathCount is < 0 or > 32)
        {
            throw ResponseRejected("The migration result values are invalid.");
        }
        var summary = status switch
        {
            MigrationExecutionStatus.Completed => "Migration completed and monitoring started.",
            MigrationExecutionStatus.Refused => "Migration did not start.",
            MigrationExecutionStatus.FailedRolledBack => "Migration failed and was restored.",
            MigrationExecutionStatus.FailedRollbackIncomplete => "Migration needs manual inspection.",
            _ => "Migration result is unavailable."
        };
        return new MigrationElevatedResponseEnvelope
        {
            RequestId = RequireToken(requestId, "request id"),
            Result = new OperationResult
            {
                Success = wire.Success,
                Summary = wire.Success ? summary : null,
                Error = wire.Success ? null : summary,
                Payload = new MigrationExecutionResult
                {
                    Status = status,
                    Summary = summary,
                    MovedPathCount = wire.MovedPathCount,
                    RollbackAttempted = wire.RollbackAttempted,
                    RollbackSucceeded = wire.RollbackSucceeded,
                    Errors = []
                }
            }
        };
    }

    private static void ValidateReadyMessage(MigrationAuthenticatedMessage message)
    {
        if (!message.Request.CanSubmit
            || message.Request.Operation is null
            || !string.Equals(message.RequestId, message.Request.RequestId, StringComparison.Ordinal)
            || !string.Equals(
                message.DescriptorSha256,
                message.Request.DescriptorSha256,
                StringComparison.OrdinalIgnoreCase)
            || message.AuthenticationTag is not { Length: 32 })
        {
            throw Protocol("Only a complete authenticated migration request can be serialized.");
        }
    }

    private static byte[] SerializeBounded<T>(T value)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        if (payload.Length == 0 || payload.Length > MaximumPayloadBytes)
            throw Protocol("The migration pipe payload size is invalid.");
        return payload;
    }

    private static T DeserializeBounded<T>(ReadOnlySpan<byte> payload)
    {
        if (payload.Length == 0 || payload.Length > MaximumPayloadBytes)
            throw Protocol("The migration pipe payload size is invalid.");
        try
        {
            return JsonSerializer.Deserialize<T>(payload, JsonOptions)
                ?? throw Protocol("The migration pipe payload is empty.");
        }
        catch (MigrationPipeProtocolException)
        {
            throw;
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            throw Protocol("The migration pipe payload is malformed.", exception);
        }
    }

    private static void ValidateCollection(IReadOnlyList<string>? values, string name)
    {
        if (values is null || values.Count > MaximumCollectionCount)
            throw Protocol($"The {name} collection is invalid.");
        foreach (var value in values)
            _ = RequireText(value, name, MaximumTextLength);
    }

    private static string RequireToken(string? value, string name)
    {
        if (!MigrationTransportAuthentication.IsToken(value))
            throw Protocol($"The {name} is invalid.");
        return value!;
    }

    private static string RequireSha256(string? value)
    {
        if (value is not { Length: 64 } || !value.All(Uri.IsHexDigit))
            throw Protocol("The migration descriptor SHA-256 is invalid.");
        return value.ToUpperInvariant();
    }

    private static string RequireText(string? value, string name, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength)
            throw Protocol($"The {name} text is invalid.");
        return value;
    }

    private static string? OptionalText(string? value, string name, int maximumLength) =>
        value is null ? null : RequireText(value, name, maximumLength);

    private static MigrationPipeProtocolException Protocol(
        string message,
        Exception? innerException = null) =>
        new(MigrationTransportStatus.InvalidRequest, message, innerException);

    private static MigrationPipeProtocolException ResponseRejected(
        string message,
        Exception? innerException = null) =>
        new(MigrationTransportStatus.ResponseRejected, message, innerException);

    private sealed class RequestEnvelope
    {
        public required int SchemaVersion { get; init; }
        public required string MessageType { get; init; }
        public required string ProtocolVersion { get; init; }
        public required string SessionId { get; init; }
        public required string MessageId { get; init; }
        public required string Nonce { get; init; }
        public required string RequestId { get; init; }
        public required string DescriptorSha256 { get; init; }
        public required long PreparedAtUtcTicks { get; init; }
        public required long SentAtUtcTicks { get; init; }
        public required string AuthenticationTag { get; init; }
        public WireOperation? Operation { get; init; }
    }

    private sealed class ResponseEnvelope
    {
        public ResponseBody? Body { get; init; }
        public required string AuthenticationTag { get; init; }
    }

    private sealed class ResponseBody
    {
        public required int SchemaVersion { get; init; }
        public required string MessageType { get; init; }
        public required string ProtocolVersion { get; init; }
        public required string SessionId { get; init; }
        public required string RequestMessageId { get; init; }
        public required string RequestId { get; init; }
        public required string Status { get; init; }
        public WireResult? Result { get; init; }
    }

    private sealed class WireOperation
    {
        public required string Kind { get; init; }
        public required string Title { get; init; }
        public required string Source { get; init; }
        public required string Risk { get; init; }
        public required bool IsDestructive { get; init; }
        public required bool RequiresElevation { get; init; }
        public required bool RequiresSnapshot { get; init; }
        public string? SnapshotId { get; init; }
        public required bool RollbackRequired { get; init; }
        public required bool ConfirmationAccepted { get; init; }
        public string? EvidenceSummary { get; init; }
        public required long EstimatedImpactBytes { get; init; }
        public string? ConfirmationText { get; init; }
        public required string[] AffectedPaths { get; init; }
        public required string[] AffectedRegistryKeys { get; init; }
        public required string[] AffectedServices { get; init; }
        public Dictionary<string, WireArgument>? Arguments { get; init; }
    }

    private sealed class WireArgument
    {
        public required string Type { get; init; }
        public string? StringValue { get; init; }
        public bool? BooleanValue { get; init; }
        public string[]? StringListValue { get; init; }
    }

    private sealed class WireResult
    {
        public required bool Success { get; init; }
        public required string ExecutionStatus { get; init; }
        public required int MovedPathCount { get; init; }
        public required bool RollbackAttempted { get; init; }
        public required bool RollbackSucceeded { get; init; }
    }
}
