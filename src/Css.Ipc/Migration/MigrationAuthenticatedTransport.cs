using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Css.Core.Migration;

namespace Css.Ipc.Migration;

public enum MigrationTransportStatus
{
    Completed,
    InvalidRequest,
    Expired,
    AuthenticationFailed,
    ReplayRejected,
    CapacityExceeded,
    AuthorizationFailed,
    EndpointFailed,
    ResponseRejected,
    StartupTimedOut,
    ResponseTimedOut
}

public sealed class MigrationTransportResult
{
    public required MigrationTransportStatus Status { get; init; }
    public MigrationElevatedResponseEnvelope? Response { get; init; }
}

public sealed class MigrationAuthenticatedMessage
{
    public required string ProtocolVersion { get; init; }
    public required string SessionId { get; init; }
    public required string MessageId { get; init; }
    public required string Nonce { get; init; }
    public required string RequestId { get; init; }
    public required string DescriptorSha256 { get; init; }
    public required DateTimeOffset SentAtUtc { get; init; }
    public required byte[] AuthenticationTag { get; init; }
    public required MigrationElevatedRequestDraft Request { get; init; }
}

public sealed class MigrationAuthenticatedClient : IDisposable
{
    private readonly string _sessionId;
    private readonly byte[] _sessionKey;
    private bool _disposed;

    public MigrationAuthenticatedClient(string sessionId, byte[] sessionKey)
    {
        _sessionId = MigrationTransportAuthentication.ValidateSession(sessionId, sessionKey);
        _sessionKey = sessionKey.ToArray();
    }

    public MigrationAuthenticatedMessage CreateMessage(
        MigrationElevatedRequestDraft request,
        DateTimeOffset sentAtUtc)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(request);
        if (!request.CanSubmit)
            throw new InvalidOperationException("Only a ready migration request can be authenticated.");

        var message = new MigrationAuthenticatedMessage
        {
            ProtocolVersion = MigrationTransportAuthentication.ProtocolVersion,
            SessionId = _sessionId,
            MessageId = "migration-message-" + Guid.NewGuid().ToString("N"),
            Nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            RequestId = request.RequestId!,
            DescriptorSha256 = request.DescriptorSha256!,
            SentAtUtc = sentAtUtc.ToUniversalTime(),
            AuthenticationTag = [],
            Request = request
        };
        return CopyWithTag(
            message,
            MigrationTransportAuthentication.ComputeTag(message, _sessionKey));
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        CryptographicOperations.ZeroMemory(_sessionKey);
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    internal static MigrationAuthenticatedMessage CopyWithTag(
        MigrationAuthenticatedMessage message,
        byte[] tag) =>
        new()
        {
            ProtocolVersion = message.ProtocolVersion,
            SessionId = message.SessionId,
            MessageId = message.MessageId,
            Nonce = message.Nonce,
            RequestId = message.RequestId,
            DescriptorSha256 = message.DescriptorSha256,
            SentAtUtc = message.SentAtUtc,
            AuthenticationTag = tag,
            Request = message.Request
        };
}

public sealed class MigrationAuthenticatedEndpoint : IDisposable
{
    private const int MaximumReplayEntries = 1024;
    private static readonly TimeSpan MaximumMessageAge = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan MaximumRequestAge = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan MaximumClockSkew = TimeSpan.FromSeconds(30);

    private readonly string _sessionId;
    private readonly byte[] _sessionKey;
    private readonly Func<MigrationElevatedRequestDraft, CancellationToken,
        Task<MigrationElevatedResponseEnvelope>> _handler;
    private readonly object _sync = new();
    private readonly Dictionary<string, DateTimeOffset> _messageIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DateTimeOffset> _nonces = new(StringComparer.Ordinal);
    private bool _disposed;

