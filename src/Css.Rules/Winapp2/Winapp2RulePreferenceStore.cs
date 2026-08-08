using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Css.Core.Software;

namespace Css.Rules.Winapp2;

public static class Winapp2RulePreferenceKey
{
    public static string Create(string rulePackSha256, string ruleName)
    {
        if (!IsValid(rulePackSha256))
            throw new ArgumentException("Rule-pack SHA-256 is invalid.", nameof(rulePackSha256));
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleName);
        var identity = rulePackSha256.ToUpperInvariant() + "\n" + ruleName.Trim();
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }

    public static string Create(CommunityRuleCacheEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        return Create(evidence.RulePackSha256, evidence.RuleName);
    }

    public static bool IsValid(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public sealed class Winapp2IgnoredRule
{
    public required string RuleKey { get; init; }
    public required string RuleName { get; init; }
    public required string RulePackSource { get; init; }
    public required string RulePackVersion { get; init; }
    public required string RulePackSha256 { get; init; }
    public required DateTimeOffset AddedAtUtc { get; init; }
}

public sealed class Winapp2RulePreferences
{
    public static Winapp2RulePreferences Empty { get; } = new() { IgnoredRules = [] };

    public required IReadOnlyList<Winapp2IgnoredRule> IgnoredRules { get; init; }
    public bool IsExecutionAuthorized => false;

    public bool IsIgnored(CommunityRuleCacheEvidence evidence) =>
        IsIgnored(Winapp2RulePreferenceKey.Create(evidence));

    public bool IsIgnored(string ruleKey) =>
        IgnoredRules.Any(item => item.RuleKey.Equals(ruleKey, StringComparison.OrdinalIgnoreCase));

    public IReadOnlySet<string> IgnoredRuleKeys() =>
        IgnoredRules
            .Select(item => item.RuleKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
}

public sealed class Winapp2RulePreferenceStore
{
    public const int MaximumFileBytes = 128 * 1024;
    public const int MaximumIgnoredRules = 1_000;
    private const int SchemaVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string _path;
    private readonly string _root;
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public Winapp2RulePreferenceStore(string path, TimeProvider? timeProvider = null)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            throw new ArgumentException("Rule-preference path must be absolute.", nameof(path));
        _path = Path.GetFullPath(path);
        _root = Path.GetDirectoryName(_path)
            ?? throw new ArgumentException("Rule-preference path has no parent directory.", nameof(path));
        if (Path.TrimEndingDirectorySeparator(_root).Equals(
                Path.TrimEndingDirectorySeparator(Path.GetPathRoot(_root) ?? string.Empty),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Rule preferences cannot be stored at a volume root.", nameof(path));
        }
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public Winapp2RulePreferences Load()
    {
        if (!File.Exists(_path))
            return Winapp2RulePreferences.Empty;
        EnsureRegularDirectory(_root);
        EnsureRegularFile(_path);
        var info = new FileInfo(_path);
        if (info.Length is <= 0 or > MaximumFileBytes)
            throw new InvalidDataException("The rule-preference file size is invalid.");

        StoredDocument? document;
        try
        {
            document = JsonSerializer.Deserialize<StoredDocument>(File.ReadAllBytes(_path), JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("The rule-preference file is malformed.", exception);
        }

        if (document is null || document.SchemaVersion != SchemaVersion)
            throw new InvalidDataException("The rule-preference schema is invalid.");
        Validate(document.IgnoredRules);
        return new Winapp2RulePreferences
        {
            IgnoredRules = document.IgnoredRules
                .OrderByDescending(item => item.AddedAtUtc)
                .ToArray()
        };
    }

    public async Task<Winapp2RulePreferences> IgnoreAsync(
        CommunityRuleCacheEvidence evidence,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        cancellationToken.ThrowIfCancellationRequested();
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var current = Load();
            var key = Winapp2RulePreferenceKey.Create(evidence);
            var items = current.IgnoredRules
                .Where(item => !item.RuleKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (items.Count >= MaximumIgnoredRules)
                throw new InvalidOperationException("The ignored-rule limit has been reached.");
            items.Add(new Winapp2IgnoredRule
            {
                RuleKey = key,
                RuleName = evidence.RuleName,
                RulePackSource = evidence.RulePackSource,
                RulePackVersion = evidence.RulePackVersion,
                RulePackSha256 = evidence.RulePackSha256.ToUpperInvariant(),
                AddedAtUtc = _timeProvider.GetUtcNow().ToUniversalTime()
            });
            return await WriteAsync(items, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Winapp2RulePreferences> RestoreAsync(
        string ruleKey,
        CancellationToken cancellationToken = default)
    {
        if (!Winapp2RulePreferenceKey.IsValid(ruleKey))
            throw new ArgumentException("Ignored-rule key is invalid.", nameof(ruleKey));
        cancellationToken.ThrowIfCancellationRequested();
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var current = Load();
            var items = current.IgnoredRules
                .Where(item => !item.RuleKey.Equals(ruleKey, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (items.Length == current.IgnoredRules.Count)
                return current;
            return await WriteAsync(items, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<Winapp2RulePreferences> WriteAsync(
        IReadOnlyList<Winapp2IgnoredRule> items,
        CancellationToken cancellationToken)
    {
        Validate(items);
        Directory.CreateDirectory(_root);
        EnsureRegularDirectory(_root);
        if (File.Exists(_path)) EnsureRegularFile(_path);
        var document = new StoredDocument { SchemaVersion = SchemaVersion, IgnoredRules = items.ToList() };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        if (bytes.Length > MaximumFileBytes)
            throw new InvalidDataException("The rule-preference file exceeds its size limit.");
        var temporary = _path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await File.WriteAllBytesAsync(temporary, bytes, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(_path))
                File.Replace(temporary, _path, destinationBackupFileName: null, ignoreMetadataErrors: true);
            else
                File.Move(temporary, _path, overwrite: false);
            temporary = string.Empty;
        }
        finally
        {
            if (!string.IsNullOrEmpty(temporary) && File.Exists(temporary)) File.Delete(temporary);
        }

        return new Winapp2RulePreferences
        {
            IgnoredRules = items.OrderByDescending(item => item.AddedAtUtc).ToArray()
        };
    }

    private static void Validate(IReadOnlyList<Winapp2IgnoredRule> items)
    {
        if (items.Count > MaximumIgnoredRules)
            throw new InvalidDataException("The ignored-rule count exceeds its limit.");
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (!Winapp2RulePreferenceKey.IsValid(item.RuleKey)
                || !Winapp2RulePreferenceKey.IsValid(item.RulePackSha256)
                || string.IsNullOrWhiteSpace(item.RuleName)
                || string.IsNullOrWhiteSpace(item.RulePackSource)
                || string.IsNullOrWhiteSpace(item.RulePackVersion)
                || !keys.Add(item.RuleKey))
            {
                throw new InvalidDataException("The ignored-rule entry is invalid.");
            }
        }
    }

    private static void EnsureRegularDirectory(string path)
    {
        if (!Directory.Exists(path) || (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException("The managed rule-preference directory is missing or redirected.");
    }

    private static void EnsureRegularFile(string path)
    {
        if (!File.Exists(path) || (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException("The managed rule-preference file is missing or redirected.");
    }

    private sealed class StoredDocument
    {
        public int SchemaVersion { get; init; }
        public List<Winapp2IgnoredRule> IgnoredRules { get; init; } = [];
    }
}
