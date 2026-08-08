using System.Net.Http.Headers;

namespace Css.Rules.Winapp2;

public sealed class Winapp2RulePackDownloadClient
{
    private readonly HttpClient _httpClient;
    private readonly Winapp2RulePackStore _store;

    public Winapp2RulePackDownloadClient(
        HttpClient httpClient,
        Winapp2RulePackStore store)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<Winapp2RulePackActivationReceipt> DownloadAndActivateAsync(
        Winapp2RulePackDescriptor descriptor,
        Winapp2RulePackActivationConsent consent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(consent);
        Winapp2RulePackConsentPolicy.ValidateActivation(descriptor, consent);
        if (!descriptor.SourceUri.IsAbsoluteUri
            || !descriptor.SourceUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Community rule-pack downloads require an HTTPS source.");
        }
        cancellationToken.ThrowIfCancellationRequested();

        using var request = new HttpRequestMessage(HttpMethod.Get, descriptor.SourceUri);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("OMNIX-Entropy", "0.1"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.RequestMessage?.RequestUri is not { } finalUri
            || !finalUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("The community rule-pack response did not remain on HTTPS.");
        }
        if (response.Content.Headers.ContentLength > Winapp2RuleCatalogLoader.MaxPackBytes)
            throw new InvalidDataException("The community rule-pack response exceeds its size limit.");

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await _store.ActivateAsync(content, descriptor, consent, cancellationToken);
    }
}
