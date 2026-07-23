using System.Text.Json;
using System.Text.RegularExpressions;

namespace Css.Core.Updates;

public sealed record PersonalReleasePackage
{
    public string AssetName { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public long Length { get; init; }
    public string SHA256 { get; init; } = string.Empty;
    public string InstallerManifestSHA256 { get; init; } = string.Empty;
    public string SignerThumbprint { get; init; } = string.Empty;
    public bool ValidSameSigner { get; init; }
}

public sealed record PersonalReleaseChannel
{
    public int SchemaVersion { get; init; }
    public string Product { get; init; } = string.Empty;
    public string Repository { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Tag { get; init; } = string.Empty;
    public string CommitSHA { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; init; }
    public PersonalReleasePackage Package { get; init; } = new();
}

public sealed record PersonalReleaseChannelValidation
{
    public required bool IsValid { get; init; }
    public required string Reason { get; init; }
    public PersonalReleaseChannel? Channel { get; init; }
}

public static partial class PersonalReleaseChannelPolicy
{
    public const string Repository = "plnoble/OMNIX-Entropy";
    public const string Product = "OMNIX-Entropy";
    public const long MaximumPackageBytes = 1024L * 1024 * 1024;
    public const int MaximumChannelBytes = 128 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static PersonalReleaseChannelValidation ParseAndValidate(string json)
    {
        if (string.IsNullOrWhiteSpace(json)
            || json.Length > MaximumChannelBytes)
        {
            return Invalid("更新清单为空或过大。");
        }

        PersonalReleaseChannel? channel;
        try
        {
            channel = JsonSerializer.Deserialize<PersonalReleaseChannel>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return Invalid("更新清单格式不正确。");
        }

        if (channel is null
            || channel.SchemaVersion != 1
            || channel.Product != Product
            || channel.Repository != Repository
            || !TryParseVersion(channel.Version, out _)
            || channel.Tag != $"v{channel.Version}"
            || !Sha40Regex().IsMatch(channel.CommitSHA)
            || channel.GeneratedAtUtc == default)
        {
            return Invalid("更新清单的产品、仓库或版本身份不匹配。");
        }

        var package = channel.Package;
        var expectedAsset = $"OMNIX-Entropy-{channel.Version}-win-x64-setup.exe";
        var expectedUrl =
            $"https://github.com/{Repository}/releases/download/{channel.Tag}/{expectedAsset}";
        if (package.AssetName != expectedAsset
            || package.DownloadUrl != expectedUrl
            || package.Length <= 0
            || package.Length > MaximumPackageBytes
            || !Sha256Regex().IsMatch(package.SHA256)
            || !Sha256Regex().IsMatch(package.InstallerManifestSHA256)
            || !Sha40Regex().IsMatch(package.SignerThumbprint)
            || !package.ValidSameSigner)
        {
            return Invalid("更新包的名称、地址、哈希或同签名证据不可信。");
        }

        return new PersonalReleaseChannelValidation
        {
            IsValid = true,
            Reason = "更新清单来源和完整性字段符合 OMNIX 规则。",
            Channel = channel
        };
    }

    public static bool IsNewer(string candidate, Version current)
    {
        ArgumentNullException.ThrowIfNull(current);
        return TryParseVersion(candidate, out var parsed)
            && parsed > new Version(
                Math.Max(0, current.Major),
                Math.Max(0, current.Minor),
                Math.Max(0, current.Build));
    }

    public static bool IsExpectedReleasePage(string url, string tag) =>
        url == $"https://github.com/{Repository}/releases/tag/{tag}";

    public static bool IsExpectedChannelAssetUrl(string url, string tag) =>
        url == $"https://github.com/{Repository}/releases/download/{tag}/omnix-release.json";

    public static bool IsExpectedInstallerManifestUrl(string url, string tag) =>
        url == $"https://github.com/{Repository}/releases/download/{tag}/installer-manifest.json";

    private static bool TryParseVersion(string value, out Version version)
    {
        version = new Version(0, 0, 0);
        if (!StableVersionRegex().IsMatch(value))
            return false;
        return Version.TryParse(value, out version!);
    }

    private static PersonalReleaseChannelValidation Invalid(string reason) => new()
    {
        IsValid = false,
        Reason = reason
    };

    [GeneratedRegex("^[0-9]+\\.[0-9]+\\.[0-9]+$")]
    private static partial Regex StableVersionRegex();

    [GeneratedRegex("^[0-9A-Fa-f]{40}$")]
    private static partial Regex Sha40Regex();

    [GeneratedRegex("^[0-9A-Fa-f]{64}$")]
    private static partial Regex Sha256Regex();
}
