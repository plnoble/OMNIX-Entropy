using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using Css.Core.Operations;
using Css.Win32.Security;

namespace Css.InstallGuard.Installers;

public sealed record InteractiveInstallerLaunchRequest
{
    public required string PackagePath { get; init; }
    public required string ExpectedSha256 { get; init; }
    public IReadOnlyList<string> Arguments { get; init; } = [];
}

public enum InteractiveInstallerLaunchStatus
{
    Started,
    UserCanceled,
    Refused,
    Failed
}

public interface IInteractiveInstallerProcessSession : IDisposable
{
    Task WaitForExitAsync(CancellationToken cancellationToken = default);
    int? ExitCode { get; }
}

public sealed record InteractiveInstallerLaunchResult
{
    public required InteractiveInstallerLaunchStatus Status { get; init; }
    public IInteractiveInstallerProcessSession? Session { get; init; }
}

public interface IInteractiveInstallerProcessLauncher
{
    ValueTask<InteractiveInstallerLaunchResult> LaunchAsync(
        InteractiveInstallerLaunchRequest request,
        CancellationToken cancellationToken = default);
}

public interface IInstallerTargetPathPolicy
{
    bool IsAllowed(string targetPath, out string? reason);
}

public sealed class WindowsInstallerTargetPathPolicy : IInstallerTargetPathPolicy
{
    private static readonly HashSet<string> AllowedTopLevelFolders =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Software",
            "Game",
            "Agent",
            "Development"
        };

    public bool IsAllowed(string targetPath, out string? reason)
    {
        reason = null;
        try
        {
            if (string.IsNullOrWhiteSpace(targetPath) || targetPath.StartsWith("\\\\"))
                return Refuse("Installer target must be a local folder.", out reason);
            var fullPath = Path.GetFullPath(targetPath);
            var root = Path.GetPathRoot(fullPath);
            if (string.IsNullOrWhiteSpace(root) || !root.EndsWith(":\\", StringComparison.Ordinal))
                return Refuse("Installer target must be on a local drive.", out reason);
            var windowsRoot = Path.GetPathRoot(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            if (string.IsNullOrWhiteSpace(windowsRoot))
                windowsRoot = @"C:\";
            if (string.Equals(root, windowsRoot, StringComparison.OrdinalIgnoreCase))
                return Refuse("Installer target cannot be on the Windows system drive.", out reason);

            var segments = fullPath[root.Length..]
                .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 3
                || !AllowedTopLevelFolders.Contains(segments[0])
                || !segments[^1].Equals("Install", StringComparison.OrdinalIgnoreCase))
            {
                return Refuse("Installer target is outside the OMNIX managed layout.", out reason);
            }

            var drive = new DriveInfo(root);
            if (!drive.IsReady || drive.DriveType != DriveType.Fixed)
                return Refuse("Installer target drive is unavailable or not fixed storage.", out reason);
            return true;
        }
        catch
        {
            return Refuse("Installer target path could not be validated.", out reason);
        }
    }

    private static bool Refuse(string message, out string? reason)
    {
        reason = message;
        return false;
    }
}

public sealed record InstallerLaunchOperationPlan
{
    public required OperationDescriptor Operation { get; init; }
    public required InstallerPackageEvidence PackageEvidence { get; init; }
    public required InstallerRoutingCapability Capability { get; init; }
    public required InstallBeforeSnapshotEvidenceCreationResult BeforeSnapshot { get; init; }
}

public static class InstallerLaunchOperationPlanner
{
    public const string OperationKind = "install.launch-interactive";

