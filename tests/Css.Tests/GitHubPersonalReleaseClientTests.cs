using System.Net;
using System.Text;
using System.Text.Json;
using Css.Core.Updates;
using Css.Win32.Updates;
using FluentAssertions;

namespace Css.Tests;

public sealed class GitHubPersonalReleaseClientTests
{
    [Fact]
    public async Task Valid_new_release_is_reported_without_downloading_package()
    {
        var handler = new QueueHandler(
            JsonResponse(CreateReleaseJson()),
            JsonResponse(CreateChannelJson()));
        var client = new GitHubPersonalReleaseClient(new HttpClient(handler));

        var result = await client.CheckAsync(new Version(0, 1, 0));

        result.Status.Should().Be(PersonalReleaseCheckStatus.UpdateAvailable);
        result.Version.Should().Be("0.2.0");
        handler.Requests.Should().Equal(
            "https://api.github.com/repos/plnoble/OMNIX-Entropy/releases/latest",
            "https://github.com/plnoble/OMNIX-Entropy/releases/download/v0.2.0/omnix-release.json");
        handler.Requests.Should().NotContain(value => value.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Draft_prerelease_or_wrong_asset_host_is_refused()
    {
        foreach (var release in new[]
                 {
                     CreateReleaseJson(draft: true),
                     CreateReleaseJson(prerelease: true),
                     CreateReleaseJson(channelUrl: "https://example.com/omnix-release.json")
                 })
        {
            var handler = new QueueHandler(JsonResponse(release));
            var result = await new GitHubPersonalReleaseClient(new HttpClient(handler))
                .CheckAsync(new Version(0, 1, 0));

            result.Status.Should().Be(PersonalReleaseCheckStatus.Unavailable);
            handler.Requests.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task Invalid_channel_is_refused_without_package_download()
    {
        var channel = CreateChannel() with
        {
            Package = CreateChannel().Package with { ValidSameSigner = false }
        };
        var handler = new QueueHandler(
            JsonResponse(CreateReleaseJson()),
            JsonResponse(JsonSerializer.Serialize(channel)));

        var result = await new GitHubPersonalReleaseClient(new HttpClient(handler))
            .CheckAsync(new Version(0, 1, 0));

        result.Status.Should().Be(PersonalReleaseCheckStatus.Unavailable);
        result.ReleasePageUrl.Should().BeNull();
        handler.Requests.Should().NotContain(value => value.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
    }

    private static string CreateReleaseJson(
        bool draft = false,
        bool prerelease = false,
        string? channelUrl = null) => JsonSerializer.Serialize(new
        {
            tag_name = "v0.2.0",
            html_url = "https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.2.0",
            draft,
            prerelease,
            assets = new[]
            {
                new
                {
                    name = "omnix-release.json",
                    size = 1024,
                    browser_download_url = channelUrl
                        ?? "https://github.com/plnoble/OMNIX-Entropy/releases/download/v0.2.0/omnix-release.json"
                }
            }
        });

    private static string CreateChannelJson() => JsonSerializer.Serialize(CreateChannel());

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

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed class QueueHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);
        public List<string> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!.AbsoluteUri);
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
