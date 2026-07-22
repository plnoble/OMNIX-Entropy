using System.Diagnostics;
using System.Security.Principal;
using Css.Elevated.Uninstall;
using Css.Ipc.Uninstall;
using Css.Win32.Processes;
using Css.Win32.Security;
using FluentAssertions;

namespace Css.Tests;

public sealed class OfficialUninstallProductionPackageAuthorizerTests
{
    [Fact]
    public void Same_windows_trusted_signer_authorizes_the_actual_client_and_worker()
    {
        var identities = Identities();
        var paths = Paths(identities.Client.ProcessId, identities.Worker.ProcessId);
        var verifier = new FakeVerifier(new Dictionary<string, AuthenticodeSignatureEvidence>
        {
            [paths.Client] = Trusted("AA11", 'A'),
            [paths.Worker] = Trusted("AA11", 'B')
        });
        var authorizer = new WindowsOfficialUninstallProductionPackageAuthorizer(
            paths.Resolver,
            verifier);

        var result = authorizer.Authorize(identities.Client, identities.Worker);

        result.Status.Should().Be(OfficialUninstallProductionPackageTrustStatus.Trusted);
        result.CanAuthorize.Should().BeTrue();
        verifier.Paths.Should().Equal(paths.Client, paths.Worker);
    }

