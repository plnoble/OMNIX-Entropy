using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Css.Ipc.Uninstall;

public enum OfficialUninstallSessionBootstrapStatus
{
    ProtocolRejected,
    PayloadRejected,
    ReplayRejected,
    CapacityExceeded,
    KeyConfirmationFailed,
    TimedOut
}

public sealed class OfficialUninstallSessionBootstrapException : Exception
{
    public OfficialUninstallSessionBootstrapException(
        OfficialUninstallSessionBootstrapStatus status,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Status = status;
    }

    public OfficialUninstallSessionBootstrapStatus Status { get; }
}

public sealed record OfficialUninstallSessionBootstrapContext
{
    public required string PipeName { get; init; }
    public required string SessionId { get; init; }
    public required OfficialUninstallPipePeerIdentity Client { get; init; }
    public required OfficialUninstallPipePeerIdentity Server { get; init; }
}

public sealed class OfficialUninstallSessionKey : IDisposable
{
    private readonly object _sync = new();
    private byte[]? _key;

    internal OfficialUninstallSessionKey(
        string sessionId,
        byte[] key,
        byte[] transcriptSha256)
    {
        SessionId = sessionId;
        _key = key;
        TranscriptSha256 = Convert.ToHexString(transcriptSha256);
    }

    public string SessionId { get; }
    public string TranscriptSha256 { get; }
    public bool IsDisposed
    {
        get
        {
            lock (_sync)
                return _key is null;
        }
    }

    public byte[] ExportCopy()
    {
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_key is null, this);
            return _key!.ToArray();
        }
    }

    ~OfficialUninstallSessionKey()
    {
        ZeroOwnedKey();
    }

    public void Dispose()
    {
        ZeroOwnedKey();
        GC.SuppressFinalize(this);
    }

    private void ZeroOwnedKey()
    {
        lock (_sync)
        {
            if (_key is null)
                return;
            CryptographicOperations.ZeroMemory(_key);
            _key = null;
        }
    }
}

public sealed class OfficialUninstallSessionBootstrapReplayGuard
{
    private const int DefaultCapacity = 1024;
    private static readonly TimeSpan DefaultRetention = TimeSpan.FromMinutes(10);

    private readonly int _capacity;
    private readonly TimeSpan _retention;
    private readonly object _sync = new();
    private readonly Dictionary<string, DateTimeOffset> _clientNonces =
        new(StringComparer.Ordinal);

    public OfficialUninstallSessionBootstrapReplayGuard(
        int capacity = DefaultCapacity,
        TimeSpan? retention = null)
    {
        if (capacity <= 0 || capacity > 16_384)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        var actualRetention = retention ?? DefaultRetention;
        if (actualRetention <= TimeSpan.Zero || actualRetention > TimeSpan.FromHours(1))
            throw new ArgumentOutOfRangeException(nameof(retention));
        _capacity = capacity;
        _retention = actualRetention;
    }

    internal OfficialUninstallSessionBootstrapStatus? TryRegister(
        ReadOnlySpan<byte> clientNonce,
        DateTimeOffset now)
    {
        var token = Convert.ToHexString(SHA256.HashData(clientNonce));
        lock (_sync)
        {
            foreach (var expired in _clientNonces
                         .Where(pair => pair.Value < now)
                         .Select(pair => pair.Key)
                         .ToArray())
            {
                _clientNonces.Remove(expired);
            }

            if (_clientNonces.ContainsKey(token))
                return OfficialUninstallSessionBootstrapStatus.ReplayRejected;
            if (_clientNonces.Count >= _capacity)
                return OfficialUninstallSessionBootstrapStatus.CapacityExceeded;
            _clientNonces.Add(token, now.Add(_retention));
            return null;
        }
    }
}

public sealed class OfficialUninstallSessionBootstrapHello
{
    public required string SessionId { get; init; }
    public required byte[] Nonce { get; init; }
    public required byte[] PublicKey { get; init; }
}

public static class OfficialUninstallSessionBootstrapCodec
{
    public const int MaximumPayloadBytes = 16 * 1024;
    public const string ProtocolVersion = "official-uninstall-bootstrap-v1";

