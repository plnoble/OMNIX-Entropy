using System.Text.Json;
using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Software;
using Css.Core.Timeline;
using Css.Core.Uninstall;
using Css.Snapshot.Uninstall;

namespace Css.Elevated.Uninstall;

public sealed class OfficialUninstallerLaunchRequest
{
    public required string ExecutablePath { get; init; }
    public required string Arguments { get; init; }
    public required bool RequiresElevation { get; init; }
}

public sealed class OfficialUninstallerLaunchResult
{
    public required bool Started { get; init; }
    public int? ExitCode { get; init; }
    public bool UserCancelled { get; init; }
    public string? Error { get; init; }

    public static OfficialUninstallerLaunchResult Completed(int exitCode) =>
        new() { Started = true, ExitCode = exitCode };

    public static OfficialUninstallerLaunchResult NotStarted(string error, bool userCancelled = false) =>
        new() { Started = false, Error = error, UserCancelled = userCancelled };
}

public interface IOfficialUninstallerLauncher
{
    Task<OfficialUninstallerLaunchResult> LaunchAsync(
        OfficialUninstallerLaunchRequest request,
        CancellationToken cancellationToken);
}

public interface IOfficialUninstallPostScanner
{
    Task<OfficialUninstallPostScanResult> ScanAsync(
        string softwareName,
        CancellationToken cancellationToken);
}

