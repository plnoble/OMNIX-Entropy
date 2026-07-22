using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Css.Core.Uninstall;

namespace Css.Ipc.Uninstall;

public sealed record OfficialUninstallAuthenticatedMessage
{
    public required string ProtocolVersion { get; init; }
    public required string SessionId { get; init; }
    public required string MessageId { get; init; }
    public required string Nonce { get; init; }
    public required string RequestId { get; init; }
    public required string DescriptorSha256 { get; init; }
    public required DateTimeOffset SentAtUtc { get; init; }
    public required byte[] AuthenticationTag { get; init; }
    public required OfficialUninstallElevatedRequestDraft Request { get; init; }
}

public enum OfficialUninstallTransportStatus
{
    Completed,
    InvalidRequest,
    AuthenticationFailed,
    Expired,
    ReplayRejected,
    CapacityExceeded,
    EndpointFailed,
    ResponseRejected,
    PayloadRejected,
    ProtocolRejected,
    PeerRejected,
    AuthorizationFailed,
    StartupTimedOut,
    ResponseTimedOut,
    ConnectionFailed
}

public sealed class OfficialUninstallTransportResult
{
    public required OfficialUninstallTransportStatus Status { get; init; }
    public OfficialUninstallElevatedResponseEnvelope? Response { get; init; }
}

public sealed class OfficialUninstallAuthenticatedInMemoryClient : IDisposable
{
    private readonly string _sessionId;
    private readonly byte[] _sessionKey;
    private readonly Func<string> _messageIdFactory;
    private readonly Func<string> _nonceFactory;
    private bool _disposed;

    public OfficialUninstallAuthenticatedInMemoryClient(
        string sessionId,
        byte[] sessionKey,
        Func<string>? messageIdFactory = null,
        Func<string>? nonceFactory = null)
    {
        _sessionId = TransportAuthentication.ValidateSession(sessionId, sessionKey);
        _sessionKey = sessionKey.ToArray();
        _messageIdFactory = messageIdFactory ?? (() => Guid.NewGuid().ToString("N"));
        _nonceFactory = nonceFactory ?? (() => Guid.NewGuid().ToString("N"));
    }

    public OfficialUninstallAuthenticatedMessage CreateMessage(
        OfficialUninstallElevatedRequestDraft request,
        DateTimeOffset sentAtUtc)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(request);
        if (!request.CanSubmit
            || request.Operation is null
            || string.IsNullOrWhiteSpace(request.RequestId)
            || string.IsNullOrWhiteSpace(request.DescriptorSha256))
        {
            throw new ArgumentException("Only a ready request can be transported.", nameof(request));
        }

        var messageId = TransportAuthentication.ValidateToken(
            _messageIdFactory(),
            "message id");
        var nonce = TransportAuthentication.ValidateToken(_nonceFactory(), "nonce");
        var message = new OfficialUninstallAuthenticatedMessage
        {
            ProtocolVersion = TransportAuthentication.ProtocolVersion,
            SessionId = _sessionId,
            MessageId = messageId,
            Nonce = nonce,
            RequestId = request.RequestId,
            DescriptorSha256 = request.DescriptorSha256,
            SentAtUtc = sentAtUtc.ToUniversalTime(),
            AuthenticationTag = [],
            Request = request
        };

