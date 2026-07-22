using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Css.Core.Operations;
using Css.Core.Uninstall;

namespace Css.Ipc.Uninstall;

public sealed class OfficialUninstallPipeProtocolException : Exception
{
    public OfficialUninstallPipeProtocolException(
        OfficialUninstallTransportStatus status,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Status = status;
    }

    public OfficialUninstallTransportStatus Status { get; }
}

public static class OfficialUninstallPipeCodec
{
    public const int MaximumPayloadBytes = 64 * 1024;
    public const int SchemaVersion = 2;
    public const string RequestMessageType = "official-uninstall-request";
    public const string ResponseMessageType = "official-uninstall-response";

    private const int MaximumTextLength = 32 * 1024;
    private const int MaximumCollectionCount = 256;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 32
    };

    public static byte[] SerializeRequest(OfficialUninstallAuthenticatedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        ValidateReadyMessage(message);

        var wire = new RequestEnvelope
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
        };
        return SerializeBounded(wire);
    }

    public static OfficialUninstallAuthenticatedMessage DeserializeRequest(
        ReadOnlySpan<byte> payload)
    {
        var wire = DeserializeBounded<RequestEnvelope>(payload);
        if (wire.SchemaVersion != SchemaVersion
            || !string.Equals(wire.MessageType, RequestMessageType, StringComparison.Ordinal))
        {
            throw Protocol("The request schema or message type is not supported.");
        }

        try
        {
            var operation = FromWireOperation(wire.Operation);
            var request = new OfficialUninstallElevatedRequestDraft
            {
                Status = OfficialUninstallElevatedRequestStatus.Ready,
                MissingRequirements = [],
                PreparedAtUtc = new DateTimeOffset(
                    wire.PreparedAtUtcTicks,
                    TimeSpan.Zero),
                RequestId = RequireToken(wire.RequestId, "request id"),
                DescriptorSha256 = RequireSha256(wire.DescriptorSha256),
                Operation = operation
            };
            var message = new OfficialUninstallAuthenticatedMessage
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
        catch (OfficialUninstallPipeProtocolException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is FormatException
                or ArgumentException
                or ArgumentOutOfRangeException)
        {
            throw Protocol("The request contains invalid values.", exception);
        }
    }

    public static byte[] SerializeResponse(
        OfficialUninstallAuthenticatedMessage request,
        OfficialUninstallTransportResult result)
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
        var envelope = new ResponseEnvelope
        {
            Body = body,
            AuthenticationTag = Convert.ToBase64String(hmac.ComputeHash(bodyBytes))
        };
        return SerializeBounded(envelope);
    }

    public static OfficialUninstallTransportResult DeserializeResponse(
        ReadOnlySpan<byte> payload,
        OfficialUninstallAuthenticatedMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var envelope = DeserializeBounded<ResponseEnvelope>(payload);
        var body = envelope.Body ?? throw Protocol("The response body is missing.");
        byte[] actualTag;
        try
        {
            actualTag = Convert.FromBase64String(envelope.AuthenticationTag);
        }
        catch (FormatException exception)
        {
            throw ResponseRejected("The response authentication tag is invalid.", exception);
        }

        var bodyBytes = JsonSerializer.SerializeToUtf8Bytes(body, JsonOptions);
        using var hmac = new HMACSHA256(request.AuthenticationTag);
        var expectedTag = hmac.ComputeHash(bodyBytes);
        if (actualTag.Length != expectedTag.Length
            || !CryptographicOperations.FixedTimeEquals(actualTag, expectedTag))
        {
            throw ResponseRejected("The response authentication tag does not match.");
        }

        if (body.SchemaVersion != SchemaVersion
            || !string.Equals(body.MessageType, ResponseMessageType, StringComparison.Ordinal)
            || !string.Equals(body.ProtocolVersion, request.ProtocolVersion, StringComparison.Ordinal)
            || !string.Equals(body.SessionId, request.SessionId, StringComparison.Ordinal)
            || !string.Equals(body.RequestMessageId, request.MessageId, StringComparison.Ordinal)
            || !string.Equals(body.RequestId, request.RequestId, StringComparison.Ordinal)
            || !Enum.TryParse<OfficialUninstallTransportStatus>(body.Status, out var status))
        {
            throw ResponseRejected("The response correlation or schema is invalid.");
        }

        if (status != OfficialUninstallTransportStatus.Completed)
            return new OfficialUninstallTransportResult { Status = status };
        if (body.Result is null)
            throw ResponseRejected("A completed response has no typed result.");

        var response = FromWireResult(body.RequestId, body.Result);
        return new OfficialUninstallTransportResult
        {
            Status = OfficialUninstallTransportStatus.Completed,
            Response = response
        };
    }

    private static void ValidateReadyMessage(OfficialUninstallAuthenticatedMessage message)
    {
        if (!message.Request.CanSubmit
            || message.Request.Operation is null
            || !string.Equals(message.RequestId, message.Request.RequestId, StringComparison.Ordinal)
            || !string.Equals(
                message.DescriptorSha256,
                message.Request.DescriptorSha256,
                StringComparison.OrdinalIgnoreCase)
            || message.AuthenticationTag is not { Length: > 0 and <= 128 })
        {
            throw Protocol("Only a complete authenticated request can be serialized.");
        }
    }

    private static WireOperation ToWireOperation(OperationDescriptor operation)
    {
        ValidateCollection(operation.AffectedPaths, "affected paths");
        ValidateCollection(operation.AffectedRegistryKeys, "affected registry keys");
        ValidateCollection(operation.AffectedServices, "affected services");
        if (operation.Arguments.Count > MaximumCollectionCount)
            throw Protocol("The operation has too many arguments.");

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
                _ => throw Protocol("The operation contains an unsupported argument type.")
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
            throw Protocol("The operation is missing.");
        ValidateCollection(wire.AffectedPaths, "affected paths");
        ValidateCollection(wire.AffectedRegistryKeys, "affected registry keys");
        ValidateCollection(wire.AffectedServices, "affected services");
        if (wire.Arguments is null || wire.Arguments.Count > MaximumCollectionCount)
            throw Protocol("The operation argument collection is invalid.");
        if (!Enum.TryParse<OperationSource>(wire.Source, out var source)
            || !Enum.TryParse<RiskLevel>(wire.Risk, out var risk))
        {
            throw Protocol("The operation source or risk is invalid.");
        }

        var arguments = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var pair in wire.Arguments)
        {
            var key = RequireText(pair.Key, "argument key", 256);
            var value = pair.Value ?? throw Protocol("An operation argument is missing.");
            arguments.Add(key, value.Type switch
            {
                "string" when value.StringValue is not null && value.BooleanValue is null =>
                    RequireText(value.StringValue, "argument value", MaximumTextLength),
                "boolean" when value.BooleanValue.HasValue && value.StringValue is null =>
                    value.BooleanValue.Value,
                _ => throw Protocol("An operation argument has an invalid type or shape.")
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

    private static WireResult? ToWireResult(OfficialUninstallTransportResult result)
    {
        if (result.Status != OfficialUninstallTransportStatus.Completed)
            return null;
        var response = result.Response
            ?? throw ResponseRejected("The completed endpoint response is missing.");
        if (response.Result.Payload is not OfficialUninstallHandlerPayload payload)
            throw ResponseRejected("The endpoint response payload is not a supported uninstall result.");

        return new WireResult
        {
            Success = response.Result.Success,
            UninstallerStarted = payload.UninstallerStarted,
            UninstallerCompleted = payload.UninstallerCompleted,
            ExitCode = payload.ExitCode,
            RequiresPostScanRetry = payload.RequiresPostScanRetry,
            PostScanSuccess = payload.PostScan.Success,
            SoftwareStillPresent = payload.PostScan.SoftwareStillPresent,
            ResidueCandidateCount = Math.Max(0, payload.PostScan.ResidueCandidateCount),
            PathResidueCandidateCount = Math.Max(0, payload.PostScan.PathResidueCandidateCount),
            VerifiedBackgroundResidueCount = Math.Max(0, payload.PostScan.VerifiedBackgroundResidueCount),
            UnverifiedBackgroundHintCount = Math.Max(0, payload.PostScan.UnverifiedBackgroundHintCount),
            RequiresBackgroundRescan = payload.PostScan.RequiresBackgroundRescan
        };
    }

    private static OfficialUninstallElevatedResponseEnvelope FromWireResult(
        string requestId,
        WireResult wire)
    {
        var payload = new OfficialUninstallHandlerPayload
        {
            UninstallerStarted = wire.UninstallerStarted,
            UninstallerCompleted = wire.UninstallerCompleted,
            ExitCode = wire.ExitCode,
            RequiresPostScanRetry = wire.RequiresPostScanRetry,
            PostScan = new OfficialUninstallPostScanResult
            {
                Success = wire.PostScanSuccess,
                SoftwareStillPresent = wire.SoftwareStillPresent,
                ResidueCandidateCount = Math.Max(0, wire.ResidueCandidateCount),
                PathResidueCandidateCount = Math.Max(0, wire.PathResidueCandidateCount),
                VerifiedBackgroundResidueCount = Math.Max(0, wire.VerifiedBackgroundResidueCount),
                UnverifiedBackgroundHintCount = Math.Max(0, wire.UnverifiedBackgroundHintCount),
                RequiresBackgroundRescan = wire.RequiresBackgroundRescan,
                Summary = "Verified path-free IPC result."
            }
        };
        return new OfficialUninstallElevatedResponseEnvelope
        {
            RequestId = requestId,
            Result = wire.Success
                ? OperationResult.Ok("Verified path-free IPC result.", payload)
                : new OperationResult
                {
                    Success = false,
                    Error = "The official uninstaller did not complete successfully.",
                    Payload = payload
                }
        };
    }

    private static byte[] SerializeBounded<T>(T value)
    {
        byte[] payload;
        try
        {
            payload = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            throw Protocol("The message could not be serialized.", exception);
        }

        if (payload.Length == 0 || payload.Length > MaximumPayloadBytes)
            throw PayloadRejected("The serialized message exceeds the allowed size.");
        return payload;
    }

    private static T DeserializeBounded<T>(ReadOnlySpan<byte> payload)
    {
        if (payload.Length == 0 || payload.Length > MaximumPayloadBytes)
            throw PayloadRejected("The serialized message size is invalid.");
        try
        {
            return JsonSerializer.Deserialize<T>(payload, JsonOptions)
                ?? throw Protocol("The serialized message is empty.");
        }
        catch (OfficialUninstallPipeProtocolException)
        {
            throw;
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            throw Protocol("The serialized message is malformed.", exception);
        }
    }

    private static void ValidateCollection(IReadOnlyCollection<string>? values, string name)
    {
        if (values is null || values.Count > MaximumCollectionCount)
            throw Protocol($"The {name} collection is invalid.");
        foreach (var value in values)
            RequireText(value, name, MaximumTextLength);
    }

    private static string RequireToken(string? value, string name) =>
        !TransportAuthentication.IsValidToken(value)
            ? throw Protocol($"The {name} is invalid.")
            : value!;

    private static string RequireSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit)
            ? value
            : throw Protocol("The descriptor hash is invalid.");

    private static string RequireText(string? value, string name, int maximumLength) =>
        string.IsNullOrWhiteSpace(value) || value.Length > maximumLength
            ? throw Protocol($"The {name} is invalid.")
            : value;

    private static string? OptionalText(string? value, string name, int maximumLength) =>
        value is null ? null : RequireText(value, name, maximumLength);

    private static OfficialUninstallPipeProtocolException Protocol(
        string message,
        Exception? innerException = null) =>
        new(OfficialUninstallTransportStatus.ProtocolRejected, message, innerException);

    private static OfficialUninstallPipeProtocolException PayloadRejected(string message) =>
        new(OfficialUninstallTransportStatus.PayloadRejected, message);

    private static OfficialUninstallPipeProtocolException ResponseRejected(
        string message,
        Exception? innerException = null) =>
        new(OfficialUninstallTransportStatus.ResponseRejected, message, innerException);

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
        public required WireOperation Operation { get; init; }
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
        public required Dictionary<string, WireArgument> Arguments { get; init; }
    }

    private sealed class WireArgument
    {
        public required string Type { get; init; }
        public string? StringValue { get; init; }
        public bool? BooleanValue { get; init; }
    }

    private sealed class ResponseEnvelope
    {
        public required ResponseBody Body { get; init; }
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

    private sealed class WireResult
    {
        public required bool Success { get; init; }
        public required bool UninstallerStarted { get; init; }
        public required bool UninstallerCompleted { get; init; }
        public int? ExitCode { get; init; }
        public required bool RequiresPostScanRetry { get; init; }
        public required bool PostScanSuccess { get; init; }
        public required bool SoftwareStillPresent { get; init; }
        public required int ResidueCandidateCount { get; init; }
        public required int PathResidueCandidateCount { get; init; }
        public required int VerifiedBackgroundResidueCount { get; init; }
        public required int UnverifiedBackgroundHintCount { get; init; }
        public required bool RequiresBackgroundRescan { get; init; }
    }
}

public static class OfficialUninstallPipeFrame
{
    public static async Task WriteAsync(
        Stream stream,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (payload.Length == 0 || payload.Length > OfficialUninstallPipeCodec.MaximumPayloadBytes)
        {
            throw new OfficialUninstallPipeProtocolException(
                OfficialUninstallTransportStatus.PayloadRejected,
                "The frame payload size is invalid.");
        }

        var header = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(header, payload.Length);
        await stream.WriteAsync(header, cancellationToken);
        await stream.WriteAsync(payload, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    public static async Task<byte[]> ReadAsync(Stream stream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var header = new byte[sizeof(int)];
        await ReadExactlyAsync(stream, header, cancellationToken);
        var length = BinaryPrimitives.ReadInt32LittleEndian(header);
        if (length <= 0 || length > OfficialUninstallPipeCodec.MaximumPayloadBytes)
        {
            throw new OfficialUninstallPipeProtocolException(
                OfficialUninstallTransportStatus.PayloadRejected,
                "The frame payload size is invalid.");
        }

        var payload = new byte[length];
        await ReadExactlyAsync(stream, payload, cancellationToken);
        return payload;
    }

    private static async Task ReadExactlyAsync(
        Stream stream,
        Memory<byte> buffer,
        CancellationToken cancellationToken)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var count = await stream.ReadAsync(buffer[read..], cancellationToken);
            if (count == 0)
                throw new EndOfStreamException("The pipe closed before the frame was complete.");
            read += count;
        }
    }
}
