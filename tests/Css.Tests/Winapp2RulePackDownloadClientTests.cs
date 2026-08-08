using System.Net;
using System.Security.Cryptography;
using System.Text;
using Css.Rules.Winapp2;
using FluentAssertions;

namespace Css.Tests;

public sealed class Winapp2RulePackDownloadClientTests
{
    [Fact]
    public async Task Download_requires_reviewed_consent_and_https_before_any_request()
    {
        var root = TemporaryRoot();
        const string content = "[Example]\nFileKey1=C:\\Fixture\\Cache|*\n";
        var secure = Descriptor(content, new Uri("https://example.invalid/winapp2.ini"));
        var insecure = Descriptor(content, new Uri("http://example.invalid/winapp2.ini"));
        var handler = new RecordingHandler(_ => Response(content));
        var client = new Winapp2RulePackDownloadClient(
            new HttpClient(handler),
            new Winapp2RulePackStore(root));

        try
        {
            var noConsent = () => client.DownloadAndActivateAsync(
                secure,
                Consent(secure) with { UserAcceptedLicense = false });
            var http = () => client.DownloadAndActivateAsync(insecure, Consent(insecure));

            await noConsent.Should().ThrowAsync<InvalidOperationException>().WithMessage("*confirmation*");
            await http.Should().ThrowAsync<InvalidOperationException>().WithMessage("*HTTPS*");
            handler.RequestCount.Should().Be(0);
            Directory.Exists(root).Should().BeFalse();
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task Download_streams_only_pinned_content_into_the_validating_store()
    {
        var root = TemporaryRoot();
        const string content = "[Example]\nFileKey1=C:\\Fixture\\Cache|*\n";
        var descriptor = Descriptor(content, new Uri("https://example.invalid/winapp2.ini"));
        var handler = new RecordingHandler(_ => Response(content));
        var store = new Winapp2RulePackStore(root);
        var client = new Winapp2RulePackDownloadClient(new HttpClient(handler), store);

        try
        {
            var receipt = await client.DownloadAndActivateAsync(descriptor, Consent(descriptor));

            receipt.ActiveVersion.Should().Be("fixture-v1");
            receipt.IsExecutionAuthorized.Should().BeFalse();
            handler.RequestCount.Should().Be(1);
            handler.LastRequest!.RequestUri.Should().Be(descriptor.SourceUri);
            handler.LastRequest.Headers.UserAgent.ToString().Should().Contain("OMNIX-Entropy");
            store.LoadActiveCatalog().Rules.Should().ContainSingle();
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task Oversized_or_hash_changed_download_never_replaces_the_active_pointer()
    {
        var root = TemporaryRoot();
        const string firstContent = "[First]\nFileKey1=C:\\Fixture\\First|*\n";
        const string changedContent = "[Changed]\nFileKey1=C:\\Fixture\\Changed|*\n";
        var first = Descriptor(firstContent, new Uri("https://example.invalid/first.ini"));
        var changed = Descriptor(firstContent, new Uri("https://example.invalid/changed.ini")) with
        {
            Version = "changed"
        };
        var store = new Winapp2RulePackStore(root);
        await store.ActivateAsync(Stream(firstContent), first, Consent(first));
        var statePath = store.GetStatus()!.StatePath;
        var before = await File.ReadAllBytesAsync(statePath);
        var changedClient = new Winapp2RulePackDownloadClient(
            new HttpClient(new RecordingHandler(_ => Response(changedContent))),
            store);
        var oversizedClient = new Winapp2RulePackDownloadClient(
            new HttpClient(new RecordingHandler(_ => OversizedResponse())),
            store);

        try
        {
            var changedAction = () => changedClient.DownloadAndActivateAsync(changed, Consent(changed));
            var oversizedAction = () => oversizedClient.DownloadAndActivateAsync(changed, Consent(changed));

            await changedAction.Should().ThrowAsync<InvalidDataException>().WithMessage("*SHA-256*");
            await oversizedAction.Should().ThrowAsync<InvalidDataException>().WithMessage("*size*");
            (await File.ReadAllBytesAsync(statePath)).Should().Equal(before);
            store.GetStatus()!.ActiveDescriptor.Version.Should().Be("fixture-v1");
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    private static HttpResponseMessage Response(string content) => new(HttpStatusCode.OK)
    {
        Content = new ByteArrayContent(Encoding.UTF8.GetBytes(content))
    };

    private static HttpResponseMessage OversizedResponse()
    {
        var response = Response(string.Empty);
        response.Content.Headers.ContentLength = Winapp2RuleCatalogLoader.MaxPackBytes + 1;
        return response;
    }

    private static Winapp2RulePackActivationConsent Consent(Winapp2RulePackDescriptor descriptor) =>
        new()
        {
            UserConfirmedActivation = true,
            UserAcceptedLicense = true,
            ReviewedSourceUri = descriptor.SourceUri,
            ReviewedLicenseUri = descriptor.LicenseUri,
            ReviewedVersion = descriptor.Version,
            ReviewedSha256 = descriptor.ExpectedSha256
        };

    private static Winapp2RulePackDescriptor Descriptor(string content, Uri sourceUri) =>
        new()
        {
            SourceName = "Fixture pack",
            SourceUri = sourceUri,
            Version = "fixture-v1",
            LicenseName = "Fixture-only",
            LicenseUri = new Uri("https://example.invalid/license"),
            ExpectedSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)))
        };

    private static MemoryStream Stream(string content) => new(Encoding.UTF8.GetBytes(content));

    private static string TemporaryRoot() =>
        Path.Combine(Path.GetTempPath(), "omnix-winapp2-download-" + Guid.NewGuid().ToString("N"));

    private static void DeleteRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            LastRequest = request;
            var result = response(request);
            result.RequestMessage = request;
            return Task.FromResult(result);
        }
    }
}
