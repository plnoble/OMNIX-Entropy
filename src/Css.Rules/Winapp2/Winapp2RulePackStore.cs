using System.Text.Json;

namespace Css.Rules.Winapp2;

public sealed record Winapp2RulePackActivationConsent
{
    public bool UserConfirmedActivation { get; init; }
    public bool UserAcceptedLicense { get; init; }
    public required Uri ReviewedSourceUri { get; init; }
    public required Uri ReviewedLicenseUri { get; init; }
    public required string ReviewedVersion { get; init; }
    public required string ReviewedSha256 { get; init; }
}

public sealed record Winapp2RulePackRollbackConsent
{
    public bool UserConfirmedRollback { get; init; }
    public required string ExpectedActiveSha256 { get; init; }
    public required string ExpectedPreviousSha256 { get; init; }
}

public static class Winapp2RulePackConsentPolicy
{
    public static void ValidateActivation(
        Winapp2RulePackDescriptor descriptor,
        Winapp2RulePackActivationConsent consent)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(consent);
        Winapp2RulePackDescriptorPolicy.Validate(descriptor);
        if (!consent.UserConfirmedActivation || !consent.UserAcceptedLicense)
            throw new InvalidOperationException("Rule-pack activation requires source and license confirmation.");
        if (consent.ReviewedSourceUri != descriptor.SourceUri
            || consent.ReviewedLicenseUri != descriptor.LicenseUri
            || !string.Equals(consent.ReviewedVersion, descriptor.Version, StringComparison.Ordinal)
            || !HashesEqual(consent.ReviewedSha256, descriptor.ExpectedSha256))
        {
            throw new InvalidOperationException("The rule-pack reviewed metadata no longer matches the activation request.");
        }
    }

    private static bool HashesEqual(string? left, string? right) =>
        IsSha256(left)
        && IsSha256(right)
        && string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public sealed class Winapp2RulePackStatus
{
    public required Winapp2RulePackDescriptor ActiveDescriptor { get; init; }
    public Winapp2RulePackDescriptor? PreviousDescriptor { get; init; }
    public required DateTimeOffset ActivatedAtUtc { get; init; }
    public required string StatePath { get; init; }
    public required string ActivePackPath { get; init; }
    public string? PreviousPackPath { get; init; }
    public bool CanRollback => PreviousDescriptor is not null;
    public bool IsExecutionAuthorized => false;
}

public sealed class Winapp2RulePackActivationReceipt
{
    public required string ActiveVersion { get; init; }
    public string? PreviousVersion { get; init; }
    public required string ActivePackPath { get; init; }
    public required string StatePath { get; init; }
    public required DateTimeOffset ActivatedAtUtc { get; init; }
    public bool IsExecutionAuthorized => false;
}

public sealed class Winapp2RulePackStore
{
    private const int StateSchemaVersion = 1;
    private const int MaximumStateBytes = 64 * 1024;
    private const int CopyBufferBytes = 81920;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string _root;
    private readonly string _packsRoot;
    private readonly string _statePath;
    private readonly TimeProvider _timeProvider;
    private readonly Winapp2RuleCatalogLoader _loader;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public Winapp2RulePackStore(
        string root,
        TimeProvider? timeProvider = null,
        Winapp2RuleCatalogLoader? loader = null)
    {
        if (string.IsNullOrWhiteSpace(root) || !Path.IsPathFullyQualified(root))
            throw new ArgumentException("Rule-pack root must be an absolute path.", nameof(root));

        _root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        if (IsVolumeRoot(_root))
            throw new ArgumentException("Rule-pack root cannot be a volume root.", nameof(root));

        _packsRoot = Path.Combine(_root, "packs");
        _statePath = Path.Combine(_root, "active-state.json");
        _timeProvider = timeProvider ?? TimeProvider.System;
        _loader = loader ?? new Winapp2RuleCatalogLoader();
    }