    public MigrationAuthenticatedEndpoint(
        string sessionId,
        byte[] sessionKey,
        Func<MigrationElevatedRequestDraft, CancellationToken,
            Task<MigrationElevatedResponseEnvelope>> handler)
    {
        _sessionId = MigrationTransportAuthentication.ValidateSession(sessionId, sessionKey);
        _sessionKey = sessionKey.ToArray();
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public async Task<MigrationTransportResult> HandleAsync(
        MigrationAuthenticatedMessage message,
        DateTimeOffset receivedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsStructurallyValid(message))
            return Result(MigrationTransportStatus.InvalidRequest);

        var received = receivedAtUtc.ToUniversalTime();
        var messageAge = received - message.SentAtUtc.ToUniversalTime();
        var requestAge = received - message.Request.PreparedAtUtc!.Value.ToUniversalTime();
        if (messageAge > MaximumMessageAge || messageAge < -MaximumClockSkew
            || requestAge > MaximumRequestAge || requestAge < -MaximumClockSkew)
        {
            return Result(MigrationTransportStatus.Expired);
        }

        var expectedTag = MigrationTransportAuthentication.ComputeTag(message, _sessionKey);
        try
        {
            if (message.AuthenticationTag.Length != expectedTag.Length
                || !CryptographicOperations.FixedTimeEquals(message.AuthenticationTag, expectedTag))
            {
                return Result(MigrationTransportStatus.AuthenticationFailed);
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(expectedTag);
        }

        lock (_sync)
        {
            RemoveExpired(received);
            if (_messageIds.ContainsKey(message.MessageId) || _nonces.ContainsKey(message.Nonce))
                return Result(MigrationTransportStatus.ReplayRejected);
            if (_messageIds.Count >= MaximumReplayEntries || _nonces.Count >= MaximumReplayEntries)
                return Result(MigrationTransportStatus.CapacityExceeded);
            var expires = received.Add(MaximumMessageAge).Add(MaximumClockSkew);
            _messageIds.Add(message.MessageId, expires);
            _nonces.Add(message.Nonce, expires);
        }

        MigrationElevatedResponseEnvelope response;
        try
        {
            response = await _handler(message.Request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Result(MigrationTransportStatus.EndpointFailed);
        }

        return response is not null
            && string.Equals(response.RequestId, message.RequestId, StringComparison.Ordinal)
                ? new MigrationTransportResult
                {
                    Status = MigrationTransportStatus.Completed,
                    Response = response
                }
                : Result(MigrationTransportStatus.ResponseRejected);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        CryptographicOperations.ZeroMemory(_sessionKey);
        lock (_sync)
        {
            _messageIds.Clear();
            _nonces.Clear();
        }
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private bool IsStructurallyValid(MigrationAuthenticatedMessage message)
    {
        var request = message.Request;
        if (!string.Equals(
                message.ProtocolVersion,
                MigrationTransportAuthentication.ProtocolVersion,
                StringComparison.Ordinal)
            || !string.Equals(message.SessionId, _sessionId, StringComparison.Ordinal)
            || !MigrationTransportAuthentication.IsToken(message.MessageId)
            || !MigrationTransportAuthentication.IsToken(message.Nonce)
            || !request.CanSubmit
            || request.Operation is null
            || !string.Equals(message.RequestId, request.RequestId, StringComparison.Ordinal)
            || !HashesEqual(message.DescriptorSha256, request.DescriptorSha256))
        {
            return false;
        }

        string actual;
        try
        {
            actual = MigrationElevatedRequestComposer.ComputeDescriptorSha256(request.Operation);
        }
        catch
        {
            return false;
        }
        return HashesEqual(actual, message.DescriptorSha256);
    }

    private void RemoveExpired(DateTimeOffset now)
    {
        foreach (var key in _messageIds.Where(pair => pair.Value < now).Select(pair => pair.Key).ToArray())
            _messageIds.Remove(key);
        foreach (var key in _nonces.Where(pair => pair.Value < now).Select(pair => pair.Key).ToArray())
            _nonces.Remove(key);
    }

    private static bool HashesEqual(string? left, string? right)
    {
        if (left is not { Length: 64 } || right is not { Length: 64 }
            || !left.All(Uri.IsHexDigit) || !right.All(Uri.IsHexDigit))
        {
            return false;
        }
        var leftBytes = Convert.FromHexString(left);
        var rightBytes = Convert.FromHexString(right);
        try
        {
            return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(leftBytes);
            CryptographicOperations.ZeroMemory(rightBytes);
        }
    }

    private static MigrationTransportResult Result(MigrationTransportStatus status) =>
        new() { Status = status };
}

internal static class MigrationTransportAuthentication
{
    internal const string ProtocolVersion = "migration-execution-v1";

    internal static string ValidateSession(string sessionId, byte[] sessionKey)
    {
        ArgumentNullException.ThrowIfNull(sessionKey);
        if (!IsToken(sessionId))
            throw new ArgumentException("The migration session id is invalid.", nameof(sessionId));
        if (sessionKey.Length < 32)
            throw new ArgumentException("The migration session key is too short.", nameof(sessionKey));
        return sessionId;
    }

    internal static bool IsToken(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Length <= 128
        && value.IndexOfAny(['\\', '/']) < 0;

    internal static byte[] ComputeTag(MigrationAuthenticatedMessage message, byte[] sessionKey)
    {
        var canonical = new StringBuilder();
        Append(canonical, message.ProtocolVersion);
        Append(canonical, message.SessionId);
        Append(canonical, message.MessageId);
        Append(canonical, message.Nonce);
        Append(canonical, message.RequestId);
        Append(canonical, message.DescriptorSha256);
        Append(canonical, message.Request.PreparedAtUtc?.ToUniversalTime().UtcDateTime.Ticks);
        Append(canonical, message.SentAtUtc.ToUniversalTime().UtcDateTime.Ticks);
        using var hmac = new HMACSHA256(sessionKey);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString()));
    }

    private static void Append(StringBuilder builder, object? value)
    {
        var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        builder.Append(text.Length).Append(':').Append(text).Append(';');
    }
}