    [Fact]
    public void Different_certificate_thumbprints_are_rejected_even_when_both_are_trusted()
    {
        var identities = Identities();
        var paths = Paths(identities.Client.ProcessId, identities.Worker.ProcessId);
        var authorizer = new WindowsOfficialUninstallProductionPackageAuthorizer(
            paths.Resolver,
            new FakeVerifier(new Dictionary<string, AuthenticodeSignatureEvidence>
            {
                [paths.Client] = Trusted("AA11", 'A'),
                [paths.Worker] = Trusted("BB22", 'B')
            }));

        var result = authorizer.Authorize(identities.Client, identities.Worker);

        result.Status.Should().Be(OfficialUninstallProductionPackageTrustStatus.SignerMismatch);
        result.CanAuthorize.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, OfficialUninstallProductionPackageTrustStatus.ClientNotTrusted)]
    [InlineData(false, OfficialUninstallProductionPackageTrustStatus.WorkerNotTrusted)]
    public void Any_unsigned_package_side_is_rejected(
        bool unsignedClient,
        OfficialUninstallProductionPackageTrustStatus expected)
    {
        var identities = Identities();
        var paths = Paths(identities.Client.ProcessId, identities.Worker.ProcessId);
        var unsigned = new AuthenticodeSignatureEvidence
        {
            Status = AuthenticodeSignatureStatus.NotSigned,
            FileSha256 = new string('C', 64)
        };
        var authorizer = new WindowsOfficialUninstallProductionPackageAuthorizer(
            paths.Resolver,
            new FakeVerifier(new Dictionary<string, AuthenticodeSignatureEvidence>
            {
                [paths.Client] = unsignedClient ? unsigned : Trusted("AA11", 'A'),
                [paths.Worker] = unsignedClient ? Trusted("AA11", 'B') : unsigned
            }));

        var result = authorizer.Authorize(identities.Client, identities.Worker);

        result.Status.Should().Be(expected);
        result.CanAuthorize.Should().BeFalse();
    }

    [Fact]
    public void Worker_identity_must_be_the_current_elevated_process()
    {
        var identities = Identities();
        var authorizer = new WindowsOfficialUninstallProductionPackageAuthorizer(
            new FakePathResolver(new Dictionary<int, string>()),
            new FakeVerifier(new Dictionary<string, AuthenticodeSignatureEvidence>()));

        var result = authorizer.Authorize(
            identities.Client,
            identities.Worker with { ProcessId = Environment.ProcessId + 1 });

        result.Status.Should().Be(
            OfficialUninstallProductionPackageTrustStatus.WorkerIdentityMismatch);
        result.CanAuthorize.Should().BeFalse();
    }

    [Fact]
    public void Current_unsigned_release_app_and_worker_are_production_denied()
    {
        var identities = Identities();
        var app = AppOutputFile("Css.App.exe");
        var worker = AppOutputFile("Css.Elevated.exe");
        var authorizer = new WindowsOfficialUninstallProductionPackageAuthorizer(
            new FakePathResolver(new Dictionary<int, string>
            {
                [identities.Client.ProcessId] = app,
                [identities.Worker.ProcessId] = worker
            }),
            new WindowsAuthenticodeSignatureVerifier());

        var result = authorizer.Authorize(identities.Client, identities.Worker);

        result.CanAuthorize.Should().BeFalse();
        result.Status.Should().BeOneOf(
            OfficialUninstallProductionPackageTrustStatus.ClientNotTrusted,
            OfficialUninstallProductionPackageTrustStatus.WorkerNotTrusted);
    }

    [Fact]
    public void Registered_production_mode_uses_thumbprints_and_is_absent_from_app()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "Uninstall",
            "OfficialUninstallProductionPackageAuthorizer.cs"));
        var program = File.ReadAllText(FindRepositoryFile("src", "Css.Elevated", "Program.cs"));
        var worker = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "OfficialUninstallProductionWorker.cs"));
        var app = File.ReadAllText(FindRepositoryFile("src", "Css.App", "App.xaml.cs"));

        source.Should().Contain("SignerThumbprint");
        source.Should().Contain("IsTrusted");
        source.Should().NotContain("SignerSubject,");
        program.Should().Contain("official-uninstall-production-worker");
        worker.Should().Contain("WindowsOfficialUninstallProductionPackageAuthorizer");
        app.Should().NotContain("official-uninstall-production-worker");
    }

    private static (OfficialUninstallPipePeerIdentity Client,
        OfficialUninstallPipePeerIdentity Worker) Identities()
    {
        using var identity = WindowsIdentity.GetCurrent();
        using var process = Process.GetCurrentProcess();
        var sid = identity.User?.Value
            ?? throw new InvalidOperationException("Current SID is unavailable.");
        return (
            new OfficialUninstallPipePeerIdentity
            {
                UserSid = sid,
                ProcessId = Environment.ProcessId + 1000,
                WindowsSessionId = process.SessionId
            },
            new OfficialUninstallPipePeerIdentity
            {
                UserSid = sid,
                ProcessId = Environment.ProcessId,
                WindowsSessionId = process.SessionId
            });
    }

    private static (string Client, string Worker, FakePathResolver Resolver) Paths(
        int clientPid,
        int workerPid)
    {
        var client = @"D:\OMNIX\Css.App.exe";
        var worker = @"D:\OMNIX\Css.Elevated.exe";
        return (client, worker, new FakePathResolver(new Dictionary<int, string>
        {
            [clientPid] = client,
            [workerPid] = worker
        }));
    }

    private static AuthenticodeSignatureEvidence Trusted(string thumbprint, char hash) =>
        new()
        {
            Status = AuthenticodeSignatureStatus.Trusted,
            SignerSubject = "CN=OMNIX",
            SignerThumbprint = thumbprint,
            FileSha256 = new string(hash, 64)
        };

    private static string AppOutputFile(string fileName)
    {
        var path = Path.Combine(
            FindRepositoryRoot(),
            "src", "Css.App", "bin", "Release", "net8.0-windows", fileName);
        File.Exists(path).Should().BeTrue($"Release package file {fileName} should exist");
        return path;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static string FindRepositoryFile(params string[] segments) =>
        Path.Combine([FindRepositoryRoot(), .. segments]);

    private sealed class FakePathResolver(IReadOnlyDictionary<int, string> paths)
        : IWindowsProcessImagePathResolver
    {
        public string Resolve(int processId) => paths[processId];
    }

    private sealed class FakeVerifier(
        IReadOnlyDictionary<string, AuthenticodeSignatureEvidence> evidence)
        : IAuthenticodeSignatureVerifier
    {
        public List<string> Paths { get; } = [];

        public AuthenticodeSignatureEvidence Verify(string filePath)
        {
            Paths.Add(filePath);
            return evidence[filePath];
        }
    }
}