        return message with
        {
            AuthenticationTag = TransportAuthentication.ComputeTag(message, _sessionKey)
        };
    }

    public Task<OfficialUninstallTransportResult> SendAsync(
        OfficialUninstallElevatedRequestDraft request,
        OfficialUninstallAuthenticatedInMemoryEndpoint endpoint,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(endpoint);
        cancellationToken.ThrowIfCancellationRequested();
        var message = CreateMessage(request, now);
        return endpoint.HandleAsync(message, now, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        CryptographicOperations.ZeroMemory(_sessionKey);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

public sealed class OfficialUninstallAuthenticatedInMemoryEndpoint : IDisposable
{
    private const int MaximumReplayEntries = 1024;
    private static readonly TimeSpan MaximumMessageAge = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan MaximumRequestAge = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan MaximumClockSkew = TimeSpan.FromSeconds(30);

    private readonly string _sessionId;
    private readonly byte[] _sessionKey;
    private readonly Func<OfficialUninstallElevatedRequestDraft, CancellationToken,
        Task<OfficialUninstallElevatedResponseEnvelope>> _handler;
    private readonly object _sync = new();
    private readonly Dictionary<string, DateTimeOffset> _messageIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DateTimeOffset> _nonces = new(StringComparer.Ordinal);
    private bool _disposed;

    public OfficialUninstallAuthenticatedInMemoryEndpoint(
        string sessionId,
        byte[] sessionKey,
        Func<OfficialUninstallElevatedRequestDraft, CancellationToken,
            Task<OfficialUninstallElevatedResponseEnvelope>> handler)
    {
        _sessionId = TransportAuthentication.ValidateSession(sessionId, sessionKey);
        _sessionKey = sessionKey.ToArray();
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public async Task<OfficialUninstallTransportResult> HandleAsync(
        OfficialUninstallAuthenticatedMessage message,
        DateTimeOffset receivedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsStructurallyValid(message))
            return Result(OfficialUninstallTransportStatus.InvalidRequest);

        var received = receivedAtUtc.ToUniversalTime();
        var age = received - message.SentAtUtc.ToUniversalTime();
        if (age > MaximumMessageAge || age < -MaximumClockSkew)
            return Result(OfficialUninstallTransportStatus.Expired);
        var requestAge = received
            - message.Request.PreparedAtUtc!.Value.ToUniversalTime();
        if (requestAge > MaximumRequestAge || requestAge < -MaximumClockSkew)
            return Result(OfficialUninstallTransportStatus.Expired);

        var expectedTag = TransportAuthentication.ComputeTag(message, _sessionKey);
        if (message.AuthenticationTag.Length != expectedTag.Length
            || !CryptographicOperations.FixedTimeEquals(message.AuthenticationTag, expectedTag))
        {
            return Result(OfficialUninstallTransportStatus.AuthenticationFailed);
        }

        lock (_sync)
        {
            RemoveExpiredReplayEntries(received);
            if (_messageIds.ContainsKey(message.MessageId) || _nonces.ContainsKey(message.Nonce))
                return Result(OfficialUninstallTransportStatus.ReplayRejected);
            if (_messageIds.Count >= MaximumReplayEntries || _nonces.Count >= MaximumReplayEntries)
                return Result(OfficialUninstallTransportStatus.CapacityExceeded);

            var expires = received.Add(MaximumMessageAge).Add(MaximumClockSkew);
            _messageIds.Add(message.MessageId, expires);
            _nonces.Add(message.Nonce, expires);
        }

        OfficialUninstallElevatedResponseEnvelope response;
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
            return Result(OfficialUninstallTransportStatus.EndpointFailed);
        }

        if (response is null
            || !string.Equals(response.RequestId, message.RequestId, StringComparison.Ordinal))
        {
            return Result(OfficialUninstallTransportStatus.ResponseRejected);
        }

        return new OfficialUninstallTransportResult
        {
            Status = OfficialUninstallTransportStatus.Completed,
            Response = response
        };
    }

    private bool IsStructurallyValid(OfficialUninstallAuthenticatedMessage message)
    {
        var request = message.Request;
        if (!string.Equals(
                message.ProtocolVersion,
                TransportAuthentication.ProtocolVersion,
                StringComparison.Ordinal)
            || !string.Equals(message.SessionId, _sessionId, StringComparison.Ordinal)
            || !TransportAuthentication.IsValidToken(message.MessageId)
            || !TransportAuthentication.IsValidToken(message.Nonce)
            || !request.CanSubmit
            || request.Operation is null
            || !string.Equals(message.RequestId, request.RequestId, StringComparison.Ordinal)
            || !string.Equals(
                message.DescriptorSha256,
                request.DescriptorSha256,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var actualDescriptorHash = OfficialUninstallElevatedRequestComposer
            .ComputeDescriptorSha256(request.Operation);
        return string.Equals(
            actualDescriptorHash,
            message.DescriptorSha256,
            StringComparison.OrdinalIgnoreCase);
    }

    private void RemoveExpiredReplayEntries(DateTimeOffset now)
    {
        foreach (var key in _messageIds
                     .Where(pair => pair.Value < now)
                     .Select(pair => pair.Key)
                     .ToArray())
        {
            _messageIds.Remove(key);
        }

        foreach (var key in _nonces
                     .Where(pair => pair.Value < now)
                     .Select(pair => pair.Key)
                     .ToArray())
        {
            _nonces.Remove(key);
        }
    }

    private static OfficialUninstallTransportResult Result(
        OfficialUninstallTransportStatus status) =>
        new() { Status = status };

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
}

internal static class TransportAuthentication
{
    internal const string ProtocolVersion = "official-uninstall-in-memory-v2";

    internal static string ValidateSession(string sessionId, byte[] sessionKey)
    {
        ArgumentNullException.ThrowIfNull(sessionKey);
        if (!IsValidToken(sessionId))
            throw new ArgumentException("Session id is invalid.", nameof(sessionId));
        if (sessionKey.Length < 32)
            throw new ArgumentException("Session key must contain at least 32 bytes.", nameof(sessionKey));
        return sessionId;
    }

    internal static string ValidateToken(string value, string name)
    {
        if (!IsValidToken(value))
            throw new InvalidOperationException($"Generated {name} is invalid.");
        return value;
    }

    internal static bool IsValidToken(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 128;

    internal static byte[] ComputeTag(
        OfficialUninstallAuthenticatedMessage message,
        byte[] sessionKey)
    {
        var canonical = new StringBuilder();
        Append(canonical, message.ProtocolVersion);
        Append(canonical, message.SessionId);
        Append(canonical, message.MessageId);
        Append(canonical, message.Nonce);
        Append(canonical, message.RequestId);
        Append(canonical, message.DescriptorSha256);
        Append(
            canonical,
            message.Request.PreparedAtUtc?.ToUniversalTime().UtcDateTime.Ticks);
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