    private const int SchemaVersion = 1;
    private const string ClientHelloType = "client-hello";
    private const string ServerHelloType = "server-hello";
    private const string FinishedType = "finished";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 8
    };

    public static byte[] EncodeClientHello(
        string sessionId,
        byte[] nonce,
        byte[] publicKey) =>
        EncodeHello(ClientHelloType, sessionId, nonce, publicKey);

    public static byte[] EncodeServerHello(
        string sessionId,
        byte[] nonce,
        byte[] publicKey) =>
        EncodeHello(ServerHelloType, sessionId, nonce, publicKey);

    public static OfficialUninstallSessionBootstrapHello DecodeClientHello(
        ReadOnlySpan<byte> payload) =>
        DecodeHello(payload, ClientHelloType);

    public static OfficialUninstallSessionBootstrapHello DecodeServerHello(
        ReadOnlySpan<byte> payload) =>
        DecodeHello(payload, ServerHelloType);

    public static byte[] EncodeFinished(
        string sessionId,
        string role,
        byte[] authenticationTag)
    {
        ValidateToken(sessionId, "session id");
        if (role is not ("server-finished" or "client-finished"))
            throw Protocol("The finished role is invalid.");
        if (authenticationTag is not { Length: 32 })
            throw Protocol("The finished authentication tag is invalid.");
        return Serialize(new FinishedWire
        {
            SchemaVersion = SchemaVersion,
            MessageType = FinishedType,
            ProtocolVersion = ProtocolVersion,
            SessionId = sessionId,
            Role = role,
            AuthenticationTag = Convert.ToBase64String(authenticationTag)
        });
    }

    public static byte[] DecodeFinished(
        ReadOnlySpan<byte> payload,
        string expectedSessionId,
        string expectedRole)
    {
        var wire = Deserialize<FinishedWire>(payload);
        if (wire.SchemaVersion != SchemaVersion
            || !string.Equals(wire.MessageType, FinishedType, StringComparison.Ordinal)
            || !string.Equals(wire.ProtocolVersion, ProtocolVersion, StringComparison.Ordinal)
            || !string.Equals(wire.SessionId, expectedSessionId, StringComparison.Ordinal)
            || !string.Equals(wire.Role, expectedRole, StringComparison.Ordinal))
        {
            throw Protocol("The finished message schema, session, or role is invalid.");
        }

        try
        {
            var tag = Convert.FromBase64String(wire.AuthenticationTag);
            if (tag.Length != 32)
                throw Protocol("The finished authentication tag length is invalid.");
            return tag;
        }
        catch (FormatException exception)
        {
            throw Protocol("The finished authentication tag is malformed.", exception);
        }
    }

    private static byte[] EncodeHello(
        string messageType,
        string sessionId,
        byte[] nonce,
        byte[] publicKey)
    {
        ValidateToken(sessionId, "session id");
        ValidateNonce(nonce);
        ValidatePublicKey(publicKey);
        return Serialize(new HelloWire
        {
            SchemaVersion = SchemaVersion,
            MessageType = messageType,
            ProtocolVersion = ProtocolVersion,
            SessionId = sessionId,
            Nonce = Convert.ToBase64String(nonce),
            PublicKey = Convert.ToBase64String(publicKey)
        });
    }

    private static OfficialUninstallSessionBootstrapHello DecodeHello(
        ReadOnlySpan<byte> payload,
        string expectedType)
    {
        var wire = Deserialize<HelloWire>(payload);
        if (wire.SchemaVersion != SchemaVersion
            || !string.Equals(wire.MessageType, expectedType, StringComparison.Ordinal)
            || !string.Equals(wire.ProtocolVersion, ProtocolVersion, StringComparison.Ordinal))
        {
            throw Protocol("The hello message schema, type, or protocol is invalid.");
        }

        try
        {
            ValidateToken(wire.SessionId, "session id");
            var nonce = Convert.FromBase64String(wire.Nonce);
            var publicKey = Convert.FromBase64String(wire.PublicKey);
            ValidateNonce(nonce);
            ValidatePublicKey(publicKey);
            return new OfficialUninstallSessionBootstrapHello
            {
                SessionId = wire.SessionId,
                Nonce = nonce,
                PublicKey = publicKey
            };
        }
        catch (OfficialUninstallSessionBootstrapException)
        {
            throw;
        }
        catch (FormatException exception)
        {
            throw Protocol("The hello message contains malformed base64.", exception);
        }
    }

    private static byte[] Serialize<T>(T value)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        if (payload.Length == 0 || payload.Length > MaximumPayloadBytes)
            throw Payload("The bootstrap payload size is invalid.");
        return payload;
    }

    private static T Deserialize<T>(ReadOnlySpan<byte> payload)
    {
        if (payload.Length == 0 || payload.Length > MaximumPayloadBytes)
            throw Payload("The bootstrap payload size is invalid.");
        try
        {
            return JsonSerializer.Deserialize<T>(payload, JsonOptions)
                ?? throw Protocol("The bootstrap payload is empty.");
        }
        catch (OfficialUninstallSessionBootstrapException)
        {
            throw;
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            throw Protocol("The bootstrap payload is malformed.", exception);
        }
    }

    internal static void ValidateNonce(byte[] nonce)
    {
        if (nonce is not { Length: 32 })
            throw Protocol("The bootstrap nonce must contain 32 bytes.");
    }

    internal static void ValidatePublicKey(byte[] publicKey)
    {
        if (publicKey is not { Length: >= 64 and <= 512 })
            throw Protocol("The bootstrap public key size is invalid.");
    }

    internal static void ValidateToken(string? value, string name)
    {
        if (!TransportAuthentication.IsValidToken(value))
            throw Protocol($"The {name} is invalid.");
    }

    internal static OfficialUninstallSessionBootstrapException Protocol(
        string message,
        Exception? innerException = null) =>
        new(OfficialUninstallSessionBootstrapStatus.ProtocolRejected, message, innerException);

    private static OfficialUninstallSessionBootstrapException Payload(string message) =>
        new(OfficialUninstallSessionBootstrapStatus.PayloadRejected, message);

    private sealed class HelloWire
    {
        public required int SchemaVersion { get; init; }
        public required string MessageType { get; init; }
        public required string ProtocolVersion { get; init; }
        public required string SessionId { get; init; }
        public required string Nonce { get; init; }
        public required string PublicKey { get; init; }
    }

    private sealed class FinishedWire
    {
        public required int SchemaVersion { get; init; }
        public required string MessageType { get; init; }
        public required string ProtocolVersion { get; init; }
        public required string SessionId { get; init; }
        public required string Role { get; init; }
        public required string AuthenticationTag { get; init; }
    }
}

