using System.Security.Cryptography;
using Css.App;
using Css.Ipc.Uninstall;
using Css.Win32.Security;
using FluentAssertions;

namespace Css.Tests;

public sealed class OfficialUninstallWorkerTrustPolicyTests
{
    [Fact]
    public void Same_trusted_signer_thumbprint_allows_production()
    {
        var verifier = new FakeVerifier(new Dictionary<string, AuthenticodeSignatureEvidence>
        {
            ["app.exe"] = Trusted("CN=OMNIX", new string('A', 40), '1'),
            ["worker.exe"] = Trusted("CN=OMNIX", new string('A', 40), '2')
        });

        var assessment = OfficialUninstallWorkerTrustPolicy.Evaluate(
            "app.exe",
            Available("worker.exe"),
            verifier);

        assessment.Status.Should().Be(
            OfficialUninstallWorkerTrustStatus.TrustedForProduction);
        assessment.CanLaunchProduction.Should().BeTrue();
        assessment.CanLaunchDevelopmentVerification.Should().BeTrue();
        assessment.WorkerEvidence.FileSha256.Should().Be(new string('2', 64));
    }

    [Fact]
    public void Same_subject_with_a_different_certificate_is_rejected()
    {
        var assessment = OfficialUninstallWorkerTrustPolicy.Evaluate(
            "app.exe",
            Available("worker.exe"),
            new FakeVerifier(new Dictionary<string, AuthenticodeSignatureEvidence>
            {
                ["app.exe"] = Trusted("CN=Same Name", new string('A', 40), '1'),
                ["worker.exe"] = Trusted("CN=Same Name", new string('B', 40), '2')
            }));

        assessment.Status.Should().Be(OfficialUninstallWorkerTrustStatus.SignerMismatch);
        assessment.CanLaunchProduction.Should().BeFalse();
        assessment.CanLaunchDevelopmentVerification.Should().BeFalse();
    }

    [Theory]
    [InlineData(AuthenticodeSignatureStatus.Invalid)]
    [InlineData(AuthenticodeSignatureStatus.Untrusted)]
    [InlineData(AuthenticodeSignatureStatus.ProbeFailed)]
    public void Invalid_untrusted_or_unreadable_worker_fails_closed(
        AuthenticodeSignatureStatus workerStatus)
    {
        var assessment = OfficialUninstallWorkerTrustPolicy.Evaluate(
            "app.exe",
            Available("worker.exe"),
            new FakeVerifier(new Dictionary<string, AuthenticodeSignatureEvidence>
            {
                ["app.exe"] = Trusted("CN=OMNIX", new string('A', 40), '1'),
                ["worker.exe"] = Evidence(workerStatus, '2')
            }));

        assessment.CanLaunchProduction.Should().BeFalse();
        assessment.CanLaunchDevelopmentVerification.Should().BeFalse();
        assessment.Status.Should().Be(workerStatus == AuthenticodeSignatureStatus.ProbeFailed
            ? OfficialUninstallWorkerTrustStatus.ProbeFailed
            : OfficialUninstallWorkerTrustStatus.WorkerUntrusted);
    }

    [Fact]
    public void Both_unsigned_files_allow_only_explicit_development_verification()
    {
        var assessment = OfficialUninstallWorkerTrustPolicy.Evaluate(
            "app.exe",
            Available("worker.exe"),
            new FakeVerifier(new Dictionary<string, AuthenticodeSignatureEvidence>
            {
                ["app.exe"] = Evidence(AuthenticodeSignatureStatus.NotSigned, '1'),
                ["worker.exe"] = Evidence(AuthenticodeSignatureStatus.NotSigned, '2')
            }));

        assessment.Status.Should().Be(OfficialUninstallWorkerTrustStatus.AppNotSigned);
        assessment.CanLaunchProduction.Should().BeFalse();
        assessment.CanLaunchDevelopmentVerification.Should().BeTrue();
        var view = OfficialUninstallWorkerTrustPresenter.Create(assessment);
        view.Title.Should().Be("当前是开发验证版本");
        view.StatusLabel.Should().Be("仅允许测试");
        view.SafetyText.Should().Contain("不会获得真实卸载");
    }

