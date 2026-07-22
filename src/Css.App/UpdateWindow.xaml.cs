using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using Css.Core.Updates;
using Css.Win32.Updates;

namespace Css.App;

public partial class UpdateWindow : Window
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private readonly IPersonalReleaseClient _client;
    private readonly Version _currentVersion;
    private string? _releasePageUrl;

    public UpdateWindow()
        : this(
            new GitHubPersonalReleaseClient(HttpClient),
            Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 1, 0))
    {
    }

    internal UpdateWindow(IPersonalReleaseClient client, Version currentVersion)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _currentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
        InitializeComponent();
        CurrentVersionTextBlock.Text =
            $"当前版本 {_currentVersion.Major}.{_currentVersion.Minor}.{Math.Max(0, _currentVersion.Build)}";
    }

    private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        CheckForUpdatesButton.IsEnabled = false;
        OpenReleasePageButton.Visibility = Visibility.Collapsed;
        _releasePageUrl = null;
        UpdateStatusTitleTextBlock.Text = "正在检查";
        UpdateStatusBodyTextBlock.Text = "正在读取固定 GitHub 仓库的公开版本信息...";
        try
        {
            var result = await _client.CheckAsync(_currentVersion);
            UpdateStatusTitleTextBlock.Text = result.Status switch
            {
                PersonalReleaseCheckStatus.UpdateAvailable => "发现新版本",
                PersonalReleaseCheckStatus.UpToDate => "已经是最新版本",
                _ => "暂时无法确认更新"
            };
            UpdateStatusBodyTextBlock.Text = result.Message;
            if (result.Status == PersonalReleaseCheckStatus.UpdateAvailable
                && result.ReleasePageUrl is not null
                && PersonalReleaseChannelPolicy.IsExpectedReleasePage(
                    result.ReleasePageUrl,
                    result.Channel?.Tag ?? string.Empty))
            {
                _releasePageUrl = result.ReleasePageUrl;
                OpenReleasePageButton.Visibility = Visibility.Visible;
            }
        }
        catch
        {
            UpdateStatusTitleTextBlock.Text = "暂时无法确认更新";
            UpdateStatusBodyTextBlock.Text = "更新检查没有完成，也没有下载或安装任何内容。";
        }
        finally
        {
            CheckForUpdatesButton.IsEnabled = true;
        }
    }

    private void OpenReleasePage_Click(object sender, RoutedEventArgs e)
    {
        if (_releasePageUrl is null)
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = _releasePageUrl,
            UseShellExecute = true
        });
    }
}
