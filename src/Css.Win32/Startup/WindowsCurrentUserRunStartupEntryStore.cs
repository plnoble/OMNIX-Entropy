using System.Security.AccessControl;
using System.Security.Cryptography;
using Css.Core.Software;
using Css.Core.Startup;
using Microsoft.Win32;

namespace Css.Win32.Startup;

public sealed record WindowsCurrentUserRunRegistrySnapshot
{
    public required bool Readable { get; init; }
    public required bool Exists { get; init; }
    public required string ValueName { get; init; }
    public StartupRegistryValueKind ValueKind { get; init; }
    public string? ValueData { get; init; }
    public string? KeyAclSha256 { get; init; }
    public StartupApprovalObservation? StartupApproval { get; init; }
    public required DateTimeOffset CapturedAtUtc { get; init; }
    public string? Error { get; init; }

    public static WindowsCurrentUserRunRegistrySnapshot Missing(
        string valueName,
        DateTimeOffset capturedAtUtc) =>
        new()
        {
            Readable = true,
            Exists = false,
            ValueName = valueName,
            CapturedAtUtc = capturedAtUtc.ToUniversalTime()
        };

    public static WindowsCurrentUserRunRegistrySnapshot Unreadable(
        string valueName,
        DateTimeOffset capturedAtUtc,
        string error) =>
        new()
        {
            Readable = false,
            Exists = false,
            ValueName = valueName,
            CapturedAtUtc = capturedAtUtc.ToUniversalTime(),
            Error = error
        };
}

public interface IWindowsCurrentUserRunRegistryBackend
{
    WindowsCurrentUserRunRegistrySnapshot Capture(string valueName);
    StartupEntryMutationResult DisableExact(StartupEntryState expected);
    StartupEntryMutationResult RestoreExact(StartupEntryState expected);
}

public sealed class WindowsCurrentUserRunStartupEntryStore : IStartupEntryControlStore
{
    private readonly IWindowsCurrentUserRunRegistryBackend _backend;

    public WindowsCurrentUserRunStartupEntryStore()
        : this(new SystemWindowsCurrentUserRunRegistryBackend())
    {
    }

    public WindowsCurrentUserRunStartupEntryStore(IWindowsCurrentUserRunRegistryBackend backend)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
    }

    public Task<StartupEntryCaptureResult> CaptureAsync(
        BackgroundComponentObservation observation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!StartupEntryControlPolicy.IsSupportedObservation(observation))
            return Task.FromResult(StartupEntryCaptureResult.Refused("Startup observation is outside the supported scope."));

        WindowsCurrentUserRunRegistrySnapshot current;
        try
        {
            current = _backend.Capture(observation.Identity.DisplayName);
        }
        catch
        {
            return Task.FromResult(StartupEntryCaptureResult.Refused("Startup state could not be read."));
        }
        if (!current.Readable
            || !current.Exists
            || current.ValueData is null
            || current.KeyAclSha256 is null
            || current.StartupApproval is null)
        {
            return Task.FromResult(StartupEntryCaptureResult.Refused("Startup recovery evidence is incomplete."));
        }

        BackgroundComponentObservation fresh;
        try
        {
            fresh = BackgroundComponentObservationFactory.Startup(
                current.ValueName,
                StartupEntryControlPolicy.SupportedSourceLocator,
                current.ValueData,
                current.CapturedAtUtc,
                current.StartupApproval);
        }
        catch
        {
            return Task.FromResult(StartupEntryCaptureResult.Refused("Startup state is invalid."));
        }
        if (!string.Equals(fresh.Identity.StableId, observation.Identity.StableId, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(fresh.ObservationFingerprint, observation.ObservationFingerprint, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(StartupEntryCaptureResult.Refused("Startup state changed after the application scan."));
        }

        try
        {
            var state = StartupEntryStateFactory.Create(
                fresh,
                current.ValueKind,
                current.ValueData,
                current.KeyAclSha256,
                current.CapturedAtUtc);
            return Task.FromResult(StartupEntryCaptureResult.Completed(state));
        }
        catch
        {
            return Task.FromResult(StartupEntryCaptureResult.Refused("Startup recovery evidence is invalid."));
        }
    }

    public Task<StartupEntryMutationResult> DisableAsync(
        StartupEntryState expected,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!StartupEntryStateFactory.Verify(expected))
            return Task.FromResult(StartupEntryMutationResult.Refused("Startup snapshot is invalid."));
        return Task.FromResult(_backend.DisableExact(expected));
    }

    public Task<StartupEntryMutationResult> RestoreAsync(
        StartupEntryState expected,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!StartupEntryStateFactory.Verify(expected))
            return Task.FromResult(StartupEntryMutationResult.Refused("Startup snapshot is invalid."));
        return Task.FromResult(_backend.RestoreExact(expected));
    }
}