    [Fact]
    public void One_unsigned_file_never_gets_development_or_production_authority()
    {
        var assessment = OfficialUninstallWorkerTrustPolicy.Evaluate(
            "app.exe",
            Available("worker.exe"),
            new FakeVerifier(new Dictionary<string, AuthenticodeSignatureEvidence>
            {
                ["app.exe"] = Trusted("CN=OMNIX", new string('A', 40), '1'),
                ["worker.exe"] = Evidence(AuthenticodeSignatureStatus.NotSigned, '2')
            }));

        assessment.Status.Should().Be(OfficialUninstallWorkerTrustStatus.WorkerNotSigned);
        assessment.CanLaunchProduction.Should().BeFalse();
        assessment.CanLaunchDevelopmentVerification.Should().BeFalse();
    }

    [Fact]
    public void Missing_or_throwing_probe_is_blocked_without_exposing_paths()
    {
        var missing = OfficialUninstallWorkerTrustPolicy.Evaluate(
            "app.exe",
            new OfficialUninstallWorkerAvailability
            {
                Status = OfficialUninstallWorkerAvailabilityStatus.Missing
            },
            new FakeVerifier(new Dictionary<string, AuthenticodeSignatureEvidence>()));
        var failed = OfficialUninstallWorkerTrustPolicy.Evaluate(
            "app.exe",
            Available(@"C:\Private\worker.exe"),
            new ThrowingVerifier());

        missing.Status.Should().Be(OfficialUninstallWorkerTrustStatus.WorkerUnavailable);
        failed.Status.Should().Be(OfficialUninstallWorkerTrustStatus.ProbeFailed);
        foreach (var view in new[]
                 {
                     OfficialUninstallWorkerTrustPresenter.Create(missing),
                     OfficialUninstallWorkerTrustPresenter.Create(failed)
                 })
        {
            view.CanExecuteDirectly.Should().BeFalse();
            view.VisibleText.Should().NotContain(@"C:\");
            view.VisibleText.Should().NotContain("Private");
            view.VisibleText.Should().NotContain("thumbprint");
            view.VisibleText.Should().NotContain("WinVerifyTrust");
        }
    }

    [Fact]
    public void Current_unsigned_debug_pair_is_production_blocked_but_hash_bound_for_verification()
    {
        var appPath = AppOutputFile("Css.App.exe");
        var workerPath = AppOutputFile("Css.Elevated.exe");
        var availability = OfficialUninstallWorkerPathResolver.Resolve(
            Path.GetDirectoryName(appPath)!);

        var assessment = OfficialUninstallWorkerTrustPolicy.Evaluate(
            appPath,
            availability,
            new WindowsAuthenticodeSignatureVerifier());

        assessment.AppEvidence.Status.Should().Be(AuthenticodeSignatureStatus.NotSigned);
        assessment.WorkerEvidence.Status.Should().Be(AuthenticodeSignatureStatus.NotSigned);
        assessment.AppEvidence.FileSha256.Should().MatchRegex("^[0-9A-F]{64}$");
        assessment.WorkerEvidence.FileSha256.Should().MatchRegex("^[0-9A-F]{64}$");
        assessment.CanLaunchProduction.Should().BeFalse();
        assessment.CanLaunchDevelopmentVerification.Should().BeTrue();
    }

    [Fact]
    public void Windows_verifier_accepts_a_valid_system_signature_and_rejects_tampering()
    {
        var signedFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            "Taskmgr.exe");
        File.Exists(signedFile).Should().BeTrue();
        var verifier = new WindowsAuthenticodeSignatureVerifier();

        var trusted = verifier.Verify(signedFile);

        trusted.Status.Should().Be(AuthenticodeSignatureStatus.Trusted);
        trusted.IsTrusted.Should().BeTrue();
        trusted.SignerSubject.Should().Contain("Microsoft");
        trusted.SignerThumbprint.Should().NotBeNullOrWhiteSpace();
        trusted.FileSha256.Should().MatchRegex("^[0-9A-F]{64}$");

        var tamperedPath = Path.Combine(
            Path.GetTempPath(),
            $"omnix-authenticode-tamper-{Guid.NewGuid():N}.exe");
        try
        {
            File.Copy(signedFile, tamperedPath);
            using (var stream = File.Open(tamperedPath, FileMode.Append, FileAccess.Write, FileShare.None))
                stream.WriteByte(0x5A);

            var tampered = verifier.Verify(tamperedPath);
            tampered.IsTrusted.Should().BeFalse();
            tampered.Status.Should().BeOneOf(
                AuthenticodeSignatureStatus.Invalid,
                AuthenticodeSignatureStatus.Untrusted,
                AuthenticodeSignatureStatus.NotSigned);
        }
        finally
        {
            if (File.Exists(tamperedPath))
                File.Delete(tamperedPath);
        }
    }

