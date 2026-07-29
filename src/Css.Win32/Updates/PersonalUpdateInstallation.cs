using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using Css.Core.Operations;
using Css.Core.Updates;
using Css.Win32.Security;

namespace Css.Win32.Updates;

public enum PersonalUpdateDownloadStatus
{
    Ready,
    Refused,
    Failed,
    Canceled
}

public sealed record VerifiedPersonalUpdatePackage
{
    public required string Version { get; init; }
    public required string PackagePath { get; init; }
    public required long Length { get; init; }
    public required string Sha256 { get; init; }
    public required string SignerThumbprint { get; init; }
    public string? SignerSubject { get; init; }
}

public sealed record PersonalUpdateDownloadResult
{
    public required PersonalUpdateDownloadStatus Status { get; init; }
    public required string Message { get; init; }
    public VerifiedPersonalUpdatePackage? Package { get; init; }
}

public interface IPersonalUpdatePathPolicy
{
    string CreateStagingDirectory(string currentExecutablePath, string version);

    bool IsAllowedPackagePath(
        string currentExecutablePath,
        string version,
        string packagePath);
}

internal sealed class PersonalUpdateInstallLayoutException(string message)
    : InvalidOperationException(message);

public sealed class WindowsPersonalUpdatePathPolicy : IPersonalUpdatePathPolicy
{
    public string CreateStagingDirectory(
        string currentExecutablePath,
        string version)
    {
        var productRoot = ResolveProductRoot(currentExecutablePath);
        var versionRoot = Path.Combine(productRoot, "Updates", "v" + version);
        var stagingDirectory = Path.Combine(
            versionRoot,
            Guid.NewGuid().ToString("N"));
        if (!IsSafeDirectoryChain(productRoot))
            throw new InvalidOperationException("The OMNIX installation path is redirected.");
        return stagingDirectory;
    }

    public bool IsAllowedPackagePath(
        string currentExecutablePath,
        string version,
        string packagePath)
    {
        try
        {
            var productRoot = ResolveProductRoot(currentExecutablePath);
            var versionRoot = Path.GetFullPath(
                Path.Combine(productRoot, "Updates", "v" + version));
            var fullPackagePath = Path.GetFullPath(packagePath);
            var relative = Path.GetRelativePath(versionRoot, fullPackagePath);
            var segments = relative.Split(
                Path.DirectorySeparatorChar,
                StringSplitOptions.RemoveEmptyEntries);
            return segments.Length == 2
                && segments[0].Length == 32
                && segments[0].All(Uri.IsHexDigit)
                && !segments[1].Equals(".", StringComparison.Ordinal)
                && !segments[1].Equals("..", StringComparison.Ordinal)
                && !Path.IsPathRooted(relative)
                && !relative.StartsWith(
                    ".." + Path.DirectorySeparatorChar,
                    StringComparison.Ordinal)
                && IsSafeDirectoryChain(
                    Path.GetDirectoryName(fullPackagePath)
                    ?? productRoot);
        }
        catch
        {
            return false;
        }
    }

    private static string ResolveProductRoot(string currentExecutablePath)
    {
        var fullExecutablePath = Path.GetFullPath(currentExecutablePath);
        if (!File.Exists(fullExecutablePath))
            throw new InvalidOperationException("The running OMNIX executable is unavailable.");

        var installDirectory = Directory.GetParent(fullExecutablePath)
            ?? throw new InvalidOperationException("The OMNIX install directory is unavailable.");
        var productRoot = installDirectory.Parent
            ?? throw new InvalidOperationException("The OMNIX product directory is unavailable.");
        if (!installDirectory.Name.Equals("Install", StringComparison.OrdinalIgnoreCase)
            || !productRoot.Name.Equals(
                PersonalReleaseChannelPolicy.Product,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new PersonalUpdateInstallLayoutException(
                "In-app update requires the managed OMNIX installation layout.");
        }

        var root = Path.GetPathRoot(productRoot.FullName);
        var windowsRoot = Path.GetPathRoot(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows));
        if (string.IsNullOrWhiteSpace(root)
            || string.Equals(root, windowsRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The update staging directory must not be on the Windows drive.");
        }

        var drive = new DriveInfo(root);
        if (!drive.IsReady || drive.DriveType != DriveType.Fixed)
            throw new InvalidOperationException("The OMNIX drive is not available.");
        return productRoot.FullName;
    }

    private static bool IsSafeDirectoryChain(string productRoot)
    {
        var current = new DirectoryInfo(Path.GetFullPath(productRoot));
        while (current.Parent is not null)
        {
            if (current.Exists
                && current.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                return false;
            }
            current = current.Parent;
        }
        return true;
    }
}

public sealed class PersonalReleasePackageDownloader
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticodeSignatureVerifier _signatures;
    private readonly IPersonalUpdatePathPolicy _pathPolicy;