    public static InstallerLaunchOperationPlan Create(
        InstallerDetectionResult analysis,
        InstallerPackageEvidence package,
        InstallerRoutingCapability capability,
        InstallBeforeSnapshotEvidenceCreationResult beforeSnapshot,
        IInstallerTargetPathPolicy targetPathPolicy)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(beforeSnapshot);
        ArgumentNullException.ThrowIfNull(targetPathPolicy);
        if (!capability.CanRequestInstallerLaunch
            || capability.Mode is InstallerRoutingCapabilityMode.Refused
                or InstallerRoutingCapabilityMode.WindowsManagedStorage)
        {
            throw new InvalidOperationException("This installer cannot be launched by OMNIX.");
        }
        if (!targetPathPolicy.IsAllowed(capability.TargetInstallPath, out _))
            throw new InvalidOperationException("The installer target is unavailable or unsafe.");
        if (!package.HasStableIdentity
            || package.SignatureStatus != AuthenticodeSignatureStatus.Trusted
            || !InstallBeforeSnapshotEvidenceService.PathsEqual(
                analysis.InstallerPath,
                package.PackagePath)
            || !InstallBeforeSnapshotEvidenceService.PathsEqual(
                package.PackagePath,
                beforeSnapshot.Evidence.PackagePath)
            || !InstallBeforeSnapshotEvidenceService.HashesEqual(
                package.Sha256,
                beforeSnapshot.Evidence.PackageSha256))
        {
            throw new InvalidOperationException("Installer evidence is incomplete or inconsistent.");
        }

        var packageLastWriteUtc = package.LastWriteUtc?.ToUniversalTime().ToString("O")
            ?? throw new InvalidOperationException("Installer write time is unavailable.");
        var operation = new OperationDescriptor
        {
            Kind = OperationKind,
            Title = "打开 " + analysis.SoftwareName + " 安装界面",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = false,
            RequiresSnapshot = true,
            SnapshotId = beforeSnapshot.Evidence.SnapshotId,
            RollbackRequired = false,
            ConfirmationAccepted = false,
            EvidenceSummary = "已核验安装包发布者、文件指纹、安装器类型和安装前清单。",
            EstimatedImpactBytes = 0,
            ConfirmationText = "我确认打开这个安装包；OMNIX 不会替我点击安装，也不能保证安装器完全遵守目标位置。",
            AffectedPaths = [capability.TargetInstallPath],
            Arguments = new Dictionary<string, object?>
            {
                ["packagePath"] = package.PackagePath,
                ["packageSha256"] = package.Sha256,
                ["packageLengthBytes"] = package.LengthBytes.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["packageLastWriteUtc"] = packageLastWriteUtc,
                ["installerKind"] = package.DetectedKind.ToString(),
                ["installerKindConfidence"] = package.KindConfidence.ToString(),
                ["targetInstallPath"] = capability.TargetInstallPath,
                ["interactiveArguments"] = capability.InteractiveArguments.ToArray(),
                ["snapshotEvidencePath"] = beforeSnapshot.EvidencePath,
                ["snapshotEvidenceSha256"] = beforeSnapshot.Sha256
            }
        };
        return new InstallerLaunchOperationPlan
        {
            Operation = operation,
            PackageEvidence = package,
            Capability = capability,
            BeforeSnapshot = beforeSnapshot
        };
    }
}

public sealed record InstallerLaunchFinalConsentViewModel
{
    public required string Title { get; init; }
    public required string Headline { get; init; }
    public required string PublisherText { get; init; }
    public required string TargetText { get; init; }
    public required string LocationWarning { get; init; }
    public required string PackageAcknowledgement { get; init; }
    public required string LocationAcknowledgement { get; init; }
    public required string InteractionAcknowledgement { get; init; }
    public required string ReportAcknowledgement { get; init; }
    public required string ConfirmButtonText { get; init; }
    public required string CancelButtonText { get; init; }
}

public sealed record InstallerLaunchFinalConsentDecision
{
    public bool PackagePublisherAccepted { get; init; }
    public bool LocationLimitAccepted { get; init; }
    public bool InteractiveReviewAccepted { get; init; }
    public bool PostScanLimitAccepted { get; init; }

    public bool IsComplete =>
        PackagePublisherAccepted
        && LocationLimitAccepted
        && InteractiveReviewAccepted
        && PostScanLimitAccepted;
}