public sealed class OfficialUninstallOperationHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IOfficialUninstallerLauncher _launcher;
    private readonly Func<UninstallEvidenceSnapshotManifest, IOfficialUninstallPostScanner>
        _postScannerFactory;
    private readonly ActionTimelineStore _timeline;
    private readonly Func<string, bool> _fileExists;
    private readonly Func<string, string> _hashResolver;
    private readonly Func<DateTimeOffset> _clock;

    public OfficialUninstallOperationHandler(
        IOfficialUninstallerLauncher launcher,
        IOfficialUninstallPostScanner postScanner,
        ActionTimelineStore timeline,
        Func<string, bool> fileExists,
        Func<string, string> hashResolver,
        Func<DateTimeOffset>? clock = null)
        : this(
            launcher,
            FixedScannerFactory(postScanner),
            timeline,
            fileExists,
            hashResolver,
            clock)
    {
    }

    public OfficialUninstallOperationHandler(
        IOfficialUninstallerLauncher launcher,
        Func<UninstallEvidenceSnapshotManifest, IOfficialUninstallPostScanner>
            postScannerFactory,
        ActionTimelineStore timeline,
        Func<string, bool> fileExists,
        Func<string, string> hashResolver,
        Func<DateTimeOffset>? clock = null)
    {
        ArgumentNullException.ThrowIfNull(launcher);
        ArgumentNullException.ThrowIfNull(postScannerFactory);
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(fileExists);
        ArgumentNullException.ThrowIfNull(hashResolver);
        _launcher = launcher;
        _postScannerFactory = postScannerFactory;
        _timeline = timeline;
        _fileExists = fileExists;
        _hashResolver = hashResolver;
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<OperationResult> ExecuteAsync(
        OperationDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(descriptor, cancellationToken);
        if (!validation.Success)
            return OperationResult.Fail(validation.Error!);

        var context = validation.Context!;
        OfficialUninstallerLaunchResult launch;
        try
        {
            launch = await _launcher.LaunchAsync(new OfficialUninstallerLaunchRequest
            {
                ExecutablePath = context.ExecutablePath,
                Arguments = context.Arguments,
                RequiresElevation = false
            }, cancellationToken);
        }
        catch (Exception exception)
        {
            launch = OfficialUninstallerLaunchResult.NotStarted(exception.Message);
        }

        if (!launch.Started || launch.ExitCode is null)
        {
            var payload = Payload(
                started: launch.Started,
                completed: false,
                launch.ExitCode,
                OfficialUninstallPostScanResult.NotRun("\u5378\u8f7d\u5668\u672a\u5b8c\u6210\uff0c\u672a\u8fdb\u884c\u540e\u7eed\u626b\u63cf\u3002"),
                requiresPostScanRetry: false);
            await AddTimelineAsync(descriptor, context.SoftwareName,
                launch.UserCancelled
                    ? "\u7528\u6237\u53d6\u6d88\u4e86\u5b98\u65b9\u5378\u8f7d\u5668\u542f\u52a8\u3002"
                    : "\u5b98\u65b9\u5378\u8f7d\u5668\u672a\u80fd\u542f\u52a8\u3002",
                cancellationToken);
            return Failure(launch.Error ?? "Official uninstaller did not start.", payload);
        }

        OfficialUninstallPostScanResult postScan;
        try
        {
            var postScanner = _postScannerFactory(context.Manifest)
                ?? throw new InvalidOperationException("The post-scan factory returned no scanner.");
            postScan = await postScanner.ScanAsync(context.SoftwareName, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            postScan = OfficialUninstallPostScanResult.NotRun(exception.Message);
        }

        if (launch.ExitCode != 0)
        {
            var payload = Payload(
                started: true,
                completed: false,
                launch.ExitCode,
                postScan,
                requiresPostScanRetry: !postScan.Success);
            await AddTimelineAsync(
                descriptor,
                context.SoftwareName,
                postScan.Success
                    ? $"\u5b98\u65b9\u5378\u8f7d\u5668\u8fd4\u56de\u975e\u96f6\u9000\u51fa\u7801 {launch.ExitCode}\uff0c\u5df2\u5b8c\u6210\u53ea\u8bfb\u590d\u67e5\u3002"
                    : $"\u5b98\u65b9\u5378\u8f7d\u5668\u8fd4\u56de\u975e\u96f6\u9000\u51fa\u7801 {launch.ExitCode}\uff0c\u53ea\u8bfb\u590d\u67e5\u672a\u5b8c\u6210\u3002",
                cancellationToken);
            return Failure($"Official uninstaller exited with code {launch.ExitCode}.", payload);
        }

        var completedPayload = Payload(
            started: true,
            completed: true,
            launch.ExitCode,
            postScan,
            requiresPostScanRetry: !postScan.Success);
        await AddTimelineAsync(
            descriptor,
            context.SoftwareName,
            postScan.Success
                ? "\u5b98\u65b9\u5378\u8f7d\u5668\u5df2\u5b8c\u6210\uff0c\u5df2\u91cd\u65b0\u626b\u63cf\u3002"
                : "\u5b98\u65b9\u5378\u8f7d\u5668\u5df2\u5b8c\u6210\uff0c\u4f46\u540e\u7eed\u626b\u63cf\u5931\u8d25\uff0c\u9700\u8981\u91cd\u8bd5\u3002",
            cancellationToken);

        return postScan.Success
            ? OperationResult.Ok("\u5b98\u65b9\u5378\u8f7d\u5668\u5df2\u5b8c\u6210\uff0c\u5df2\u751f\u6210\u540e\u7eed\u626b\u63cf\u7ed3\u679c\u3002", completedPayload)
            : Failure("Official uninstaller completed, but mandatory post-scan failed.", completedPayload);
    }

    private async Task<ValidationResult> ValidateAsync(
        OperationDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(descriptor.Kind, "uninstall.official.run", StringComparison.Ordinal)
            || !descriptor.IsDestructive
            || descriptor.Risk != RiskLevel.High
            || !descriptor.RequiresElevation
            || !descriptor.RequiresSnapshot
            || !descriptor.RollbackRequired
            || !descriptor.ConfirmationAccepted)
        {
            return ValidationResult.Fail("Official uninstall descriptor safety flags are incomplete.");
        }

        if (!TryString(descriptor, "executablePath", out var executablePath)
            || !TryStringAllowEmpty(descriptor, "arguments", out var arguments)
            || !TryString(descriptor, "snapshotManifestPath", out var snapshotManifestPath)
            || !TryString(descriptor, "snapshotSha256", out var snapshotSha256)
            || !TryBoolean(descriptor, "snapshotCanRestoreApplication", out var canRestoreApplication)
            || !TryString(descriptor, "recoveryMethod", out var recoveryMethod)
            || !TryString(descriptor, "recoveryReference", out var recoveryReference))
        {
            return ValidationResult.Fail("Official uninstall descriptor is missing evidence arguments.");
        }

        if (canRestoreApplication)
            return ValidationResult.Fail("Evidence snapshot must not claim application rollback.");
        if (!SafeExists(snapshotManifestPath) || !SafeExists(executablePath))
            return ValidationResult.Fail("Snapshot manifest or official uninstaller file is missing.");

        var actualHash = ResolveHash(snapshotManifestPath);
        if (!string.Equals(actualHash, snapshotSha256, StringComparison.OrdinalIgnoreCase))
            return ValidationResult.Fail("\u5feb\u7167\u8bc1\u636e\u54c8\u5e0c\u4e0d\u5339\u914d\uff0c\u5df2\u963b\u6b62\u5378\u8f7d\u5668\u542f\u52a8\u3002");

        var manifest = await LoadManifestAsync(snapshotManifestPath, cancellationToken);
        if (manifest is null
            || manifest.SchemaVersion != 1
            || manifest.Purpose != "pre-uninstall-evidence"
            || manifest.CanRestoreApplication
            || !string.Equals(manifest.SnapshotId, descriptor.SnapshotId, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(manifest.SoftwareName))
        {
            return ValidationResult.Fail("Snapshot manifest is not valid for official uninstall.");
        }

        var age = _clock().ToUniversalTime() - manifest.CreatedAtUtc.ToUniversalTime();
        if (age > TimeSpan.FromHours(1) || age < TimeSpan.FromMinutes(-5))
            return ValidationResult.Fail("Snapshot manifest is stale or has an invalid timestamp.");

        if (!string.Equals(manifest.RecoveryMethod, recoveryMethod, StringComparison.Ordinal)
            || !string.Equals(manifest.RecoveryReference, recoveryReference, StringComparison.Ordinal)
            || (manifest.DataPaths.Count > 0 && !manifest.UserDataBackupConfirmed))
        {
            return ValidationResult.Fail("Recovery or personal-data backup evidence no longer matches the snapshot manifest.");
        }

        var parsed = ParseCommand(manifest.UninstallCommand);
        if (parsed is null
            || !PathsEqual(parsed.Value.ExecutablePath, executablePath)
            || !string.Equals(parsed.Value.Arguments, arguments, StringComparison.Ordinal))
        {
            return ValidationResult.Fail("Official uninstall command no longer matches the snapshot manifest.");
        }

        var trust = OfficialUninstallCommandTrustEvaluator.Evaluate(
            executablePath,
            manifest.InstallPath,
            arguments,
            manifest.Publisher,
            executableSignatureSubject: null);
        if (!trust.IsTrusted)
            return ValidationResult.Fail(trust.Summary);

        return ValidationResult.Ok(new HandlerContext(
            manifest.SoftwareName,
            executablePath,
            arguments,
            manifest));
    }

    private async Task AddTimelineAsync(
        OperationDescriptor descriptor,
        string softwareName,
        string summary,
        CancellationToken cancellationToken)
    {
        await _timeline.AddAsync(new ActionTimelineEntry
        {
            OccurredAt = _clock(),
            Source = descriptor.Source,
            Title = softwareName + " \u5b98\u65b9\u5378\u8f7d",
            EvidenceSummary = summary,
            AffectedPaths = descriptor.AffectedPaths,
            RestoreState = RestoreState.NotRestorable,
            RestoreOperationKind = null,
            RestoreManifestPaths = []
        }, cancellationToken);
    }

    private bool SafeExists(string path)
    {
        try
        {
            return _fileExists(path);
        }
        catch
        {
            return false;
        }
    }

    private string? ResolveHash(string path)
    {
        try
        {
            return _hashResolver(path);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<UninstallEvidenceSnapshotManifest?> LoadManifestAsync(
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<UninstallEvidenceSnapshotManifest>(
                stream,
                JsonOptions,
                cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private static bool TryString(
        OperationDescriptor descriptor,
        string key,
        out string value)
    {
        if (descriptor.Arguments.TryGetValue(key, out var raw)
            && raw is string text
            && !string.IsNullOrWhiteSpace(text))
        {
            value = text;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool TryBoolean(
        OperationDescriptor descriptor,
        string key,
        out bool value)
    {
        if (descriptor.Arguments.TryGetValue(key, out var raw) && raw is bool boolean)
        {
            value = boolean;
            return true;
        }

        value = false;
        return false;
    }

    private static bool TryStringAllowEmpty(
        OperationDescriptor descriptor,
        string key,
        out string value)
    {
        if (descriptor.Arguments.TryGetValue(key, out var raw) && raw is string text)
        {
            value = text;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool PathsEqual(string first, string second)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(first),
                Path.GetFullPath(second),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static ParsedCommand? ParseCommand(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return null;

        var trimmed = command.Trim();
        if (trimmed.StartsWith('"'))
        {
            var closing = trimmed.IndexOf('"', 1);
            if (closing > 1)
            {
                return new ParsedCommand(
                    trimmed[1..closing],
                    trimmed[(closing + 1)..].Trim());
            }
        }

        var firstSpace = trimmed.IndexOf(' ');
        return firstSpace < 0
            ? new ParsedCommand(trimmed, string.Empty)
            : new ParsedCommand(trimmed[..firstSpace], trimmed[(firstSpace + 1)..].Trim());
    }

    private static OfficialUninstallHandlerPayload Payload(
        bool started,
        bool completed,
        int? exitCode,
        OfficialUninstallPostScanResult postScan,
        bool requiresPostScanRetry) =>
        new()
        {
            UninstallerStarted = started,
            UninstallerCompleted = completed,
            ExitCode = exitCode,
            PostScan = postScan,
            RequiresPostScanRetry = requiresPostScanRetry
        };

    private static OperationResult Failure(
        string error,
        OfficialUninstallHandlerPayload payload) =>
        new() { Success = false, Error = error, Payload = payload };

    private sealed record HandlerContext(
        string SoftwareName,
        string ExecutablePath,
        string Arguments,
        UninstallEvidenceSnapshotManifest Manifest);

    private static Func<UninstallEvidenceSnapshotManifest, IOfficialUninstallPostScanner>
        FixedScannerFactory(IOfficialUninstallPostScanner postScanner)
    {
        ArgumentNullException.ThrowIfNull(postScanner);
        return _ => postScanner;
    }

    private sealed class ValidationResult
    {
        public required bool Success { get; init; }
        public string? Error { get; init; }
        public HandlerContext? Context { get; init; }

        public static ValidationResult Fail(string error) =>
            new() { Success = false, Error = error };

        public static ValidationResult Ok(HandlerContext context) =>
            new() { Success = true, Context = context };
    }

    private readonly record struct ParsedCommand(
        string ExecutablePath,
        string Arguments);
}