    [Fact]
    public async Task Launch_time_hash_mismatch_stops_before_process_or_uac()
    {
        var workerPath = AppOutputFile("Css.Elevated.exe");
        var launcher = new WindowsOfficialUninstallWorkerLauncher(
            workerPath,
            new string('0', 64));

        var result = await launcher.LaunchAsync(new OfficialUninstallWorkerLaunchRequest
        {
            PipeName = "hash-mismatch-test",
            SessionId = "hash-mismatch-session",
            Client = new OfficialUninstallPipePeerIdentity
            {
                UserSid = "S-1-5-21-1-2-3-1001",
                ProcessId = Environment.ProcessId,
                WindowsSessionId = 0
            },
            TimeoutMilliseconds = 1_000
        });

        result.Status.Should().Be(OfficialUninstallWorkerLaunchStatus.Failed);
        result.Process.Should().BeNull();
    }

    [Fact]
    public void Production_launcher_requires_trusted_package_evidence_and_exact_worker_hash()
    {
        var unsigned = OfficialUninstallWorkerTrustPolicy.Evaluate(
            "app.exe",
            Available("worker.exe"),
            new FakeVerifier(new Dictionary<string, AuthenticodeSignatureEvidence>
            {
                ["app.exe"] = Evidence(AuthenticodeSignatureStatus.NotSigned, '1'),
                ["worker.exe"] = Evidence(AuthenticodeSignatureStatus.NotSigned, '2')
            }));
        var trusted = OfficialUninstallWorkerTrustPolicy.Evaluate(
            "app.exe",
            Available("worker.exe"),
            new FakeVerifier(new Dictionary<string, AuthenticodeSignatureEvidence>
            {
                ["app.exe"] = Trusted("CN=OMNIX", new string('A', 40), '1'),
                ["worker.exe"] = Trusted("CN=OMNIX", new string('A', 40), '2')
            }));

        var rejected = () => WindowsOfficialUninstallProductionWorkerLauncher.Create(unsigned);
        var launcher = WindowsOfficialUninstallProductionWorkerLauncher.Create(trusted);

        rejected.Should().Throw<InvalidOperationException>();
        launcher.Should().BeAssignableTo<IOfficialUninstallProductionWorkerLauncher>();
    }