public static class InstallerLaunchFinalConsentPresenter
{
    public static InstallerLaunchFinalConsentViewModel Create(
        InstallerLaunchOperationPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var publisher = string.IsNullOrWhiteSpace(plan.PackageEvidence.SignerSubject)
            ? "发布者签名已通过 Windows 验证"
            : "已验证发布者: " + plan.PackageEvidence.SignerSubject;
        return new InstallerLaunchFinalConsentViewModel
        {
            Title = "最后确认",
            Headline = "OMNIX 将打开安装界面，不会替你完成安装",
            PublisherText = publisher,
            TargetText = "建议安装到: " + plan.Capability.TargetInstallPath,
            LocationWarning = "有些软件即使装在 D 盘，仍会在 C 盘保存配置、缓存或更新文件；安装后 Agent 会重新扫描并解释。",
            PackageAcknowledgement = "我确认这是我刚刚选择、发布者已验证的安装包",
            LocationAcknowledgement = "我知道推荐目录不能保证软件以后完全不写 C 盘",
            InteractionAcknowledgement = "我会在安装界面中检查位置，不让 OMNIX 替我点击安装",
            ReportAcknowledgement = "我知道安装后扫描只能报告变化，不能把启动进程当作安装成功",
            ConfirmButtonText = "打开安装界面",
            CancelButtonText = "先不安装"
        };
    }
}