public sealed class OfficialUninstallSessionBootstrapClient
{
    private readonly OfficialUninstallSessionBootstrapContext _context;
    private readonly Func<byte[]> _nonceFactory;
    private readonly TimeSpan _timeout;

    public OfficialUninstallSessionBootstrapClient(
        OfficialUninstallSessionBootstrapContext context,
        Func<byte[]>? nonceFactory = null,
        TimeSpan? timeout = null)
    {
        _context = SessionBootstrapCryptography.ValidateContext(context);
        _nonceFactory = nonceFactory ?? (() => RandomNumberGenerator.GetBytes(32));
        _timeout = SessionBootstrapCryptography.ValidateTimeout(timeout ?? TimeSpan.FromSeconds(15));
    }

    public async Task<OfficialUninstallSessionKey> EstablishAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        cancellationToken.ThrowIfCancellationRequested();
        using var timeout = SessionBootstrapCryptography.CreateTimeout(cancellationToken, _timeout);
        byte[]? sessionKey = null;
        byte[]? transcriptHash = null;
        var clientNonce = _nonceFactory();
        OfficialUninstallSessionBootstrapCodec.ValidateNonce(clientNonce);
        try
        {
            using var clientEcdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            var clientPublicKey = clientEcdh.ExportSubjectPublicKeyInfo();
            var clientHello = OfficialUninstallSessionBootstrapCodec.EncodeClientHello(
                _context.SessionId,
                clientNonce,
                clientPublicKey);
            await OfficialUninstallPipeFrame.WriteAsync(stream, clientHello, timeout.Token);

            var serverPayload = await OfficialUninstallPipeFrame.ReadAsync(stream, timeout.Token);
            var serverHello = OfficialUninstallSessionBootstrapCodec.DecodeServerHello(serverPayload);
            if (!string.Equals(serverHello.SessionId, _context.SessionId, StringComparison.Ordinal))
                throw OfficialUninstallSessionBootstrapCodec.Protocol("The server hello session is invalid.");

            transcriptHash = SessionBootstrapCryptography.ComputeTranscriptHash(
                _context,
                clientNonce,
                clientPublicKey,
                serverHello.Nonce,
                serverHello.PublicKey);
            sessionKey = SessionBootstrapCryptography.DeriveSessionKey(
                clientEcdh,
                serverHello.PublicKey,
                clientNonce,
                serverHello.Nonce,
                transcriptHash);

            var serverFinished = await SessionBootstrapCryptography.ReadFinishedAsync(
                stream,
                _context.SessionId,
                "server-finished",
                timeout.Token);
            SessionBootstrapCryptography.VerifyFinished(
                sessionKey,
                transcriptHash,
                "server-finished",
                serverFinished);
            var clientFinished = SessionBootstrapCryptography.ComputeFinished(
                sessionKey,
                transcriptHash,
                "client-finished");
            try
            {
                var payload = OfficialUninstallSessionBootstrapCodec.EncodeFinished(
                    _context.SessionId,
                    "client-finished",
                    clientFinished);
                await OfficialUninstallPipeFrame.WriteAsync(stream, payload, timeout.Token);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(clientFinished);
            }

            var result = new OfficialUninstallSessionKey(
                _context.SessionId,
                sessionKey,
                transcriptHash);
            sessionKey = null;
            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new OfficialUninstallSessionBootstrapException(
                OfficialUninstallSessionBootstrapStatus.TimedOut,
                "The client session bootstrap timed out.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (OfficialUninstallSessionBootstrapException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is CryptographicException
                or IOException
                or EndOfStreamException
                or ObjectDisposedException)
        {
            throw OfficialUninstallSessionBootstrapCodec.Protocol(
                "The client session bootstrap failed.",
                exception);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(clientNonce);
            if (sessionKey is not null)
                CryptographicOperations.ZeroMemory(sessionKey);
            if (transcriptHash is not null)
                CryptographicOperations.ZeroMemory(transcriptHash);
        }
    }
}

