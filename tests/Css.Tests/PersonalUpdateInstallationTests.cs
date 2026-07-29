using System.Net;
using System.Security.Cryptography;
using Css.Core.Operations;
using Css.Core.Updates;
using Css.Win32.Security;
using Css.Win32.Updates;
using FluentAssertions;

namespace Css.Tests;

public sealed class PersonalUpdateInstallationTests : IDisposable
{
    private const string SignerThumbprint = "5688958FEA0056861558E8DCF9D2381AF46074B2";
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "omnix-update-tests-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task Valid_package_is_staged_and_requires_confirmed_pipeline_launch()
    {
        var packageBytes = "signed OMNIX update package"u8.ToArray();
        var channel = CreateChannel(packageBytes);
        var currentExecutable = CreateCurrentExecutable();
        var pathPolicy = new TestPathPolicy(_root);
        var signatures = new TestSignatureVerifier(currentExecutable, SignerThumbprint);
        var http = new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(packageBytes)
        });
        var downloader = new PersonalReleasePackageDownloader(
            new HttpClient(http),
            signatures,
            pathPolicy);

        var download = await downloader.DownloadAndVerifyAsync(
            channel,
            currentExecutable);

        download.Status.Should().Be(PersonalUpdateDownloadStatus.Ready);
        download.Package.Should().NotBeNull();
        File.ReadAllBytes(download.Package!.PackagePath).Should().Equal(packageBytes);
        http.Requests.Should().Equal(channel.Package.DownloadUrl);

        var operation = PersonalUpdateLaunchOperationPlanner.Create(
            download.Package,
            currentExecutable);
        var launcher = new RecordingLauncher();
        var handler = new PersonalUpdateLaunchOperationHandler(
            signatures,
            pathPolicy,
            launcher,
            currentExecutable);
        var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

        var refused = await pipeline.ExecuteAsync(operation);

        refused.Success.Should().BeFalse();
        launcher.Requests.Should().BeEmpty();

        var confirmed = PersonalUpdateLaunchOperationPlanner.Confirm(
            operation,
            DateTimeOffset.Parse("2026-07-28T08:00:00Z"));
        var started = await pipeline.ExecuteAsync(confirmed);

        started.Success.Should().BeTrue();
        launcher.Requests.Should().ContainSingle();
        launcher.Requests[0].PackagePath.Should().Be(download.Package.PackagePath);
        launcher.Requests[0].Arguments.Should().BeEmpty();
    }

    [Fact]
    public async Task Hash_or_length_mismatch_is_refused_and_not_left_as_executable()
    {
        var expectedBytes = "expected package"u8.ToArray();
        var downloadedBytes = "substituted package with another length"u8.ToArray();
        var currentExecutable = CreateCurrentExecutable();
        var pathPolicy = new TestPathPolicy(_root);
        var downloader = new PersonalReleasePackageDownloader(
            new HttpClient(new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(downloadedBytes)
            })),
            new TestSignatureVerifier(currentExecutable, SignerThumbprint),
            pathPolicy);

        var result = await downloader.DownloadAndVerifyAsync(
            CreateChannel(expectedBytes),
            currentExecutable);

        result.Status.Should().Be(PersonalUpdateDownloadStatus.Refused);
        Directory.Exists(_root)
            .Should().BeTrue();
        var updatesRoot = Path.Combine(_root, "Updates");
        if (Directory.Exists(updatesRoot))
        {
            Directory.EnumerateFiles(
                    updatesRoot,
                    "*.exe",
                    SearchOption.AllDirectories)
                .Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Different_or_untrusted_signer_is_refused()
    {
        var packageBytes = "signed package"u8.ToArray();
        var currentExecutable = CreateCurrentExecutable();
        var pathPolicy = new TestPathPolicy(_root);
        var signatures = new TestSignatureVerifier(
            currentExecutable,
            SignerThumbprint,
            packageThumbprint: new string('A', 40));
        var downloader = new PersonalReleasePackageDownloader(
            new HttpClient(new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(packageBytes)
            })),
            signatures,
            pathPolicy);

        var result = await downloader.DownloadAndVerifyAsync(
            CreateChannel(packageBytes),
            currentExecutable);

        result.Status.Should().Be(PersonalUpdateDownloadStatus.Refused);
        result.Package.Should().BeNull();
    }

    [Fact]
    public async Task Failed_signature_recheck_after_final_move_removes_executable()
    {
        var packageBytes = "signed package"u8.ToArray();
        var currentExecutable = CreateCurrentExecutable();
        var pathPolicy = new TestPathPolicy(_root);
        var signatures = new TestSignatureVerifier(
            currentExecutable,
            SignerThumbprint,
            finalPackageThumbprint: new string('A', 40));
        var downloader = new PersonalReleasePackageDownloader(
            new HttpClient(new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(packageBytes)
            })),
            signatures,
            pathPolicy);

        var result = await downloader.DownloadAndVerifyAsync(
            CreateChannel(packageBytes),
            currentExecutable);

        result.Status.Should().Be(PersonalUpdateDownloadStatus.Refused);
        var updatesRoot = Path.Combine(_root, "Updates");
        if (Directory.Exists(updatesRoot))
        {
            Directory.EnumerateFiles(
                    updatesRoot,
                    "*.exe",
                    SearchOption.AllDirectories)
                .Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Package_changed_after_download_is_refused_before_process_launch()
    {
        var packageBytes = "signed package"u8.ToArray();
        var channel = CreateChannel(packageBytes);
        var currentExecutable = CreateCurrentExecutable();
        var pathPolicy = new TestPathPolicy(_root);
        var signatures = new TestSignatureVerifier(currentExecutable, SignerThumbprint);
        var downloader = new PersonalReleasePackageDownloader(
            new HttpClient(new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(packageBytes)
            })),
            signatures,
            pathPolicy);
        var download = await downloader.DownloadAndVerifyAsync(
            channel,
            currentExecutable);
        download.Status.Should().Be(PersonalUpdateDownloadStatus.Ready);
        await File.AppendAllTextAsync(download.Package!.PackagePath, "changed");

        var operation = PersonalUpdateLaunchOperationPlanner.Confirm(
            PersonalUpdateLaunchOperationPlanner.Create(
                download.Package,
                currentExecutable),
            DateTimeOffset.Parse("2026-07-28T08:00:00Z"));
        var launcher = new RecordingLauncher();
        var pipeline = new SafetyOperationPipeline(
            new PersonalUpdateLaunchOperationHandler(
                signatures,
                pathPolicy,
                launcher,
                currentExecutable).ExecuteAsync);

        var result = await pipeline.ExecuteAsync(operation);

        result.Success.Should().BeFalse();
        launcher.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Launch_handler_refuses_descriptor_bound_to_another_executable()
    {
        var packageBytes = "signed package"u8.ToArray();
        var currentExecutable = CreateCurrentExecutable();
        var pathPolicy = new TestPathPolicy(_root);
        var signatures = new TestSignatureVerifier(currentExecutable, SignerThumbprint);
        var downloader = new PersonalReleasePackageDownloader(
            new HttpClient(new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(packageBytes)
            })),
            signatures,
            pathPolicy);
        var download = await downloader.DownloadAndVerifyAsync(
            CreateChannel(packageBytes),
            currentExecutable);
        var alternateExecutable = Path.Combine(_root, "Other", "Css.App.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(alternateExecutable)!);
        File.WriteAllText(alternateExecutable, "another signed app copy");
        var operation = PersonalUpdateLaunchOperationPlanner.Confirm(
            PersonalUpdateLaunchOperationPlanner.Create(
                download.Package!,
                alternateExecutable),
            DateTimeOffset.Parse("2026-07-28T08:00:00Z"));
        var launcher = new RecordingLauncher();
        var pipeline = new SafetyOperationPipeline(
            new PersonalUpdateLaunchOperationHandler(
                signatures,
                pathPolicy,
                launcher,
                currentExecutable).ExecuteAsync);

        var result = await pipeline.ExecuteAsync(operation);

        result.Success.Should().BeFalse();
        launcher.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Cancellation_stops_download_and_leaves_no_update_executable()
    {
        var packageBytes = "signed package"u8.ToArray();
        var currentExecutable = CreateCurrentExecutable();
        var downloader = new PersonalReleasePackageDownloader(
            new HttpClient(new CancelingHandler()),
            new TestSignatureVerifier(currentExecutable, SignerThumbprint),
            new TestPathPolicy(_root));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await downloader.DownloadAndVerifyAsync(
            CreateChannel(packageBytes),
            currentExecutable,
            cancellation.Token);

        result.Status.Should().Be(PersonalUpdateDownloadStatus.Canceled);
        var updatesRoot = Path.Combine(_root, "Updates");
        if (Directory.Exists(updatesRoot))
        {
            Directory.EnumerateFiles(
                    updatesRoot,
                    "*.exe",
                    SearchOption.AllDirectories)
                .Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Doubled_install_layout_is_refused_with_reinstall_guidance()
    {
        var packageBytes = "signed package"u8.ToArray();
        var currentExecutable = Path.Combine(
            _root,
            "OMNIX-Entropy",
            "Install",
            "Install",
            "Css.App.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(currentExecutable)!);
        File.WriteAllText(currentExecutable, "current signed app");
        var http = new RecordingHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(packageBytes)
        });
        var downloader = new PersonalReleasePackageDownloader(
            new HttpClient(http),
            new TestSignatureVerifier(currentExecutable, SignerThumbprint),
            new WindowsPersonalUpdatePathPolicy());

        var result = await downloader.DownloadAndVerifyAsync(
            CreateChannel(packageBytes),
            currentExecutable);

        result.Status.Should().Be(PersonalUpdateDownloadStatus.Refused);
        result.Package.Should().BeNull();
        result.Message.Should().Contain("安装位置不正确")
            .And.Contain("重新安装")
            .And.Contain(@"D:\Software\OMNIX-Entropy\Install")
            .And.Contain("没有下载或启动安装程序");
        http.Requests.Should().BeEmpty();
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private string CreateCurrentExecutable()
    {
        var directory = Path.Combine(_root, "Install");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "Css.App.exe");
        File.WriteAllText(path, "current signed app");
        return path;
    }

    private static PersonalReleaseChannel CreateChannel(byte[] packageBytes)
    {
        var sha256 = Convert.ToHexString(SHA256.HashData(packageBytes));
        return new PersonalReleaseChannel
        {
            SchemaVersion = 1,
            Product = "OMNIX-Entropy",
            Repository = "plnoble/OMNIX-Entropy",
            Version = "0.1.2",
            Tag = "v0.1.2",
            CommitSHA = new string('A', 40),
            GeneratedAtUtc = DateTimeOffset.Parse("2026-07-28T08:00:00Z"),
            Package = new PersonalReleasePackage
            {
                AssetName = "OMNIX-Entropy-0.1.2-win-x64-setup.exe",
                DownloadUrl =
                    "https://github.com/plnoble/OMNIX-Entropy/releases/download/v0.1.2/OMNIX-Entropy-0.1.2-win-x64-setup.exe",
                Length = packageBytes.LongLength,
                SHA256 = sha256,
                InstallerManifestSHA256 = new string('B', 64),
                SignerThumbprint = SignerThumbprint,
                ValidSameSigner = true
            }
        };
    }

    private sealed class TestPathPolicy(string root) : IPersonalUpdatePathPolicy
    {
        public string CreateStagingDirectory(
            string currentExecutablePath,
            string version) =>
            Path.Combine(root, "Updates", "v" + version, Guid.NewGuid().ToString("N"));

        public bool IsAllowedPackagePath(
            string currentExecutablePath,
            string version,
            string packagePath) =>
            Path.GetFullPath(packagePath).StartsWith(
                Path.GetFullPath(Path.Combine(root, "Updates", "v" + version))
                    + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TestSignatureVerifier(
        string currentExecutable,
        string currentThumbprint,
        string? packageThumbprint = null,
        string? finalPackageThumbprint = null) : IAuthenticodeSignatureVerifier
    {
        public AuthenticodeSignatureEvidence Verify(string filePath)
        {
            var fullPath = Path.GetFullPath(filePath);
            if (!File.Exists(fullPath))
            {
                return new AuthenticodeSignatureEvidence
                {
                    Status = AuthenticodeSignatureStatus.Missing
                };
            }

            var isCurrent = string.Equals(
                fullPath,
                Path.GetFullPath(currentExecutable),
                StringComparison.OrdinalIgnoreCase);
            return new AuthenticodeSignatureEvidence
            {
                Status = AuthenticodeSignatureStatus.Trusted,
                SignerSubject = "CN=OMNIX-Entropy Personal Publisher",
                SignerThumbprint = isCurrent
                    ? currentThumbprint
                    : fullPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                        ? finalPackageThumbprint
                            ?? packageThumbprint
                            ?? currentThumbprint
                        : packageThumbprint ?? currentThumbprint,
                FileSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fullPath)))
            };
        }
    }

    private sealed class RecordingHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public List<string> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!.AbsoluteUri);
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }

    private sealed class CancelingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromCanceled<HttpResponseMessage>(
                cancellationToken.IsCancellationRequested
                    ? cancellationToken
                    : new CancellationToken(canceled: true));
    }

    private sealed class RecordingLauncher : IPersonalUpdateInstallerLauncher
    {
        public List<PersonalUpdateInstallerLaunchRequest> Requests { get; } = [];

        public ValueTask<PersonalUpdateInstallerLaunchResult> LaunchAsync(
            PersonalUpdateInstallerLaunchRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return ValueTask.FromResult(new PersonalUpdateInstallerLaunchResult
            {
                Status = PersonalUpdateInstallerLaunchStatus.Started
            });
        }
    }
}