public static class InstallerLaunchFinalConsentService
{
    public static OperationDescriptor Confirm(
        OperationDescriptor operation,
        InstallerLaunchFinalConsentDecision decision,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(decision);
        if (!string.Equals(
                operation.Kind,
                InstallerLaunchOperationPlanner.OperationKind,
                StringComparison.Ordinal)
            || operation.ConfirmationAccepted
            || !decision.IsComplete)
        {
            throw new InvalidOperationException("All installer consent items are required.");
        }
        var arguments = operation.Arguments.ToDictionary(pair => pair.Key, pair => pair.Value);
        arguments["finalConsentUtc"] = now.ToUniversalTime().ToString("O");
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

public sealed class InstallerLaunchOperationHandler
{
    private static readonly HashSet<string> AllowedArgumentKeys =
        new(StringComparer.Ordinal)
        {
            "packagePath",
            "packageSha256",
            "packageLengthBytes",
            "packageLastWriteUtc",
            "installerKind",
            "installerKindConfidence",
            "targetInstallPath",
            "interactiveArguments",
            "snapshotEvidencePath",
            "snapshotEvidenceSha256",
            "finalConsentUtc"
        };

    private readonly IInstallerPackageInspector _packageInspector;
    private readonly IInstallBeforeSnapshotEvidenceReader _snapshotReader;
    private readonly IInstallerTargetPathPolicy _targetPathPolicy;
    private readonly IInteractiveInstallerProcessLauncher _launcher;
    private readonly Func<DateTimeOffset> _now;

    public InstallerLaunchOperationHandler(
        IInstallerPackageInspector packageInspector,
        IInstallBeforeSnapshotEvidenceReader snapshotReader,
        IInstallerTargetPathPolicy targetPathPolicy,
        IInteractiveInstallerProcessLauncher launcher,
        Func<DateTimeOffset>? now = null)
    {
        _packageInspector = packageInspector ?? throw new ArgumentNullException(nameof(packageInspector));
        _snapshotReader = snapshotReader ?? throw new ArgumentNullException(nameof(snapshotReader));
        _targetPathPolicy = targetPathPolicy ?? throw new ArgumentNullException(nameof(targetPathPolicy));
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        _now = now ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<OperationResult> ExecuteAsync(
        OperationDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        var request = ParseAndValidateDescriptor(descriptor);
        if (request.Error is not null)
            return OperationResult.Fail(request.Error);
        var now = _now().ToUniversalTime();
        var consentAge = now - request.FinalConsentUtc;
        if (consentAge > TimeSpan.FromMinutes(15) || consentAge < -TimeSpan.FromMinutes(1))
            return OperationResult.Fail("安装最终确认已过期，请重新查看并确认。");

        var snapshotResult = await _snapshotReader.ReadVerifiedAsync(
            request.SnapshotEvidencePath!,
            request.SnapshotEvidenceSha256!,
            cancellationToken);
        if (!snapshotResult.IsValid || snapshotResult.Evidence is null)
            return OperationResult.Fail("安装前快照证据无效，已拒绝打开安装包。");
        var snapshotError = InstallBeforeSnapshotEvidenceService.ValidateForOperation(
            snapshotResult.Evidence,
            descriptor,
            now);
        if (snapshotError is not null)
            return OperationResult.Fail("安装前快照已过期或与当前安装包不一致。");

        var current = _packageInspector.Inspect(request.PackagePath!);
        if (!CurrentPackageMatches(request, current))
            return OperationResult.Fail("安装包在确认后发生变化，已拒绝打开。");
        if (!_targetPathPolicy.IsAllowed(request.TargetInstallPath!, out _))
            return OperationResult.Fail("推荐安装位置不符合当前安全规则，已拒绝打开安装包。");

        var expectedArguments = ExpectedInteractiveArguments(
            current,
            request.TargetInstallPath!);
        if (!StringListsEqual(expectedArguments, request.InteractiveArguments!))
            return OperationResult.Fail("安装参数与已确认的安装器类型不一致，已拒绝打开。");

        var launch = await _launcher.LaunchAsync(
            new InteractiveInstallerLaunchRequest
            {
                PackagePath = current.PackagePath,
                ExpectedSha256 = current.Sha256!,
                Arguments = expectedArguments
            },
            cancellationToken);
        return launch.Status switch
        {
            InteractiveInstallerLaunchStatus.Started when launch.Session is not null =>
                OperationResult.Ok(
                    "安装界面已打开；这不代表安装已经成功。",
                    launch),
            InteractiveInstallerLaunchStatus.UserCanceled =>
                OperationResult.Fail("你取消了安装器的系统提示，没有开始安装。"),
            InteractiveInstallerLaunchStatus.Refused =>
                OperationResult.Fail("安装器启动边界拒绝了这次请求。"),
            _ => OperationResult.Fail("安装器没有成功打开，系统没有报告安装成功。")
        };
    }

    private static ParsedInstallerLaunchRequest ParseAndValidateDescriptor(
        OperationDescriptor descriptor)
    {
        if (!string.Equals(
                descriptor.Kind,
                InstallerLaunchOperationPlanner.OperationKind,
                StringComparison.Ordinal)
            || descriptor.Source != OperationSource.Manual
            || descriptor.Risk != RiskLevel.High
            || !descriptor.IsDestructive
            || descriptor.RequiresElevation
            || !descriptor.RequiresSnapshot
            || descriptor.RollbackRequired
            || !descriptor.ConfirmationAccepted
            || string.IsNullOrWhiteSpace(descriptor.SnapshotId)
            || string.IsNullOrWhiteSpace(descriptor.EvidenceSummary)
            || string.IsNullOrWhiteSpace(descriptor.ConfirmationText)
            || descriptor.Arguments.Keys.Any(key => !AllowedArgumentKeys.Contains(key)))
        {
            return ParsedInstallerLaunchRequest.Invalid("安装启动操作缺少必要的人工确认或安全证据。");
        }

        var packagePath = StringArgument(descriptor, "packagePath");
        var packageSha256 = StringArgument(descriptor, "packageSha256");
        var packageLengthText = StringArgument(descriptor, "packageLengthBytes");
        var packageLastWriteText = StringArgument(descriptor, "packageLastWriteUtc");
        var kindText = StringArgument(descriptor, "installerKind");
        var confidenceText = StringArgument(descriptor, "installerKindConfidence");
        var target = StringArgument(descriptor, "targetInstallPath");
        var snapshotPath = StringArgument(descriptor, "snapshotEvidencePath");
        var snapshotSha256 = StringArgument(descriptor, "snapshotEvidenceSha256");
        var finalConsentText = StringArgument(descriptor, "finalConsentUtc");
        var arguments = StringListArgument(descriptor, "interactiveArguments");
        if (!IsLocalPackagePath(packagePath)
            || !InstallBeforeSnapshotEvidenceService.IsSha256(packageSha256)
            || !long.TryParse(
                packageLengthText,
                System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture,
                out var packageLength)
            || packageLength <= 0
            || !DateTimeOffset.TryParseExact(
                packageLastWriteText,
                "O",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out var packageLastWriteUtc)
            || !Enum.TryParse<InstallerKind>(kindText, ignoreCase: false, out var kind)
            || !Enum.IsDefined(kind)
            || !Enum.TryParse<InstallerKindConfidence>(
                confidenceText,
                ignoreCase: false,
                out var confidence)
            || !Enum.IsDefined(confidence)
            || string.IsNullOrWhiteSpace(target)
            || string.IsNullOrWhiteSpace(snapshotPath)
            || !InstallBeforeSnapshotEvidenceService.IsSha256(snapshotSha256)
            || !DateTimeOffset.TryParseExact(
                finalConsentText,
                "O",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out var finalConsentUtc)
            || arguments is null
            || descriptor.AffectedPaths.Count != 1
            || !InstallBeforeSnapshotEvidenceService.PathsEqual(
                descriptor.AffectedPaths[0],
                target))
        {
            return ParsedInstallerLaunchRequest.Invalid("安装启动操作参数无效。");
        }

        return new ParsedInstallerLaunchRequest
        {
            PackagePath = Path.GetFullPath(packagePath!),
            PackageSha256 = packageSha256!.ToUpperInvariant(),
            PackageLengthBytes = packageLength,
            PackageLastWriteUtc = packageLastWriteUtc.ToUniversalTime(),
            Kind = kind,
            KindConfidence = confidence,
            TargetInstallPath = target,
            InteractiveArguments = arguments,
            SnapshotEvidencePath = snapshotPath,
            SnapshotEvidenceSha256 = snapshotSha256!.ToUpperInvariant(),
            FinalConsentUtc = finalConsentUtc.ToUniversalTime()
        };
    }

    private static bool CurrentPackageMatches(
        ParsedInstallerLaunchRequest request,
        InstallerPackageEvidence current) =>
        current.HasStableIdentity
        && current.SignatureStatus == AuthenticodeSignatureStatus.Trusted
        && InstallBeforeSnapshotEvidenceService.PathsEqual(
            request.PackagePath,
            current.PackagePath)
        && InstallBeforeSnapshotEvidenceService.HashesEqual(
            request.PackageSha256,
            current.Sha256)
        && request.PackageLengthBytes == current.LengthBytes
        && request.PackageLastWriteUtc == current.LastWriteUtc?.ToUniversalTime()
        && request.Kind == current.DetectedKind
        && request.KindConfidence == current.KindConfidence
        && current.DetectedKind is InstallerKind.Msi
            or InstallerKind.Exe
            or InstallerKind.InnoSetup
            or InstallerKind.Nsis
            or InstallerKind.Burn;

    private static IReadOnlyList<string> ExpectedInteractiveArguments(
        InstallerPackageEvidence package,
        string targetPath) =>
        package.KindConfidence == InstallerKindConfidence.High
            ? package.DetectedKind switch
            {
                InstallerKind.InnoSetup => [$"/DIR={targetPath}"],
                InstallerKind.Nsis => [$"/D={targetPath}"],
                _ => []
            }
            : [];

    private static bool StringListsEqual(
        IReadOnlyList<string> left,
        IReadOnlyList<string> right) =>
        left.Count == right.Count
        && left.Zip(right).All(pair =>
            string.Equals(pair.First, pair.Second, StringComparison.Ordinal));

    private static string? StringArgument(OperationDescriptor descriptor, string key) =>
        descriptor.Arguments.TryGetValue(key, out var value) ? value as string : null;

    private static IReadOnlyList<string>? StringListArgument(
        OperationDescriptor descriptor,
        string key)
    {
        if (!descriptor.Arguments.TryGetValue(key, out var value))
            return null;
        return value switch
        {
            string[] array => array,
            IReadOnlyList<string> list => list,
            _ => null
        };
    }

    private static bool IsLocalPackagePath(string? path)
    {
        if (!InstallerPackagePathPolicy.TryResolveFixedLocalPath(path, out var fullPath)
            || !InstallerPackagePathPolicy.IsExistingFileWithoutReparsePoints(fullPath))
            return false;
        var extension = Path.GetExtension(fullPath);
        return extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".msi", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ParsedInstallerLaunchRequest
    {
        public string? Error { get; init; }
        public string? PackagePath { get; init; }
        public string? PackageSha256 { get; init; }
        public long PackageLengthBytes { get; init; }
        public DateTimeOffset PackageLastWriteUtc { get; init; }
        public InstallerKind Kind { get; init; }
        public InstallerKindConfidence KindConfidence { get; init; }
        public string? TargetInstallPath { get; init; }
        public IReadOnlyList<string>? InteractiveArguments { get; init; }
        public string? SnapshotEvidencePath { get; init; }
        public string? SnapshotEvidenceSha256 { get; init; }
        public DateTimeOffset FinalConsentUtc { get; init; }

        public static ParsedInstallerLaunchRequest Invalid(string error) =>
            new() { Error = error };
    }
}

public sealed class WindowsInteractiveInstallerProcessLauncher
    : IInteractiveInstallerProcessLauncher
{
    private const int ErrorCancelled = 1223;

    public ValueTask<InteractiveInstallerLaunchResult> LaunchAsync(
        InteractiveInstallerLaunchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsAllowedRequest(request))
            return ValueTask.FromResult(Result(InteractiveInstallerLaunchStatus.Refused));

        try
        {
            using var packageLock = new FileStream(
                request.PackagePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                64 * 1024,
                FileOptions.SequentialScan);
            var actualBytes = SHA256.HashData(packageLock);
            var expectedBytes = Convert.FromHexString(request.ExpectedSha256);
            try
            {
                if (!CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes))
                    return ValueTask.FromResult(Result(InteractiveInstallerLaunchStatus.Refused));
            }
            finally
            {
                CryptographicOperations.ZeroMemory(actualBytes);
                CryptographicOperations.ZeroMemory(expectedBytes);
            }

            var start = new ProcessStartInfo
            {
                FileName = request.PackagePath,
                WorkingDirectory = Path.GetDirectoryName(request.PackagePath)!,
                UseShellExecute = true
            };
            foreach (var argument in request.Arguments)
                start.ArgumentList.Add(argument);
            var process = Process.Start(start);
            return ValueTask.FromResult(process is null
                ? Result(InteractiveInstallerLaunchStatus.Failed)
                : new InteractiveInstallerLaunchResult
                {
                    Status = InteractiveInstallerLaunchStatus.Started,
                    Session = new WindowsInteractiveInstallerProcessSession(process)
                });
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode == ErrorCancelled)
        {
            return ValueTask.FromResult(Result(InteractiveInstallerLaunchStatus.UserCanceled));
        }
        catch
        {
            return ValueTask.FromResult(Result(InteractiveInstallerLaunchStatus.Failed));
        }
    }

    private static bool IsAllowedRequest(InteractiveInstallerLaunchRequest request)
    {
        if (!InstallerPackagePathPolicy.TryResolveFixedLocalPath(
                request.PackagePath,
                out var fullPath)
            || !InstallerPackagePathPolicy.IsExistingFileWithoutReparsePoints(fullPath)
            || !string.Equals(fullPath, request.PackagePath, StringComparison.OrdinalIgnoreCase)
            || !InstallBeforeSnapshotEvidenceService.IsSha256(request.ExpectedSha256)
            || request.Arguments.Count > 1)
        {
            return false;
        }
        var extension = Path.GetExtension(request.PackagePath);
        if (!extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(".msi", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (extension.Equals(".msi", StringComparison.OrdinalIgnoreCase)
            && request.Arguments.Count != 0)
            return false;
        if (request.Arguments.Count == 0)
            return true;
        var argument = request.Arguments[0];
        return argument.Length is > 3 and <= 2048
            && !argument.Contains('\0')
            && !argument.Contains('\r')
            && !argument.Contains('\n')
            && (argument.StartsWith("/DIR=", StringComparison.Ordinal)
                || argument.StartsWith("/D=", StringComparison.Ordinal));
    }

    private static InteractiveInstallerLaunchResult Result(
        InteractiveInstallerLaunchStatus status) =>
        new() { Status = status };

    private sealed class WindowsInteractiveInstallerProcessSession(Process process)
        : IInteractiveInstallerProcessSession
    {
        public int? ExitCode
        {
            get
            {
                try
                {
                    return process.HasExited ? process.ExitCode : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public Task WaitForExitAsync(CancellationToken cancellationToken = default) =>
            process.WaitForExitAsync(cancellationToken);

        public void Dispose() => process.Dispose();
    }
}