public sealed class OfficialUninstallSessionBootstrapServer
{
    private readonly OfficialUninstallSessionBootstrapContext _context;
    private readonly OfficialUninstallSessionBootstrapReplayGuard _replayGuard;
    private readonly Func<byte[]> _nonceFactory;
    private readonly Func<DateTimeOffset> _clock;
    private readonly TimeSpan _timeout;

    public OfficialUninstallSessionBootstrapServer(
        OfficialUninstallSessionBootstrapContext context,
        OfficialUninstallSessionBootstrapReplayGuard replayGuard,
        Func<byte[]>? nonceFactory = null,
        Func<DateTimeOffset>? clock = null,
        TimeSpan? timeout = null)
    {
        _context = SessionBootstrapCryptography.ValidateContext(context);
        _replayGuard = replayGuard ?? throw new ArgumentNullException(nameof(replayGuard));
        _nonceFactory = nonceFactory ?? (() => RandomNumberGenerator.GetBytes(32));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
        _timeout = SessionBootstrapCryptography.ValidateTimeout(timeout ?? TimeSpan.FromSeconds(15));
    }

    public async Task<OfficialUninstallSessionKey> EstablishAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        cancellationToken.ThrowIfCancellationRequested();
        using var timeout = SessionBootstrapCryptography.CreateTimeout(cancellationToken, _timeout);
        byte[]? sessionKey = null;
        byte[]? transcriptHash = null;
        var serverNonce = _nonceFactory();
        OfficialUninstallSessionBootstrapCodec.ValidateNonce(serverNonce);
        try
        {
            var clientPayload = await OfficialUninstallPipeFrame.ReadAsync(stream, timeout.Token);
            var clientHello = OfficialUninstallSessionBootstrapCodec.DecodeClientHello(clientPayload);
            if (!string.Equals(clientHello.SessionId, _context.SessionId, StringComparison.Ordinal))
                throw OfficialUninstallSessionBootstrapCodec.Protocol("The client hello session is invalid.");
            var replayStatus = _replayGuard.TryRegister(clientHello.Nonce, _clock());
            if (replayStatus.HasValue)
            {
                throw new OfficialUninstallSessionBootstrapException(
                    replayStatus.Value,
                    "The client bootstrap nonce was replayed or replay capacity was exceeded.");
            }

            using var serverEcdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            var serverPublicKey = serverEcdh.ExportSubjectPublicKeyInfo();
            var serverHelloPayload = OfficialUninstallSessionBootstrapCodec.EncodeServerHello(
                _context.SessionId,
                serverNonce,
                serverPublicKey);
            await OfficialUninstallPipeFrame.WriteAsync(stream, serverHelloPayload, timeout.Token);

            transcriptHash = SessionBootstrapCryptography.ComputeTranscriptHash(
                _context,
                clientHello.Nonce,
                clientHello.PublicKey,
                serverNonce,
                serverPublicKey);
            sessionKey = SessionBootstrapCryptography.DeriveSessionKey(
                serverEcdh,
                clientHello.PublicKey,
                clientHello.Nonce,
                serverNonce,
                transcriptHash);
            var serverFinished = SessionBootstrapCryptography.ComputeFinished(
                sessionKey,
                transcriptHash,
                "server-finished");
            try
            {
                var payload = OfficialUninstallSessionBootstrapCodec.EncodeFinished(
                    _context.SessionId,
                    "server-finished",
                    serverFinished);
                await OfficialUninstallPipeFrame.WriteAsync(stream, payload, timeout.Token);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(serverFinished);
            }

            var clientFinished = await SessionBootstrapCryptography.ReadFinishedAsync(
                stream,
                _context.SessionId,
                "client-finished",
                timeout.Token);
            SessionBootstrapCryptography.VerifyFinished(
                sessionKey,
                transcriptHash,
                "client-finished",
                clientFinished);

            var result = new OfficialUninstallSessionKey(
                _context.SessionId,
                sessionKey,
                transcriptHash);
            sessionKey = null;
            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new OfficialUninstallSessionBootstrapException(
                OfficialUninstallSessionBootstrapStatus.TimedOut,
                "The server session bootstrap timed out.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (OfficialUninstallSessionBootstrapException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is CryptographicException
                or IOException
                or EndOfStreamException
                or ObjectDisposedException)
        {
            throw OfficialUninstallSessionBootstrapCodec.Protocol(
                "The server session bootstrap failed.",
                exception);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(serverNonce);
            if (sessionKey is not null)
                CryptographicOperations.ZeroMemory(sessionKey);
            if (transcriptHash is not null)
                CryptographicOperations.ZeroMemory(transcriptHash);
        }
    }
}

