using System.IO.Pipes;
using Css.Core.Uninstall;
using Css.Ipc.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public sealed class OfficialUninstallOneShotWorkerAuthorizationTests
{
    [Fact]
    public async Task Authorization_denial_happens_after_peer_validation_and_before_bootstrap_or_handler()
    {
        var identity = Identity();
        var pipeName = $"omnix-authorization-{Guid.NewGuid():N}";
        var authorizationCalls = 0;
        var handlerCalls = 0;
        var server = new OfficialUninstallOneShotWorkerServer(
            new FixedPeerIdentityReader(identity));
        var options = Options(pipeName, identity);

        var serverTask = server.ServeOnceAsync(
            options,
            (_, _, _) =>
            {
                authorizationCalls++;
                return ValueTask.FromResult(false);
            },
            (_, _) =>
            {
                handlerCalls++;
                throw new InvalidOperationException("The handler must remain unreachable.");
            });
        await ConnectAndWaitForCloseAsync(pipeName);
        var result = await serverTask;

        result.Status.Should().Be(OfficialUninstallTransportStatus.AuthorizationFailed);
        authorizationCalls.Should().Be(1);
        handlerCalls.Should().Be(0);
    }

    [Fact]
    public async Task Wrong_pipe_peer_is_rejected_before_package_authorization()
    {
        var expected = Identity();
        var actual = expected with { ProcessId = expected.ProcessId + 1 };
        var pipeName = $"omnix-peer-before-authorization-{Guid.NewGuid():N}";
        var authorizationCalls = 0;
        var server = new OfficialUninstallOneShotWorkerServer(
            new FixedPeerIdentityReader(actual));

        var serverTask = server.ServeOnceAsync(
            Options(pipeName, expected),
            (_, _, _) =>
            {
                authorizationCalls++;
                return ValueTask.FromResult(true);
            },
            (_, _) => throw new InvalidOperationException());
        await ConnectAndWaitForCloseAsync(pipeName);
        var action = async () => await serverTask;

        await action.Should().ThrowAsync<InvalidOperationException>();
        authorizationCalls.Should().Be(0);
    }

    [Fact]
    public void Server_source_preserves_peer_then_authorization_then_bootstrap_order()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Ipc", "Uninstall", "OfficialUninstallOneShotWorkerServer.cs"));

        source.IndexOf("ReadClientPeer", StringComparison.Ordinal)
            .Should().BeLessThan(source.IndexOf("authorization(actualClient", StringComparison.Ordinal));
        source.IndexOf("authorization(actualClient", StringComparison.Ordinal)
            .Should().BeLessThan(source.IndexOf("OfficialUninstallSessionBootstrapServer", StringComparison.Ordinal));
        source.IndexOf("OfficialUninstallSessionBootstrapServer", StringComparison.Ordinal)
            .Should().BeLessThan(source.IndexOf(
                "new OfficialUninstallAuthenticatedInMemoryEndpoint",
                StringComparison.Ordinal));
    }

    private static async Task ConnectAndWaitForCloseAsync(string pipeName)
    {
        await using var client = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);
        await client.ConnectAsync(2_000);
        var buffer = new byte[1];
        _ = await client.ReadAsync(buffer);
    }

    private static OfficialUninstallOneShotWorkerOptions Options(
        string pipeName,
        OfficialUninstallPipePeerIdentity identity) =>
        new()
        {
            PipeName = pipeName,
            SessionId = $"authorization-session-{Guid.NewGuid():N}",
            ExpectedClient = identity,
            Worker = identity,
            Timeout = TimeSpan.FromSeconds(2)
        };

    private static OfficialUninstallPipePeerIdentity Identity() =>
        new()
        {
            UserSid = "S-1-5-21-1-2-3-1001",
            ProcessId = Environment.ProcessId,
            WindowsSessionId = 0
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

    private sealed class FixedPeerIdentityReader(OfficialUninstallPipePeerIdentity client)
        : IOfficialUninstallPipePeerIdentityReader
    {
        public OfficialUninstallPipePeerIdentity ReadClientPeer(NamedPipeServerStream pipe) => client;
        public OfficialUninstallPipePeerIdentity ReadServerPeer(NamedPipeClientStream pipe) => client;
    }
}