    [Fact]
    public void Production_worker_arguments_are_distinct_and_have_no_fake_switches()
    {
        var request = new OfficialUninstallWorkerLaunchRequest
        {
            PipeName = "production-args-pipe",
            SessionId = "production-args-session",
            Client = new OfficialUninstallPipePeerIdentity
            {
                UserSid = "S-1-5-21-1-2-3-1001",
                ProcessId = 1234,
                WindowsSessionId = 2
            },
            TimeoutMilliseconds = 5_000
        };

        var launcherType = typeof(WindowsOfficialUninstallWorkerLauncher);
        var modeType = launcherType.Assembly.GetType(
            "Css.App.WindowsOfficialUninstallWorkerMode",
            throwOnError: true)!;
        var method = launcherType.GetMethod(
            "WorkerArguments",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException("Worker argument builder is unavailable.");
        var productionMode = Enum.Parse(modeType, "Production");
        var arguments = (IReadOnlyList<string>)(method.Invoke(
            null,
            [request, productionMode])
            ?? throw new InvalidOperationException("Worker arguments were not returned."));

        arguments[0].Should().Be("official-uninstall-production-worker");
        arguments.Should().NotContain(value => value.StartsWith("--fake-", StringComparison.Ordinal));
        arguments.Should().NotContain("--session-key");
        arguments.Should().NotContain("--authentication-tag");
    }

    [Fact]
    public void Native_verifier_and_policy_sources_preserve_the_fail_closed_boundary()
    {
        var verifier = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Win32", "Security", "AuthenticodeSignatureVerifier.cs"));
        var policy = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "OfficialUninstallWorkerTrustPolicy.cs"));
        var launcher = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "OfficialUninstallWorkerLauncher.cs"));
        var app = File.ReadAllText(FindRepositoryFile("src", "Css.App", "App.xaml.cs"));

        verifier.Should().Contain("WinVerifyTrust");
        verifier.Should().Contain("WtdRevokeWholeChain");
        verifier.Should().Contain("WtdCacheOnlyUrlRetrieval");
        verifier.Should().Contain("SignerThumbprint");
        verifier.Should().Contain("SHA256.HashData");
        verifier.Should().NotContain("SignatureInspector");
        verifier.Should().NotContain("Process.Start");
        policy.Should().Contain("SignerThumbprint");
        policy.Should().NotContain("Process.Start");
        policy.Should().NotContain("OfficialUninstallOperationHandler");
        launcher.Should().Contain("FixedTimeEquals");
        launcher.Should().Contain("WorkerHashMatches");
        launcher.Should().Contain("WindowsOfficialUninstallProductionWorkerLauncher");
        launcher.Should().Contain("trust.CanLaunchProduction");
        launcher.Should().Contain("WindowsOfficialUninstallWorkerMode.Production");
        app.Should().Contain("CanLaunchDevelopmentVerification");
        app.Should().NotContain("CanLaunchProduction)");
    }

    private static OfficialUninstallWorkerAvailability Available(string path) =>
        new()
        {
            Status = OfficialUninstallWorkerAvailabilityStatus.ReadyForVerification,
            ExecutablePath = path
        };

    private static AuthenticodeSignatureEvidence Trusted(
        string subject,
        string thumbprint,
        char hashCharacter) =>
        new()
        {
            Status = AuthenticodeSignatureStatus.Trusted,
            SignerSubject = subject,
            SignerThumbprint = thumbprint,
            FileSha256 = new string(hashCharacter, 64)
        };

    private static AuthenticodeSignatureEvidence Evidence(
        AuthenticodeSignatureStatus status,
        char hashCharacter) =>
        new()
        {
            Status = status,
            FileSha256 = new string(hashCharacter, 64)
        };

    private static string AppOutputFile(string fileName)
    {
        var configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent?.Name ?? "Debug";
        var appProject = FindRepositoryFile("src", "Css.App", "Css.App.csproj");
        var path = Path.Combine(
            Path.GetDirectoryName(appProject)!,
            "bin",
            configuration,
            "net8.0-windows",
            fileName);
        File.Exists(path).Should().BeTrue($"the App output should contain {fileName}");
        return path;
    }

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

    private sealed class FakeVerifier(
        IReadOnlyDictionary<string, AuthenticodeSignatureEvidence> evidence)
        : IAuthenticodeSignatureVerifier
    {
        public AuthenticodeSignatureEvidence Verify(string filePath) => evidence[filePath];
    }

    private sealed class ThrowingVerifier : IAuthenticodeSignatureVerifier
    {
        public AuthenticodeSignatureEvidence Verify(string filePath) =>
            throw new IOException("private failure");
    }
}
