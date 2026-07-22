using System.IO;
using System.IO.Pipes;
using System.Text;
using Css.Ipc.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public sealed class OfficialUninstallSessionBootstrapTests
{
    private static readonly byte[] ClientNonce = Enumerable.Range(1, 32)
        .Select(value => (byte)value)
        .ToArray();
    private static readonly byte[] ServerNonce = Enumerable.Range(65, 32)
        .Select(value => (byte)value)
        .ToArray();

    [Fact]
    public async Task Live_named_pipe_bootstrap_derives_the_same_fresh_256_bit_key()
    {
        var context = Context();
        var guard = new OfficialUninstallSessionBootstrapReplayGuard();
        await using var connection = await ConnectedPipe.CreateAsync();
        var server = new OfficialUninstallSessionBootstrapServer(
            context,
            guard,
            nonceFactory: () => ServerNonce.ToArray());
        var client = new OfficialUninstallSessionBootstrapClient(
            context,
            nonceFactory: () => ClientNonce.ToArray());

        var serverTask = server.EstablishAsync(connection.Server);
        var clientTask = client.EstablishAsync(connection.Client);
        await Task.WhenAll(serverTask, clientTask);
        using var serverKey = await serverTask;
        using var clientKey = await clientTask;
        var serverBytes = serverKey.ExportCopy();
        var clientBytes = clientKey.ExportCopy();

        serverBytes.Should().HaveCount(32);
        clientBytes.Should().Equal(serverBytes);
        clientBytes.Any(value => value != 0).Should().BeTrue();
        clientKey.TranscriptSha256.Should().Be(serverKey.TranscriptSha256);
        clientKey.SessionId.Should().Be(context.SessionId);
    }

    [Fact]
    public async Task Changed_bound_identity_breaks_server_finished_confirmation()
    {
        var serverContext = Context();
        var clientContext = serverContext with
        {
            Client = serverContext.Client with
            {
                ProcessId = serverContext.Client.ProcessId + 1
            }
        };
        await using var connection = await ConnectedPipe.CreateAsync();
        var server = new OfficialUninstallSessionBootstrapServer(
            serverContext,
            new OfficialUninstallSessionBootstrapReplayGuard(),
            nonceFactory: () => ServerNonce.ToArray());
        var client = new OfficialUninstallSessionBootstrapClient(
            clientContext,
            nonceFactory: () => ClientNonce.ToArray());

        var serverTask = server.EstablishAsync(connection.Server);
        var clientAction = () => client.EstablishAsync(connection.Client);

        var exception = await clientAction.Should()
            .ThrowAsync<OfficialUninstallSessionBootstrapException>();
        exception.Which.Status.Should().Be(
            OfficialUninstallSessionBootstrapStatus.KeyConfirmationFailed);
        await connection.Client.DisposeAsync();
        await serverTask.Invoking(task => task).Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Tampered_server_finished_message_is_rejected()
    {
        var context = Context();
        await using var connection = await ConnectedPipe.CreateAsync();
        await using var tamperingServerStream = new FinishedTamperingStream(
            connection.Server,
            "server-finished");
        var server = new OfficialUninstallSessionBootstrapServer(
            context,
            new OfficialUninstallSessionBootstrapReplayGuard(),
            nonceFactory: () => ServerNonce.ToArray());
        var client = new OfficialUninstallSessionBootstrapClient(
            context,
            nonceFactory: () => ClientNonce.ToArray());

        var serverTask = server.EstablishAsync(tamperingServerStream);
        var clientAction = () => client.EstablishAsync(connection.Client);

        var exception = await clientAction.Should()
            .ThrowAsync<OfficialUninstallSessionBootstrapException>();
        exception.Which.Status.Should().Be(
            OfficialUninstallSessionBootstrapStatus.KeyConfirmationFailed);
        await connection.Client.DisposeAsync();
        await serverTask.Invoking(task => task).Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Replayed_client_nonce_is_rejected_before_a_second_session_key_is_created()
    {
        var context = Context();
        var guard = new OfficialUninstallSessionBootstrapReplayGuard();
        await RunSuccessfulHandshakeAsync(context, guard);

        await using var connection = await ConnectedPipe.CreateAsync();
        var server = new OfficialUninstallSessionBootstrapServer(
            context,
            guard,
            nonceFactory: () => ServerNonce.ToArray());
        var client = new OfficialUninstallSessionBootstrapClient(
            context,
            nonceFactory: () => ClientNonce.ToArray());

        var serverAction = () => server.EstablishAsync(connection.Server);
        var clientTask = client.EstablishAsync(connection.Client);

        var exception = await serverAction.Should()
            .ThrowAsync<OfficialUninstallSessionBootstrapException>();
        exception.Which.Status.Should().Be(
            OfficialUninstallSessionBootstrapStatus.ReplayRejected);
        await connection.Server.DisposeAsync();
        await clientTask.Invoking(task => task).Should().ThrowAsync<Exception>();
    }

    [Fact]
    public void Codec_rejects_oversized_malformed_and_wrong_schema_hello_messages()
    {
        var oversized = new byte[OfficialUninstallSessionBootstrapCodec.MaximumPayloadBytes + 1];
        var malformed = Encoding.UTF8.GetBytes("{broken-json");
        var wrongSchema = Encoding.UTF8.GetBytes(
            "{\"schemaVersion\":99,\"messageType\":\"client-hello\"}");

        var oversizedAction = () =>
            OfficialUninstallSessionBootstrapCodec.DecodeClientHello(oversized);
        var malformedAction = () =>
            OfficialUninstallSessionBootstrapCodec.DecodeClientHello(malformed);
        var wrongSchemaAction = () =>
            OfficialUninstallSessionBootstrapCodec.DecodeClientHello(wrongSchema);

        oversizedAction.Should().Throw<OfficialUninstallSessionBootstrapException>()
            .Which.Status.Should().Be(OfficialUninstallSessionBootstrapStatus.PayloadRejected);
        malformedAction.Should().Throw<OfficialUninstallSessionBootstrapException>()
            .Which.Status.Should().Be(OfficialUninstallSessionBootstrapStatus.ProtocolRejected);
        wrongSchemaAction.Should().Throw<OfficialUninstallSessionBootstrapException>()
            .Which.Status.Should().Be(OfficialUninstallSessionBootstrapStatus.ProtocolRejected);
    }

    [Fact]
    public async Task Internal_timeout_is_typed_and_external_cancellation_propagates()
    {
        var context = Context();
        await using var waiting = new NeverCompletingReadStream();
        var timedClient = new OfficialUninstallSessionBootstrapClient(
            context,
            nonceFactory: () => ClientNonce.ToArray(),
            timeout: TimeSpan.FromMilliseconds(100));

        var timedAction = () => timedClient.EstablishAsync(waiting);

        var timeout = await timedAction.Should()
            .ThrowAsync<OfficialUninstallSessionBootstrapException>();
        timeout.Which.Status.Should().Be(OfficialUninstallSessionBootstrapStatus.TimedOut);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelledClient = new OfficialUninstallSessionBootstrapClient(context);
        var cancelledAction = () => cancelledClient.EstablishAsync(
            waiting,
            cancellation.Token);
        await cancelledAction.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Session_key_is_zeroized_on_dispose_and_bootstrap_has_no_secret_side_channel()
    {
        var context = Context();
        var guard = new OfficialUninstallSessionBootstrapReplayGuard();
        await using var connection = await ConnectedPipe.CreateAsync();
        var server = new OfficialUninstallSessionBootstrapServer(
            context,
            guard,
            nonceFactory: () => ServerNonce.ToArray());
        var client = new OfficialUninstallSessionBootstrapClient(
            context,
            nonceFactory: () => ClientNonce.ToArray());
        var serverTask = server.EstablishAsync(connection.Server);
        var key = await client.EstablishAsync(connection.Client);
        using var serverKey = await serverTask;

        key.ExportCopy().Should().HaveCount(32);
        key.Dispose();

        key.IsDisposed.Should().BeTrue();
        var exportAction = () => key.ExportCopy();
        exportAction.Should().Throw<ObjectDisposedException>();

        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Ipc", "Uninstall", "OfficialUninstallSessionBootstrap.cs"));
        var program = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "Program.cs"));
        source.Should().Contain("ECDiffieHellman");
        source.Should().Contain("nistP256");
        source.Should().Contain("HMACSHA256");
        source.Should().Contain("FixedTimeEquals");
        source.Should().Contain("CryptographicOperations.ZeroMemory");
        source.Should().NotContain("Environment.GetEnvironmentVariable");
        source.Should().NotContain("Environment.CommandLine");
        source.Should().NotContain("File.Read");
        source.Should().NotContain("File.Write");
        source.Should().NotContain("Process.Start");
        source.Should().NotContain("SafetyOperationPipeline");
        source.Should().NotContain("OfficialUninstallOperationHandler");
        program.Should().NotContain("OfficialUninstallSessionBootstrap");
    }

    private static async Task RunSuccessfulHandshakeAsync(
        OfficialUninstallSessionBootstrapContext context,
        OfficialUninstallSessionBootstrapReplayGuard guard)
    {
        await using var connection = await ConnectedPipe.CreateAsync();
        var server = new OfficialUninstallSessionBootstrapServer(
            context,
            guard,
            nonceFactory: () => ServerNonce.ToArray());
        var client = new OfficialUninstallSessionBootstrapClient(
            context,
            nonceFactory: () => ClientNonce.ToArray());
        var serverTask = server.EstablishAsync(connection.Server);
        var clientTask = client.EstablishAsync(connection.Client);
        await Task.WhenAll(serverTask, clientTask);
        using var serverKey = await serverTask;
        using var clientKey = await clientTask;
    }

    private static OfficialUninstallSessionBootstrapContext Context() =>
        new()
        {
            PipeName = "omnix-bootstrap-test",
            SessionId = "bootstrap-session-1",
            Client = new OfficialUninstallPipePeerIdentity
            {
                UserSid = "S-1-5-21-1000",
                ProcessId = 4100,
                WindowsSessionId = 3
            },
            Server = new OfficialUninstallPipePeerIdentity
            {
                UserSid = "S-1-5-21-1000",
                ProcessId = 5100,
                WindowsSessionId = 3
            }
        };

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(path))
                return path;
            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(segments));
    }

    private sealed class ConnectedPipe : IAsyncDisposable
    {
        private ConnectedPipe(
            NamedPipeServerStream server,
            NamedPipeClientStream client)
        {
            Server = server;
            Client = client;
        }

        public NamedPipeServerStream Server { get; }
        public NamedPipeClientStream Client { get; }

        public static async Task<ConnectedPipe> CreateAsync()
        {
            var pipeName = $"omnix-bootstrap-{Guid.NewGuid():N}";
            var server = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            var client = new NamedPipeClientStream(
                ".",
                pipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);
            var wait = server.WaitForConnectionAsync();
            await client.ConnectAsync(2000);
            await wait;
            return new ConnectedPipe(server, client);
        }

        public async ValueTask DisposeAsync()
        {
            await Client.DisposeAsync();
            await Server.DisposeAsync();
        }
    }

    private sealed class FinishedTamperingStream(Stream inner, string marker) : Stream
    {
        private bool _tampered;

        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => inner.CanWrite;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => inner.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) =>
            inner.FlushAsync(cancellationToken);
        public override int Read(byte[] buffer, int offset, int count) =>
            inner.Read(buffer, offset, count);
        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            inner.ReadAsync(buffer, cancellationToken);
        public override void Write(byte[] buffer, int offset, int count) =>
            inner.Write(buffer, offset, count);

        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            if (!_tampered)
            {
                var text = Encoding.UTF8.GetString(buffer.Span);
                if (text.Contains(marker, StringComparison.Ordinal))
                {
                    var copy = buffer.ToArray();
                    var tagMarker = Encoding.UTF8.GetBytes("\"authenticationTag\":\"");
                    var index = copy.AsSpan().IndexOf(tagMarker);
                    if (index >= 0)
                    {
                        var tagIndex = index + tagMarker.Length;
                        copy[tagIndex] = copy[tagIndex] == (byte)'A' ? (byte)'B' : (byte)'A';
                        _tampered = true;
                        return inner.WriteAsync(copy, cancellationToken);
                    }
                }
            }

            return inner.WriteAsync(buffer, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override async ValueTask DisposeAsync()
        {
            await inner.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }

    private sealed class NeverCompletingReadStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            new(Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken)
                .ContinueWith(_ => 0, cancellationToken));
        public override void Write(byte[] buffer, int offset, int count) { }
        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