    public async Task<Winapp2RulePackActivationReceipt> ActivateAsync(
        Stream content,
        Winapp2RulePackDescriptor descriptor,
        Winapp2RulePackActivationConsent consent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(consent);
        Winapp2RulePackConsentPolicy.ValidateActivation(descriptor, consent);
        cancellationToken.ThrowIfCancellationRequested();

        await _gate.WaitAsync(cancellationToken);
        try
        {
            EnsureManagedDirectories();
            var stagingPath = Path.Combine(_packsRoot, "pack.ini.tmp-" + Guid.NewGuid().ToString("N"));
            try
            {
                await CopyBoundedAsync(content, stagingPath, cancellationToken);
                var catalog = _loader.Load(stagingPath, descriptor);
                cancellationToken.ThrowIfCancellationRequested();

                var activePackPath = PackPath(descriptor.ExpectedSha256);
                if (File.Exists(activePackPath))
                {
                    EnsureRegularFile(activePackPath);
                    _loader.Load(activePackPath, descriptor);
                }
                else
                {
                    File.Move(stagingPath, activePackPath, overwrite: false);
                    stagingPath = string.Empty;
                }

                var current = ReadStateOrNull();
                var sameContent = current is not null
                    && HashesEqual(current.ActiveDescriptor.ExpectedSha256, descriptor.ExpectedSha256);
                var next = new StoredState
                {
                    SchemaVersion = StateSchemaVersion,
                    ActiveDescriptor = descriptor,
                    PreviousDescriptor = sameContent ? current!.PreviousDescriptor : current?.ActiveDescriptor,
                    ActivatedAtUtc = _timeProvider.GetUtcNow().ToUniversalTime()
                };
                await WriteStateAtomicallyAsync(next, cancellationToken);
                return Receipt(Status(next));
            }
            finally
            {
                if (!string.IsNullOrEmpty(stagingPath) && File.Exists(stagingPath))
                    File.Delete(stagingPath);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Winapp2RulePackActivationReceipt> RollbackAsync(
        Winapp2RulePackRollbackConsent consent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(consent);
        if (!consent.UserConfirmedRollback)
            throw new InvalidOperationException("Rule-pack rollback requires explicit user confirmation.");
        cancellationToken.ThrowIfCancellationRequested();

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var current = ReadStateOrNull()
                ?? throw new InvalidOperationException("No active rule pack is available to roll back.");
            if (current.PreviousDescriptor is null)
                throw new InvalidOperationException("No previous rule pack is available to roll back to.");
            if (!HashesEqual(current.ActiveDescriptor.ExpectedSha256, consent.ExpectedActiveSha256)
                || !HashesEqual(current.PreviousDescriptor.ExpectedSha256, consent.ExpectedPreviousSha256))
            {
                throw new InvalidOperationException("The active rule-pack state changed since review; review it again before rollback.");
            }

            var previousPath = PackPath(current.PreviousDescriptor.ExpectedSha256);
            EnsureRegularFile(previousPath);
            _loader.Load(previousPath, current.PreviousDescriptor);
            cancellationToken.ThrowIfCancellationRequested();

            var next = new StoredState
            {
                SchemaVersion = StateSchemaVersion,
                ActiveDescriptor = current.PreviousDescriptor,
                PreviousDescriptor = current.ActiveDescriptor,
                ActivatedAtUtc = _timeProvider.GetUtcNow().ToUniversalTime()
            };
            await WriteStateAtomicallyAsync(next, cancellationToken);
            return Receipt(Status(next));
        }
        finally
        {
            _gate.Release();
        }
    }

    public Winapp2RulePackStatus? GetStatus()
    {
        var state = ReadStateOrNull();
        return state is null ? null : Status(state);
    }

    public Winapp2RuleCatalog LoadActiveCatalog()
    {
        var state = ReadStateOrNull()
            ?? throw new InvalidOperationException("No active rule pack is available.");
        var path = PackPath(state.ActiveDescriptor.ExpectedSha256);
        EnsureRegularFile(path);
        return _loader.Load(path, state.ActiveDescriptor);
    }

    private async Task CopyBoundedAsync(
        Stream source,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        if (!source.CanRead)
            throw new ArgumentException("Rule-pack stream must be readable.", nameof(source));

        await using var destination = new FileStream(
            destinationPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            CopyBufferBytes,
            FileOptions.Asynchronous | FileOptions.WriteThrough);
        var buffer = new byte[CopyBufferBytes];
        var total = 0;
        while (true)
        {
            var read = await source.ReadAsync(buffer, cancellationToken);
            if (read == 0)
                break;
            if (total > Winapp2RuleCatalogLoader.MaxPackBytes - read)
                throw new InvalidDataException($"The rule pack exceeds the {Winapp2RuleCatalogLoader.MaxPackBytes}-byte size limit.");

            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            total += read;
        }

        await destination.FlushAsync(cancellationToken);
    }

    private StoredState? ReadStateOrNull()
    {
        if (!File.Exists(_statePath))
            return null;

        EnsureSafeExistingRoot();
        EnsureRegularFile(_statePath);
        var info = new FileInfo(_statePath);
        if (info.Length is <= 0 or > MaximumStateBytes)
            throw new InvalidDataException("The rule-pack state size is invalid.");

        StoredState? state;
        try
        {
            state = JsonSerializer.Deserialize<StoredState>(File.ReadAllBytes(_statePath), JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("The rule-pack state is malformed.", exception);
        }

        if (state is null || state.SchemaVersion != StateSchemaVersion)
            throw new InvalidDataException("The rule-pack state schema is invalid.");
        ValidateDescriptorShape(state.ActiveDescriptor);
        if (state.PreviousDescriptor is not null)
            ValidateDescriptorShape(state.PreviousDescriptor);
        return state;
    }

    private async Task WriteStateAtomicallyAsync(StoredState state, CancellationToken cancellationToken)
    {
        var temporaryPath = _statePath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await using (var stream = new FileStream(
                             temporaryPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             4096,
                             FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            if (new FileInfo(temporaryPath).Length > MaximumStateBytes)
                throw new InvalidDataException("The rule-pack state exceeds its size limit.");
            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(_statePath))
                File.Replace(temporaryPath, _statePath, destinationBackupFileName: null, ignoreMetadataErrors: true);
            else
                File.Move(temporaryPath, _statePath, overwrite: false);
            temporaryPath = string.Empty;
        }
        finally
        {
            if (!string.IsNullOrEmpty(temporaryPath) && File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private Winapp2RulePackStatus Status(StoredState state)
    {
        var activePath = PackPath(state.ActiveDescriptor.ExpectedSha256);
        EnsureRegularFile(activePath);
        var previousPath = state.PreviousDescriptor is null
            ? null
            : PackPath(state.PreviousDescriptor.ExpectedSha256);
        if (previousPath is not null)
            EnsureRegularFile(previousPath);

        return new Winapp2RulePackStatus
        {
            ActiveDescriptor = state.ActiveDescriptor,
            PreviousDescriptor = state.PreviousDescriptor,
            ActivatedAtUtc = state.ActivatedAtUtc,
            StatePath = _statePath,
            ActivePackPath = activePath,
            PreviousPackPath = previousPath
        };
    }

    private static Winapp2RulePackActivationReceipt Receipt(Winapp2RulePackStatus status) =>
        new()
        {
            ActiveVersion = status.ActiveDescriptor.Version,
            PreviousVersion = status.PreviousDescriptor?.Version,
            ActivePackPath = status.ActivePackPath,
            StatePath = status.StatePath,
            ActivatedAtUtc = status.ActivatedAtUtc
        };

    private void EnsureManagedDirectories()
    {
        Directory.CreateDirectory(_root);
        EnsureRegularDirectory(_root);
        Directory.CreateDirectory(_packsRoot);
        EnsureRegularDirectory(_packsRoot);
    }

    private void EnsureSafeExistingRoot()
    {
        EnsureRegularDirectory(_root);
        EnsureRegularDirectory(_packsRoot);
    }

    private static void EnsureRegularDirectory(string path)
    {
        if (!Directory.Exists(path) || (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException("The managed rule-pack directory is missing or redirected.");
    }

    private static void EnsureRegularFile(string path)
    {
        if (!File.Exists(path) || (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException("A managed rule-pack file is missing or redirected.");
    }

    private string PackPath(string sha256)
    {
        if (!IsSha256(sha256))
            throw new InvalidDataException("A stored rule-pack SHA-256 is invalid.");
        return Path.Combine(_packsRoot, sha256.ToUpperInvariant() + ".ini");
    }

    private static void ValidateDescriptorShape(Winapp2RulePackDescriptor descriptor)
    {
        try
        {
            Winapp2RulePackDescriptorPolicy.Validate(descriptor);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidDataException("The stored rule-pack descriptor is invalid.", exception);
        }
    }

    private static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);

    private static bool HashesEqual(string? left, string? right) =>
        IsSha256(left)
        && IsSha256(right)
        && string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static bool IsVolumeRoot(string path)
    {
        var root = Path.GetPathRoot(path);
        return !string.IsNullOrWhiteSpace(root)
            && Path.TrimEndingDirectorySeparator(root).Equals(path, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class StoredState
    {
        public int SchemaVersion { get; init; }
        public required Winapp2RulePackDescriptor ActiveDescriptor { get; init; }
        public Winapp2RulePackDescriptor? PreviousDescriptor { get; init; }
        public DateTimeOffset ActivatedAtUtc { get; init; }
    }
}