    public PersonalReleasePackageDownloader(
        HttpClient httpClient,
        IAuthenticodeSignatureVerifier signatures,
        IPersonalUpdatePathPolicy pathPolicy)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _signatures = signatures ?? throw new ArgumentNullException(nameof(signatures));
        _pathPolicy = pathPolicy ?? throw new ArgumentNullException(nameof(pathPolicy));
    }

    public async Task<PersonalUpdateDownloadResult> DownloadAndVerifyAsync(
        PersonalReleaseChannel channel,
        string currentExecutablePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(channel);
        if (!TryValidateChannel(channel, out var validatedChannel))
            return Refused("更新清单不再满足 OMNIX 的固定来源和完整性规则。");

        var package = validatedChannel.Package;
        var currentEvidence = _signatures.Verify(currentExecutablePath);
        if (!IsTrustedIdentity(
                currentEvidence,
                package.SignerThumbprint,
                expectedSha256: null))
        {
            return Refused("当前 OMNIX 的发布者身份无法确认，已停止下载更新。");
        }

        string? stagingDirectory = null;
        var retainVerifiedPackage = false;
        try
        {
            stagingDirectory = _pathPolicy.CreateStagingDirectory(
                currentExecutablePath,
                validatedChannel.Version);
            var packagePath = Path.Combine(stagingDirectory, package.AssetName);
            if (!_pathPolicy.IsAllowedPackagePath(
                    currentExecutablePath,
                    validatedChannel.Version,
                    packagePath))
            {
                return Refused("更新暂存位置不符合 D 盘安全目录规则。");
            }

            Directory.CreateDirectory(stagingDirectory);
            if (!_pathPolicy.IsAllowedPackagePath(
                    currentExecutablePath,
                    validatedChannel.Version,
                    packagePath))
            {
                return Refused("更新暂存目录发生变化，已停止更新。");
            }
            var partialPath = packagePath + ".partial";
            using var request = CreatePackageRequest(package.DownloadUrl);
            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (!response.IsSuccessStatusCode)
                return Failed("更新包下载失败，没有启动安装程序。");
            if (response.Content.Headers.ContentLength is long contentLength
                && contentLength != package.Length)
            {
                return Refused("更新包大小与发布记录不一致，已停止更新。");
            }

            await DownloadExactAsync(
                response,
                partialPath,
                package.Length,
                cancellationToken);
            var actualSha256 = await ComputeSha256Async(
                partialPath,
                cancellationToken);
            if (!HashesEqual(actualSha256, package.SHA256))
                return Refused("更新包哈希不一致，文件可能损坏或被替换。");

            var packageEvidence = _signatures.Verify(partialPath);
            if (!IsTrustedIdentity(
                    packageEvidence,
                    package.SignerThumbprint,
                    package.SHA256)
                || !ThumbprintsEqual(
                    currentEvidence.SignerThumbprint,
                    packageEvidence.SignerThumbprint))
            {
                return Refused("更新包不是当前 OMNIX 的同一可信发布者，已停止更新。");
            }

            File.Move(partialPath, packagePath, overwrite: false);
            var finalEvidence = _signatures.Verify(packagePath);
            if (!IsTrustedIdentity(
                    finalEvidence,
                    package.SignerThumbprint,
                    package.SHA256))
            {
                return Refused("更新包落盘后的签名复核失败，已停止更新。");
            }

            retainVerifiedPackage = true;
            return new PersonalUpdateDownloadResult
            {
                Status = PersonalUpdateDownloadStatus.Ready,
                Message = $"版本 {validatedChannel.Version} 已下载，哈希和发布者签名均已验证。",
                Package = new VerifiedPersonalUpdatePackage
                {
                    Version = validatedChannel.Version,
                    PackagePath = packagePath,
                    Length = package.Length,
                    Sha256 = package.SHA256.ToUpperInvariant(),
                    SignerThumbprint = package.SignerThumbprint.ToUpperInvariant(),
                    SignerSubject = finalEvidence.SignerSubject
                }
            };
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return Failed("更新包下载超时，请检查网络后重试。");
        }
        catch (OperationCanceledException)
        {
            return new PersonalUpdateDownloadResult
            {
                Status = PersonalUpdateDownloadStatus.Canceled,
                Message = "更新下载已取消，没有启动安装程序。"
            };
        }
        catch (HttpRequestException)
        {
            return Failed("无法下载更新包，请检查网络后重试。");
        }
        catch (InvalidDataException)
        {
            return Refused("更新包大小与发布记录不一致，已停止更新。");
        }
        catch (PersonalUpdateInstallLayoutException)
        {
            return Refused(
                "OMNIX 的安装位置不正确，无法安全更新。请重新安装到 "
                + @"D:\Software\OMNIX-Entropy\Install"
                + " 后再检查更新；没有下载或启动安装程序。");
        }
        catch (Exception)
        {
            return Failed("更新包没有准备完成，也没有启动安装程序。");
        }
        finally
        {
            // Sole cleanup authority: only a fully verified package survives, so
            // every refusal, failure, cancellation, and throw exits through here.
            if (!retainVerifiedPackage)
            {
                CleanupStaging(
                    currentExecutablePath,
                    validatedChannel.Version,
                    stagingDirectory);
            }
        }
    }

    private static bool TryValidateChannel(
        PersonalReleaseChannel channel,
        out PersonalReleaseChannel validatedChannel)
    {
        var result = PersonalReleaseChannelPolicy.ParseAndValidate(
            JsonSerializer.Serialize(channel));
        validatedChannel = result.Channel ?? new PersonalReleaseChannel();
        return result.IsValid && result.Channel is not null;
    }

    private static HttpRequestMessage CreatePackageRequest(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.Add(
            new ProductInfoHeaderValue("OMNIX-Entropy", "0.1"));
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/octet-stream"));
        return request;
    }

    private static async Task DownloadExactAsync(
        HttpResponseMessage response,
        string partialPath,
        long expectedLength,
        CancellationToken cancellationToken)
    {
        await using var input = await response.Content.ReadAsStreamAsync(
            cancellationToken);
        await using var output = new FileStream(
            partialPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        var buffer = new byte[64 * 1024];
        long total = 0;
        while (true)
        {
            var read = await input.ReadAsync(buffer, cancellationToken);
            if (read == 0)
                break;
            total += read;
            if (total > expectedLength)
                throw new InvalidDataException("Update package exceeded expected length.");
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
        await output.FlushAsync(cancellationToken);
        if (total != expectedLength)
            throw new InvalidDataException("Update package length did not match.");
    }

    private static async Task<string> ComputeSha256Async(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Convert.ToHexString(
            await SHA256.HashDataAsync(stream, cancellationToken));
    }

    private static bool IsTrustedIdentity(
        AuthenticodeSignatureEvidence evidence,
        string expectedThumbprint,
        string? expectedSha256) =>
        evidence.IsTrusted
        && ThumbprintsEqual(evidence.SignerThumbprint, expectedThumbprint)
        && (expectedSha256 is null
            || HashesEqual(evidence.FileSha256, expectedSha256));

    internal static bool ThumbprintsEqual(string? left, string? right) =>
        string.Equals(
            NormalizeHex(left),
            NormalizeHex(right),
            StringComparison.Ordinal);

    internal static bool HashesEqual(string? left, string? right) =>
        string.Equals(
            NormalizeHex(left),
            NormalizeHex(right),
            StringComparison.Ordinal);

    private static string NormalizeHex(string? value) =>
        new((value ?? string.Empty)
            .Where(Uri.IsHexDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());

    private void CleanupStaging(
        string currentExecutablePath,
        string version,
        string? stagingDirectory)
    {
        if (string.IsNullOrWhiteSpace(stagingDirectory))
            return;
        try
        {
            var proofPath = Path.Combine(
                stagingDirectory,
                $"OMNIX-Entropy-{version}-win-x64-setup.exe");
            if (_pathPolicy.IsAllowedPackagePath(
                    currentExecutablePath,
                    version,
                    proofPath)
                && Directory.Exists(stagingDirectory))
            {
                Directory.Delete(stagingDirectory, recursive: true);
            }
        }
        catch
        {
            // A failed cleanup never turns an unverified package into launch authority.
        }
    }

    private static PersonalUpdateDownloadResult Refused(string message) => new()
    {
        Status = PersonalUpdateDownloadStatus.Refused,
        Message = message
    };

    private static PersonalUpdateDownloadResult Failed(string message) => new()
    {
        Status = PersonalUpdateDownloadStatus.Failed,
        Message = message
    };
}

public sealed record PersonalUpdateInstallerLaunchRequest
{
    public required string PackagePath { get; init; }
    public IReadOnlyList<string> Arguments { get; init; } = [];
}

public enum PersonalUpdateInstallerLaunchStatus
{
    Started,
    UserCanceled,
    Failed
}

public sealed record PersonalUpdateInstallerLaunchResult
{
    public required PersonalUpdateInstallerLaunchStatus Status { get; init; }
}

public interface IPersonalUpdateInstallerLauncher
{
    ValueTask<PersonalUpdateInstallerLaunchResult> LaunchAsync(
        PersonalUpdateInstallerLaunchRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class WindowsPersonalUpdateInstallerLauncher
    : IPersonalUpdateInstallerLauncher
{
    public ValueTask<PersonalUpdateInstallerLaunchResult> LaunchAsync(
        PersonalUpdateInstallerLaunchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Arguments.Count != 0)
        {
            return ValueTask.FromResult(new PersonalUpdateInstallerLaunchResult
            {
                Status = PersonalUpdateInstallerLaunchStatus.Failed
            });
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = request.PackagePath,
                UseShellExecute = true
            });
            return ValueTask.FromResult(new PersonalUpdateInstallerLaunchResult
            {
                Status = process is null
                    ? PersonalUpdateInstallerLaunchStatus.Failed
                    : PersonalUpdateInstallerLaunchStatus.Started
            });
        }
        catch (Win32Exception exception)
            when (exception.NativeErrorCode == 1223)
        {
            return ValueTask.FromResult(new PersonalUpdateInstallerLaunchResult
            {
                Status = PersonalUpdateInstallerLaunchStatus.UserCanceled
            });
        }
        catch
        {
            return ValueTask.FromResult(new PersonalUpdateInstallerLaunchResult
            {
                Status = PersonalUpdateInstallerLaunchStatus.Failed
            });
        }
    }
}

public static class PersonalUpdateLaunchOperationPlanner
{
    public const string OperationKind = "update.install-interactive";

    public static OperationDescriptor Create(
        VerifiedPersonalUpdatePackage package,
        string currentExecutablePath)
    {
        ArgumentNullException.ThrowIfNull(package);
        return new OperationDescriptor
        {
            Kind = OperationKind,
            Title = $"安装 OMNIX-Entropy {package.Version}",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = false,
            RequiresSnapshot = false,
            RollbackRequired = false,
            ConfirmationAccepted = false,
            EvidenceSummary =
                "更新包大小、SHA-256 和 Windows 发布者签名已验证，并与当前 OMNIX 发布者一致。",
            EstimatedImpactBytes = package.Length,
            ConfirmationText =
                "打开交互式安装程序？OMNIX 会关闭当前窗口，但不会静默点击安装。",
            AffectedPaths = [currentExecutablePath, package.PackagePath],
            Arguments = new Dictionary<string, object?>
            {
                ["version"] = package.Version,
                ["packagePath"] = package.PackagePath,
                ["packageLength"] = package.Length.ToString(CultureInfo.InvariantCulture),
                ["packageSha256"] = package.Sha256,
                ["signerThumbprint"] = package.SignerThumbprint,
                ["currentExecutablePath"] = currentExecutablePath
            }
        };
    }

    public static OperationDescriptor Confirm(
        OperationDescriptor operation,
        DateTimeOffset confirmedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (operation.Kind != OperationKind
            || operation.ConfirmationAccepted)
        {
            throw new InvalidOperationException("The update operation cannot be confirmed.");
        }

        var arguments = operation.Arguments.ToDictionary(
            pair => pair.Key,
            pair => pair.Value);
        arguments["confirmedAtUtc"] = confirmedAtUtc
            .ToUniversalTime()
            .ToString("O");
        return new OperationDescriptor
        {
            Kind = operation.Kind,
            Title = operation.Title,
            Source = operation.Source,
            Risk = operation.Risk,
            IsDestructive = operation.IsDestructive,
            RequiresElevation = operation.RequiresElevation,
            RequiresSnapshot = operation.RequiresSnapshot,
            SnapshotId = operation.SnapshotId,
            RollbackRequired = operation.RollbackRequired,
            ConfirmationAccepted = true,
            EvidenceSummary = operation.EvidenceSummary,
            EstimatedImpactBytes = operation.EstimatedImpactBytes,
            ConfirmationText = operation.ConfirmationText,
            AffectedPaths = operation.AffectedPaths,
            AffectedRegistryKeys = operation.AffectedRegistryKeys,
            AffectedServices = operation.AffectedServices,
            Arguments = arguments
        };
    }
}

public sealed class PersonalUpdateLaunchOperationHandler
{
    private static readonly HashSet<string> AllowedArgumentKeys =
        new(StringComparer.Ordinal)
        {
            "version",
            "packagePath",
            "packageLength",
            "packageSha256",
            "signerThumbprint",
            "currentExecutablePath",
            "confirmedAtUtc"
        };

    private readonly IAuthenticodeSignatureVerifier _signatures;
    private readonly IPersonalUpdatePathPolicy _pathPolicy;
    private readonly IPersonalUpdateInstallerLauncher _launcher;
    private readonly string _currentExecutablePath;

    public PersonalUpdateLaunchOperationHandler(
        IAuthenticodeSignatureVerifier signatures,
        IPersonalUpdatePathPolicy pathPolicy,
        IPersonalUpdateInstallerLauncher launcher,
        string currentExecutablePath)
    {
        _signatures = signatures ?? throw new ArgumentNullException(nameof(signatures));
        _pathPolicy = pathPolicy ?? throw new ArgumentNullException(nameof(pathPolicy));
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        _currentExecutablePath = Path.GetFullPath(currentExecutablePath);
    }

    public async Task<OperationResult> ExecuteAsync(
        OperationDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (!HasExpectedShape(descriptor)
            || !TryReadString(descriptor, "version", out var version)
            || !TryReadString(descriptor, "packagePath", out var packagePath)
            || !TryReadString(
                descriptor,
                "currentExecutablePath",
                out var currentExecutablePath)
            || !TryReadString(
                descriptor,
                "packageSha256",
                out var expectedSha256)
            || !TryReadString(
                descriptor,
                "signerThumbprint",
                out var expectedThumbprint)
            || !TryReadLong(descriptor, "packageLength", out var expectedLength)
            || !PathsEqual(currentExecutablePath, _currentExecutablePath)
            || !PathsEqual(descriptor.AffectedPaths[0], _currentExecutablePath)
            || !PathsEqual(descriptor.AffectedPaths[1], packagePath)
            || !_pathPolicy.IsAllowedPackagePath(
                _currentExecutablePath,
                version,
                packagePath))
        {
            return OperationResult.Fail("更新操作证据不完整，安装程序没有启动。");
        }

        try
        {
            var fullPackagePath = Path.GetFullPath(packagePath);
            var expectedName =
                $"OMNIX-Entropy-{version}-win-x64-setup.exe";
            if (!Path.GetFileName(fullPackagePath).Equals(
                    expectedName,
                    StringComparison.Ordinal)
                || !File.Exists(fullPackagePath)
                || new FileInfo(fullPackagePath).Length != expectedLength)
            {
                return OperationResult.Fail("更新包已变化，安装程序没有启动。");
            }

            var currentEvidence = _signatures.Verify(_currentExecutablePath);
            var packageEvidence = _signatures.Verify(fullPackagePath);
            if (!currentEvidence.IsTrusted
                || !packageEvidence.IsTrusted
                || !PersonalReleasePackageDownloader.ThumbprintsEqual(
                    currentEvidence.SignerThumbprint,
                    expectedThumbprint)
                || !PersonalReleasePackageDownloader.ThumbprintsEqual(
                    packageEvidence.SignerThumbprint,
                    expectedThumbprint)
                || !PersonalReleasePackageDownloader.HashesEqual(
                    packageEvidence.FileSha256,
                    expectedSha256))
            {
                return OperationResult.Fail(
                    "更新包的哈希或同发布者签名复核失败，安装程序没有启动。");
            }

            var launch = await _launcher.LaunchAsync(
                new PersonalUpdateInstallerLaunchRequest
                {
                    PackagePath = fullPackagePath,
                    Arguments = []
                },
                cancellationToken);
            return launch.Status switch
            {
                PersonalUpdateInstallerLaunchStatus.Started =>
                    OperationResult.Ok(
                        "交互式安装程序已打开。OMNIX 将退出，安装步骤仍由你确认。"),
                PersonalUpdateInstallerLaunchStatus.UserCanceled =>
                    OperationResult.Fail("你取消了安装程序，当前版本没有变化。"),
                _ => OperationResult.Fail("安装程序没有成功打开，当前版本没有变化。")
            };
        }
        catch
        {
            return OperationResult.Fail("更新包复核失败，安装程序没有启动。");
        }
    }

    private static bool HasExpectedShape(OperationDescriptor descriptor) =>
        descriptor.Kind == PersonalUpdateLaunchOperationPlanner.OperationKind
        && descriptor.Source == OperationSource.Manual
        && descriptor.Risk == RiskLevel.High
        && descriptor.IsDestructive
        && descriptor.ConfirmationAccepted
        && !descriptor.RequiresElevation
        && !descriptor.RequiresSnapshot
        && !descriptor.RollbackRequired
        && descriptor.Arguments.Count == AllowedArgumentKeys.Count
        && descriptor.Arguments.Keys.All(AllowedArgumentKeys.Contains)
        && descriptor.AffectedPaths.Count == 2;

    private static bool TryReadString(
        OperationDescriptor descriptor,
        string key,
        out string value)
    {
        value = string.Empty;
        if (!descriptor.Arguments.TryGetValue(key, out var raw)
            || raw is not string text
            || string.IsNullOrWhiteSpace(text))
        {
            return false;
        }
        value = text;
        return true;
    }

    private static bool TryReadLong(
        OperationDescriptor descriptor,
        string key,
        out long value)
    {
        value = 0;
        return TryReadString(descriptor, key, out var text)
            && long.TryParse(
                text,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out value)
            && value > 0
            && value <= PersonalReleaseChannelPolicy.MaximumPackageBytes;
    }

    private static bool PathsEqual(string left, string right)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