internal static class SessionBootstrapCryptography
{
    private const int SessionKeyBytes = 32;

    internal static OfficialUninstallSessionBootstrapContext ValidateContext(
        OfficialUninstallSessionBootstrapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        OfficialUninstallFakeNamedPipeServer.ValidatePipeName(context.PipeName);
        OfficialUninstallSessionBootstrapCodec.ValidateToken(context.SessionId, "session id");
        OfficialUninstallFakeNamedPipeServer.ValidateIdentity(context.Client);
        OfficialUninstallFakeNamedPipeServer.ValidateIdentity(context.Server);
        return context;
    }

    internal static TimeSpan ValidateTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromMinutes(1))
            throw new ArgumentOutOfRangeException(nameof(timeout));
        return timeout;
    }

    internal static CancellationTokenSource CreateTimeout(
        CancellationToken cancellationToken,
        TimeSpan timeout)
    {
        var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(timeout);
        return linked;
    }

    internal static byte[] ComputeTranscriptHash(
        OfficialUninstallSessionBootstrapContext context,
        byte[] clientNonce,
        byte[] clientPublicKey,
        byte[] serverNonce,
        byte[] serverPublicKey)
    {
        var canonical = new StringBuilder();
        Append(canonical, OfficialUninstallSessionBootstrapCodec.ProtocolVersion);
        Append(canonical, context.SessionId);
        Append(canonical, context.PipeName);
        AppendIdentity(canonical, "client", context.Client);
        AppendIdentity(canonical, "server", context.Server);
        Append(canonical, Convert.ToBase64String(clientNonce));
        Append(canonical, Convert.ToBase64String(clientPublicKey));
        Append(canonical, Convert.ToBase64String(serverNonce));
        Append(canonical, Convert.ToBase64String(serverPublicKey));
        return SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString()));
    }

    internal static byte[] DeriveSessionKey(
        ECDiffieHellman localKey,
        byte[] remotePublicKey,
        byte[] clientNonce,
        byte[] serverNonce,
        byte[] transcriptHash)
    {
        using var remoteKey = ECDiffieHellman.Create();
        remoteKey.ImportSubjectPublicKeyInfo(remotePublicKey, out var bytesRead);
        if (bytesRead != remotePublicKey.Length || remoteKey.KeySize != 256)
            throw OfficialUninstallSessionBootstrapCodec.Protocol("The remote ECDH key is invalid.");

        var inputKeyMaterial = localKey.DeriveKeyMaterial(remoteKey.PublicKey);
        var saltInput = new byte[clientNonce.Length + serverNonce.Length];
        clientNonce.CopyTo(saltInput, 0);
        serverNonce.CopyTo(saltInput, clientNonce.Length);
        var salt = SHA256.HashData(saltInput);
        byte[]? pseudoRandomKey = null;
        try
        {
            using (var extract = new HMACSHA256(salt))
                pseudoRandomKey = extract.ComputeHash(inputKeyMaterial);
            var infoLabel = Encoding.UTF8.GetBytes("OMNIX official uninstall session key v1");
            var expandInput = new byte[infoLabel.Length + transcriptHash.Length + 1];
            infoLabel.CopyTo(expandInput, 0);
            transcriptHash.CopyTo(expandInput, infoLabel.Length);
            expandInput[^1] = 0x01;
            try
            {
                using var expand = new HMACSHA256(pseudoRandomKey);
                return expand.ComputeHash(expandInput)[..SessionKeyBytes];
            }
            finally
            {
                CryptographicOperations.ZeroMemory(expandInput);
                CryptographicOperations.ZeroMemory(infoLabel);
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(inputKeyMaterial);
            CryptographicOperations.ZeroMemory(saltInput);
            CryptographicOperations.ZeroMemory(salt);
            if (pseudoRandomKey is not null)
                CryptographicOperations.ZeroMemory(pseudoRandomKey);
        }
    }

    internal static byte[] ComputeFinished(
        byte[] sessionKey,
        byte[] transcriptHash,
        string role)
    {
        var label = Encoding.UTF8.GetBytes(role);
        var input = new byte[label.Length + transcriptHash.Length];
        label.CopyTo(input, 0);
        transcriptHash.CopyTo(input, label.Length);
        try
        {
            using var hmac = new HMACSHA256(sessionKey);
            return hmac.ComputeHash(input);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(input);
            CryptographicOperations.ZeroMemory(label);
        }
    }

    internal static void VerifyFinished(
        byte[] sessionKey,
        byte[] transcriptHash,
        string role,
        byte[] actual)
    {
        var expected = ComputeFinished(sessionKey, transcriptHash, role);
        try
        {
            if (actual.Length != expected.Length
                || !CryptographicOperations.FixedTimeEquals(actual, expected))
            {
                throw new OfficialUninstallSessionBootstrapException(
                    OfficialUninstallSessionBootstrapStatus.KeyConfirmationFailed,
                    "The peer did not confirm the expected session transcript.");
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(expected);
            CryptographicOperations.ZeroMemory(actual);
        }
    }

    internal static async Task<byte[]> ReadFinishedAsync(
        Stream stream,
        string expectedSessionId,
        string expectedRole,
        CancellationToken cancellationToken)
    {
        try
        {
            var payload = await OfficialUninstallPipeFrame.ReadAsync(
                stream,
                cancellationToken);
            return OfficialUninstallSessionBootstrapCodec.DecodeFinished(
                payload,
                expectedSessionId,
                expectedRole);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (OfficialUninstallSessionBootstrapException exception) when (
            exception.Status is OfficialUninstallSessionBootstrapStatus.ProtocolRejected
                or OfficialUninstallSessionBootstrapStatus.PayloadRejected)
        {
            throw KeyConfirmationFailure(exception);
        }
        catch (Exception exception) when (
            exception is OfficialUninstallPipeProtocolException
                or IOException
                or EndOfStreamException
                or ObjectDisposedException)
        {
            throw KeyConfirmationFailure(exception);
        }
    }

    private static OfficialUninstallSessionBootstrapException KeyConfirmationFailure(
        Exception exception) =>
        new(
            OfficialUninstallSessionBootstrapStatus.KeyConfirmationFailed,
            "The peer did not provide a valid finished confirmation.",
            exception);

    private static void AppendIdentity(
        StringBuilder builder,
        string role,
        OfficialUninstallPipePeerIdentity identity)
    {
        Append(builder, role);
        Append(builder, identity.UserSid.ToUpperInvariant());
        Append(builder, identity.ProcessId);
        Append(builder, identity.WindowsSessionId);
    }

    private static void Append(StringBuilder builder, object? value)
    {
        var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        builder.Append(text.Length).Append(':').Append(text).Append(';');
    }
}
