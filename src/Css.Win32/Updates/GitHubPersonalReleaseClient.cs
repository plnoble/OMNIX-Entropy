using System.Net.Http.Headers;
using System.Text.Json;
using Css.Core.Updates;

namespace Css.Win32.Updates;

public enum PersonalReleaseCheckStatus
{
    UpdateAvailable,
    UpToDate,
    Unavailable
}

public sealed record PersonalReleaseCheckResult
{
    public required PersonalReleaseCheckStatus Status { get; init; }
    public required string Message { get; init; }
    public string? Version { get; init; }
    public string? ReleasePageUrl { get; init; }
    public PersonalReleaseChannel? Channel { get; init; }
}

public interface IPersonalReleaseClient
{
    Task<PersonalReleaseCheckResult> CheckAsync(
        Version currentVersion,
        CancellationToken cancellationToken = default);
}

public sealed class GitHubPersonalReleaseClient(HttpClient httpClient) : IPersonalReleaseClient
{
    private const string LatestReleaseApi =
        "https://api.github.com/repos/plnoble/OMNIX-Entropy/releases/latest";
    private readonly HttpClient _httpClient = httpClient
        ?? throw new ArgumentNullException(nameof(httpClient));

    public async Task<PersonalReleaseCheckResult> CheckAsync(
        Version currentVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(currentVersion);
        try
        {
            using var releaseRequest = CreateRequest(LatestReleaseApi);
            using var releaseResponse = await _httpClient.SendAsync(
                releaseRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (!releaseResponse.IsSuccessStatusCode)
                return Unavailable("暂时无法读取 GitHub 最新版本。");

            var releaseJson = await ReadBoundedAsync(
                releaseResponse,
                PersonalReleaseChannelPolicy.MaximumChannelBytes,
                cancellationToken);
            using var releaseDocument = JsonDocument.Parse(releaseJson);
            var root = releaseDocument.RootElement;
            var tag = RequiredString(root, "tag_name");
            var releasePage = RequiredString(root, "html_url");
            if (root.GetProperty("draft").GetBoolean()
                || root.GetProperty("prerelease").GetBoolean()
                || !PersonalReleaseChannelPolicy.IsExpectedReleasePage(releasePage, tag))
            {
                return Unavailable("GitHub 版本身份不符合 OMNIX 更新规则。");
            }

            var channelAssetUrl = FindChannelAssetUrl(root, tag);
            if (channelAssetUrl is null)
                return Unavailable("这个版本没有可信的更新清单。");

            using var channelRequest = CreateRequest(channelAssetUrl);
            using var channelResponse = await _httpClient.SendAsync(
                channelRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (!channelResponse.IsSuccessStatusCode)
                return Unavailable("更新清单下载失败，没有下载程序包。");

            var channelJson = await ReadBoundedAsync(
                channelResponse,
                PersonalReleaseChannelPolicy.MaximumChannelBytes,
                cancellationToken);
            var validation = PersonalReleaseChannelPolicy.ParseAndValidate(channelJson);
            if (!validation.IsValid
                || validation.Channel is null
                || validation.Channel.Tag != tag)
            {
                return Unavailable(validation.Reason);
            }

            var available = PersonalReleaseChannelPolicy.IsNewer(
                validation.Channel.Version,
                currentVersion);
            return new PersonalReleaseCheckResult
            {
                Status = available
                    ? PersonalReleaseCheckStatus.UpdateAvailable
                    : PersonalReleaseCheckStatus.UpToDate,
                Message = available
                    ? $"发现新版本 {validation.Channel.Version}，目前还没有下载或安装。"
                    : "当前已经是最新版本。",
                Version = validation.Channel.Version,
                ReleasePageUrl = releasePage,
                Channel = validation.Channel
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Unavailable("连接 GitHub 超时，请稍后再试。");
        }
        catch (HttpRequestException)
        {
            return Unavailable("无法连接 GitHub，请检查网络后重试。");
        }
        catch (JsonException)
        {
            return Unavailable("GitHub 返回的版本信息无法验证。");
        }
        catch (KeyNotFoundException)
        {
            return Unavailable("GitHub 返回的版本信息缺少必要字段。");
        }
        catch (InvalidOperationException)
        {
            return Unavailable("GitHub 返回的版本字段类型不正确。");
        }
        catch (InvalidDataException)
        {
            return Unavailable("GitHub 返回的数据超过安全限制。");
        }
    }

    private static HttpRequestMessage CreateRequest(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("OMNIX-Entropy", "0.1"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        return request;
    }

    private static string? FindChannelAssetUrl(JsonElement root, string tag)
    {
        foreach (var asset in root.GetProperty("assets").EnumerateArray())
        {
            if (RequiredString(asset, "name") != "omnix-release.json")
                continue;
            if (asset.GetProperty("size").GetInt64()
                > PersonalReleaseChannelPolicy.MaximumChannelBytes)
                return null;
            var url = RequiredString(asset, "browser_download_url");
            return PersonalReleaseChannelPolicy.IsExpectedChannelAssetUrl(url, tag)
                ? url
                : null;
        }
        return null;
    }

    private static string RequiredString(JsonElement element, string property) =>
        element.GetProperty(property).GetString()
        ?? throw new JsonException($"Missing property: {property}");

    private static async Task<string> ReadBoundedAsync(
        HttpResponseMessage response,
        int maximumBytes,
        CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength > maximumBytes)
            throw new InvalidDataException("Response is too large.");

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var memory = new MemoryStream();
        var buffer = new byte[8192];
        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read == 0)
                break;
            if (memory.Length + read > maximumBytes)
                throw new InvalidDataException("Response is too large.");
            memory.Write(buffer, 0, read);
        }
        return System.Text.Encoding.UTF8.GetString(memory.ToArray());
    }

    private static PersonalReleaseCheckResult Unavailable(string message) => new()
    {
        Status = PersonalReleaseCheckStatus.Unavailable,
        Message = message
    };
}
