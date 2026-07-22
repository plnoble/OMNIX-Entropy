using System.Text.Json;
using Css.Core.Updates;
using FluentAssertions;

namespace Css.Tests;

public sealed class PersonalReleaseChannelTests
{
    [Fact]
    public void Valid_fixed_repository_channel_is_accepted()
    {
        var result = PersonalReleaseChannelPolicy.ParseAndValidate(CreateJson());

        result.IsValid.Should().BeTrue();
        result.Channel!.Version.Should().Be("0.2.0");
        PersonalReleaseChannelPolicy.IsNewer("0.2.0", new Version(0, 1, 0))
            .Should().BeTrue();
    }

    [Theory]
    [InlineData("Repository", "attacker/OMNIX-Entropy")]
    [InlineData("Product", "Other")]
    [InlineData("Version", "0.2.0-beta")]
    public void Changed_release_identity_is_rejected(string property, string value)
    {
        var channel = CreateChannel() with
        {
            Repository = property == "Repository" ? value : PersonalReleaseChannelPolicy.Repository,
            Product = property == "Product" ? value : PersonalReleaseChannelPolicy.Product,
            Version = property == "Version" ? value : "0.2.0"
        };

        PersonalReleaseChannelPolicy.ParseAndValidate(JsonSerializer.Serialize(channel))
            .IsValid.Should().BeFalse();
    }

    [Fact]
    public void Package_url_hash_signer_and_same_signer_are_all_required()
    {
        var baseline = CreateChannel();
        var variants = new[]
        {
            baseline with { Package = baseline.Package with { DownloadUrl = "https://example.com/update.zip" } },
            baseline with { Package = baseline.Package with { SHA256 = "ABC" } },
            baseline with { Package = baseline.Package with { SignerThumbprint = "ABC" } },
            baseline with { Package = baseline.Package with { ValidSameSigner = false } },
            baseline with { Package = baseline.Package with { Length = PersonalReleaseChannelPolicy.MaximumPackageBytes + 1 } }
        };

        foreach (var channel in variants)
        {
            PersonalReleaseChannelPolicy.ParseAndValidate(JsonSerializer.Serialize(channel))
                .IsValid.Should().BeFalse();
        }
    }

    [Fact]
    public void Only_exact_release_and_channel_asset_urls_are_allowed()
    {
        PersonalReleaseChannelPolicy.IsExpectedReleasePage(
            "https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.2.0",
            "v0.2.0").Should().BeTrue();
        PersonalReleaseChannelPolicy.IsExpectedReleasePage(
            "https://github.com/attacker/OMNIX-Entropy/releases/tag/v0.2.0",
            "v0.2.0").Should().BeFalse();
        PersonalReleaseChannelPolicy.IsExpectedChannelAssetUrl(
            "https://github.com/plnoble/OMNIX-Entropy/releases/download/v0.2.0/omnix-release.json",
            "v0.2.0").Should().BeTrue();
    }

    private static string CreateJson() => JsonSerializer.Serialize(CreateChannel());

    private static PersonalReleaseChannel CreateChannel() => new()
    {
        SchemaVersion = 1,
        Product = "OMNIX-Entropy",
        Repository = "plnoble/OMNIX-Entropy",
        Version = "0.2.0",
        Tag = "v0.2.0",
        CommitSHA = new string('A', 40),
        GeneratedAtUtc = DateTimeOffset.Parse("2026-07-22T00:00:00Z"),
        Package = new PersonalReleasePackage
        {
            AssetName = "OMNIX-Entropy-0.2.0-win-x64.zip",
            DownloadUrl = "https://github.com/plnoble/OMNIX-Entropy/releases/download/v0.2.0/OMNIX-Entropy-0.2.0-win-x64.zip",
            Length = 1024,
            SHA256 = new string('B', 64),
            PackageManifestSHA256 = new string('C', 64),
            SignerThumbprint = new string('D', 40),
            ValidSameSigner = true
        }
    };
}