public sealed class SystemWindowsCurrentUserRunRegistryBackend : IWindowsCurrentUserRunRegistryBackend
{
    private const string RunSubKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ApprovalSubKey =
        @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
    private readonly Func<DateTimeOffset> _clock;

    public SystemWindowsCurrentUserRunRegistryBackend(Func<DateTimeOffset>? clock = null)
    {
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public WindowsCurrentUserRunRegistrySnapshot Capture(string valueName)
    {
        if (!StartupEntryControlPolicy.IsSafeValueName(valueName))
            return WindowsCurrentUserRunRegistrySnapshot.Unreadable(
                valueName,
                _clock(),
                "Value name is invalid.");
        try
        {
            using var root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            using var runKey = root.OpenSubKey(RunSubKey, writable: false);
            return runKey is null
                ? WindowsCurrentUserRunRegistrySnapshot.Missing(valueName, _clock())
                : CaptureFromOpenKey(root, runKey, valueName, _clock());
        }
        catch
        {
            return WindowsCurrentUserRunRegistrySnapshot.Unreadable(
                valueName,
                _clock(),
                "Current-user Run state could not be read.");
        }
    }

    public StartupEntryMutationResult DisableExact(StartupEntryState expected)
    {
        if (!StartupEntryStateFactory.Verify(expected)
            || !StartupEntryControlPolicy.IsSupportedLocator(expected.SourceLocator))
        {
            return StartupEntryMutationResult.Refused("Startup snapshot is invalid or unsupported.");
        }
        try
        {
            using var root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            using var runKey = root.OpenSubKey(RunSubKey, writable: true);
            if (runKey is null)
                return StartupEntryMutationResult.Refused("The current-user Run key no longer exists.");

            var current = CaptureFromOpenKey(root, runKey, expected.ValueName, _clock());
            if (!MatchesExpected(current, expected))
                return StartupEntryMutationResult.Refused("The startup entry changed; rescan before trying again.");

            runKey.DeleteValue(expected.ValueName, throwOnMissingValue: true);
            runKey.Flush();
            if (ContainsValue(runKey, expected.ValueName))
                return StartupEntryMutationResult.Refused("The startup entry was not removed.");
            return StartupEntryMutationResult.Completed("The exact current-user Run value was removed.");
        }
        catch (UnauthorizedAccessException)
        {
            return StartupEntryMutationResult.Refused("Windows denied access to this current-user startup entry.");
        }
        catch
        {
            return StartupEntryMutationResult.Refused("The startup entry could not be changed.");
        }
    }

    public StartupEntryMutationResult RestoreExact(StartupEntryState expected)
    {
        if (!StartupEntryStateFactory.Verify(expected)
            || !StartupEntryControlPolicy.IsSupportedLocator(expected.SourceLocator))
        {
            return StartupEntryMutationResult.Refused("Startup restore snapshot is invalid or unsupported.");
        }
        try
        {
            using var root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            using var runKey = root.OpenSubKey(RunSubKey, writable: true);
            if (runKey is null)
                return StartupEntryMutationResult.Refused("The current-user Run key no longer exists.");
            if (ContainsValue(runKey, expected.ValueName))
                return StartupEntryMutationResult.Refused("A startup value with the same name already exists; it was not overwritten.");

            var aclSha256 = FingerprintAcl(runKey);
            var approval = ReadStartupApproval(root, expected.ValueName);
            if (!string.Equals(aclSha256, expected.KeyAclSha256, StringComparison.OrdinalIgnoreCase)
                || !SameApproval(approval, expected.StartupApproval))
            {
                return StartupEntryMutationResult.Refused("Startup permissions or approval evidence changed; automatic restore was refused.");
            }

            runKey.SetValue(
                expected.ValueName,
                expected.ValueData,
                expected.ValueKind == StartupRegistryValueKind.ExpandString
                    ? RegistryValueKind.ExpandString
                    : RegistryValueKind.String);
            runKey.Flush();
            var restored = CaptureFromOpenKey(root, runKey, expected.ValueName, _clock());
            return MatchesExpected(restored, expected)
                ? StartupEntryMutationResult.Completed("The original current-user Run value was restored.")
                : StartupEntryMutationResult.Refused("The restored startup value did not match its snapshot.");
        }
        catch (UnauthorizedAccessException)
        {
            return StartupEntryMutationResult.Refused("Windows denied access while restoring this startup entry.");
        }
        catch
        {
            return StartupEntryMutationResult.Refused("The startup entry could not be restored.");
        }
    }

    private static WindowsCurrentUserRunRegistrySnapshot CaptureFromOpenKey(
        RegistryKey root,
        RegistryKey runKey,
        string valueName,
        DateTimeOffset capturedAtUtc)
    {
        if (!ContainsValue(runKey, valueName))
            return WindowsCurrentUserRunRegistrySnapshot.Missing(valueName, capturedAtUtc);

        var value = runKey.GetValue(
            valueName,
            null,
            RegistryValueOptions.DoNotExpandEnvironmentNames);
        if (value is not string valueData)
        {
            return WindowsCurrentUserRunRegistrySnapshot.Unreadable(
                valueName,
                capturedAtUtc,
                "Only string Run values are supported.");
        }
        var kind = runKey.GetValueKind(valueName) switch
        {
            RegistryValueKind.String => StartupRegistryValueKind.String,
            RegistryValueKind.ExpandString => StartupRegistryValueKind.ExpandString,
            _ => (StartupRegistryValueKind?)null
        };
        if (kind is null)
        {
            return WindowsCurrentUserRunRegistrySnapshot.Unreadable(
                valueName,
                capturedAtUtc,
                "The Run value type is unsupported.");
        }

        return new WindowsCurrentUserRunRegistrySnapshot
        {
            Readable = true,
            Exists = true,
            ValueName = valueName,
            ValueKind = kind.Value,
            ValueData = valueData,
            KeyAclSha256 = FingerprintAcl(runKey),
            StartupApproval = ReadStartupApproval(root, valueName),
            CapturedAtUtc = capturedAtUtc.ToUniversalTime()
        };
    }

    private static StartupApprovalObservation ReadStartupApproval(
        RegistryKey root,
        string valueName)
    {
        try
        {
            using var approvalKey = root.OpenSubKey(ApprovalSubKey, writable: false);
            var value = approvalKey?.GetValue(
                valueName,
                null,
                RegistryValueOptions.DoNotExpandEnvironmentNames);
            return StartupApprovalObservationFactory.FromRegistryValue(
                StartupEntryControlPolicy.SupportedApprovalLocator,
                valueName,
                value);
        }
        catch
        {
            return StartupApprovalObservationFactory.Unreadable(
                StartupEntryControlPolicy.SupportedApprovalLocator,
                valueName);
        }
    }

    private static string FingerprintAcl(RegistryKey key)
    {
        var security = key.GetAccessControl(
            AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
        return Convert.ToHexString(SHA256.HashData(security.GetSecurityDescriptorBinaryForm()));
    }

    private static bool MatchesExpected(
        WindowsCurrentUserRunRegistrySnapshot current,
        StartupEntryState expected)
    {
        if (!current.Readable
            || !current.Exists
            || current.ValueData is null
            || current.KeyAclSha256 is null
            || current.StartupApproval is null
            || current.ValueKind != expected.ValueKind
            || !string.Equals(current.ValueName, expected.ValueName, StringComparison.Ordinal)
            || !string.Equals(current.ValueData, expected.ValueData, StringComparison.Ordinal)
            || !string.Equals(current.KeyAclSha256, expected.KeyAclSha256, StringComparison.OrdinalIgnoreCase)
            || !SameApproval(current.StartupApproval, expected.StartupApproval))
        {
            return false;
        }

        var observation = BackgroundComponentObservationFactory.Startup(
            current.ValueName,
            StartupEntryControlPolicy.SupportedSourceLocator,
            current.ValueData,
            current.CapturedAtUtc,
            current.StartupApproval);
        return string.Equals(
                   observation.Identity.StableId,
                   expected.ObservationStableId,
                   StringComparison.OrdinalIgnoreCase)
               && string.Equals(
                   observation.ObservationFingerprint,
                   expected.ObservationFingerprint,
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool SameApproval(
        StartupApprovalObservation? current,
        StartupApprovalObservation? expected) =>
        current is not null
        && expected is not null
        && string.Equals(current.ApprovalKeyLocator, expected.ApprovalKeyLocator, StringComparison.OrdinalIgnoreCase)
        && string.Equals(current.ValueName, expected.ValueName, StringComparison.Ordinal)
        && current.Status == expected.Status
        && current.PayloadLength == expected.PayloadLength
        && string.Equals(current.PayloadFingerprint, expected.PayloadFingerprint, StringComparison.OrdinalIgnoreCase);

    private static bool ContainsValue(RegistryKey key, string valueName) =>
        key.GetValueNames().Contains(valueName, StringComparer.Ordinal);
}
