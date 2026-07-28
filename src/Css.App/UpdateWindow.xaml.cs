using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using Css.Core.Operations;
using Css.Core.Updates;
using Css.Win32.Security;
using Css.Win32.Updates;

namespace Css.App;

public partial class UpdateWindow : Window
{
    private static readonly HttpClient MetadataHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    private static readonly HttpClient PackageHttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    private readonly IPersonalReleaseClient _client;
    private readonly PersonalReleasePackageDownloader? _downloader;
    private readonly PersonalUpdateLaunchOperationHandler? _launchHandler;
    private readonly Version _currentVersion;
    private readonly string _currentExecutablePath;
    private readonly CancellationTokenSource _lifetime = new();
    private string? _releasePageUrl;
    private PersonalReleaseChannel? _availableChannel;
    private VerifiedPersonalUpdatePackage? _verifiedPackage;

    public UpdateWindow()
        : this(CreateRuntimeDependencies())
    {
    }

    internal UpdateWindow(IPersonalReleaseClient client, Version currentVersion)
        : this(
            client,
            currentVersion,
            downloader: null,
            launchHandler: null,
            currentExecutablePath: Environment.ProcessPath
                ?? Path.Combine(AppContext.BaseDirectory, "Css.App.exe"))
    {
    }

    private UpdateWindow(UpdateRuntimeDependencies dependencies)
        : this(
            dependencies.Client,
            Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 1, 0),
            dependencies.Downloader,
            dependencies.LaunchHandler,
            dependencies.CurrentExecutablePath)
    {
    }

    private UpdateWindow(
        IPersonalReleaseClient client,
        Version currentVersion,
        PersonalReleasePackageDownloader? downloader,
        PersonalUpdateLaunchOperationHandler? launchHandler,
        string currentExecutablePath)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _currentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
        _downloader = downloader;
        _launchHandler = launchHandler;
        _currentExecutablePath = currentExecutablePath;
        InitializeComponent();
        CurrentVersionTextBlock.Text =
            $"当前版本 {_currentVersion.Major}.{_currentVersion.Minor}.{Math.Max(0, _currentVersion.Build)}";
    }

    private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        SetButtonsEnabled(false);
        DownloadAndInstallUpdateButton.Visibility = Visibility.Collapsed;
        OpenReleasePageButton.Visibility = Visibility.Collapsed;
        _releasePageUrl = null;
        _availableChannel = null;
        _verifiedPackage = null;
        DownloadAndInstallUpdateButton.Content = "下载并安装";
        UpdateStatusTitleTextBlock.Text = "正在检查";
        UpdateStatusBodyTextBlock.Text = "正在读取固定 GitHub 仓库的公开版本信息...";
        try
        {
            var result = await _client.CheckAsync(
                _currentVersion,
                _lifetime.Token);
            UpdateStatusTitleTextBlock.Text = result.Status switch
            {
                PersonalReleaseCheckStatus.UpdateAvailable => "发现新版本",
                PersonalReleaseCheckStatus.UpToDate => "已经是最新版本",
                _ => "暂时无法确认更新"
            };
            UpdateStatusBodyTextBlock.Text = result.Message;
            if (result.Status == PersonalReleaseCheckStatus.UpdateAvailable
                && result.ReleasePageUrl is not null
                && result.Channel is not null
                && PersonalReleaseChannelPolicy.IsExpectedReleasePage(
                    result.ReleasePageUrl,
                    result.Channel.Tag))
            {
                _releasePageUrl = result.ReleasePageUrl;
                _availableChannel = result.Channel;
                OpenReleasePageButton.Visibility = Visibility.Visible;
                if (_downloader is not null && _launchHandler is not null)
                    DownloadAndInstallUpdateButton.Visibility = Visibility.Visible;
            }
        }
        catch
        {
            UpdateStatusTitleTextBlock.Text = "暂时无法确认更新";
            UpdateStatusBodyTextBlock.Text =
                "更新检查没有完成，也没有下载或安装任何内容。";
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    private async void DownloadAndInstallUpdate_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_downloader is null
            || _launchHandler is null
            || (_availableChannel is null && _verifiedPackage is null))
        {
            return;
        }

        SetButtonsEnabled(false);
        try
        {
            if (_verifiedPackage is null)
            {
                UpdateStatusTitleTextBlock.Text = "正在下载并验证";
                UpdateStatusBodyTextBlock.Text =
                    "更新包正在保存到 OMNIX 的 D 盘更新目录。验证完成前不会启动。";
                var download = await _downloader.DownloadAndVerifyAsync(
                    _availableChannel!,
                    _currentExecutablePath,
                    _lifetime.Token);
                if (download.Status != PersonalUpdateDownloadStatus.Ready
                    || download.Package is null)
                {
                    UpdateStatusTitleTextBlock.Text = "没有开始安装";
                    UpdateStatusBodyTextBlock.Text = download.Message;
                    return;
                }

                _verifiedPackage = download.Package;
                DownloadAndInstallUpdateButton.Content = "打开安装程序";
                UpdateStatusTitleTextBlock.Text = "更新包已验证";
                UpdateStatusBodyTextBlock.Text =
                    $"版本 {_verifiedPackage.Version} 已下载到 D 盘，哈希和同发布者签名均已通过。";
            }

            var confirmation = MessageBox.Show(
                this,
                $"准备打开 OMNIX-Entropy {_verifiedPackage.Version} 的安装界面。\n\n"
                + "当前 OMNIX 随后会退出。安装程序不会静默操作，安装位置和最后的安装按钮仍由你确认。",
                "确认安装更新",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information,
                MessageBoxResult.No);
            if (confirmation != MessageBoxResult.Yes)
            {
                UpdateStatusBodyTextBlock.Text =
                    $"版本 {_verifiedPackage.Version} 已验证并保留在 D 盘，尚未启动安装。";
                return;
            }

            var operation = PersonalUpdateLaunchOperationPlanner.Confirm(
                PersonalUpdateLaunchOperationPlanner.Create(
                    _verifiedPackage,
                    _currentExecutablePath),
                DateTimeOffset.UtcNow);
            var pipeline = new SafetyOperationPipeline(
                _launchHandler.ExecuteAsync);
            var result = await pipeline.ExecuteAsync(
                operation,
                _lifetime.Token);
            if (!result.Success)
            {
                UpdateStatusTitleTextBlock.Text = "安装程序没有打开";
                UpdateStatusBodyTextBlock.Text =
                    result.Error ?? "更新包复核失败，当前版本没有变化。";
                return;
            }

            UpdateStatusTitleTextBlock.Text = "安装程序已打开";
            UpdateStatusBodyTextBlock.Text =
                result.Summary ?? "请在安装界面中确认位置和安装操作。";
            Application.Current.Shutdown();
        }
        catch
        {
            UpdateStatusTitleTextBlock.Text = "没有开始安装";
            UpdateStatusBodyTextBlock.Text =
                "更新流程没有完成，也没有静默安装任何内容。";
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    private void SetButtonsEnabled(bool enabled)
    {
        CheckForUpdatesButton.IsEnabled = enabled;
        DownloadAndInstallUpdateButton.IsEnabled = enabled;
        OpenReleasePageButton.IsEnabled = enabled;
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

    protected override void OnClosed(EventArgs e)
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
        base.OnClosed(e);
    }

    private static UpdateRuntimeDependencies CreateRuntimeDependencies()
    {
        var currentExecutablePath = Environment.ProcessPath
            ?? Path.Combine(AppContext.BaseDirectory, "Css.App.exe");
        var signatures = new WindowsAuthenticodeSignatureVerifier();
        var pathPolicy = new WindowsPersonalUpdatePathPolicy();
        return new UpdateRuntimeDependencies(
            new GitHubPersonalReleaseClient(MetadataHttpClient),
            new PersonalReleasePackageDownloader(
                PackageHttpClient,
                signatures,
                pathPolicy),
            new PersonalUpdateLaunchOperationHandler(
                signatures,
                pathPolicy,
                new WindowsPersonalUpdateInstallerLauncher(),
                currentExecutablePath),
            currentExecutablePath);
    }

    private sealed record UpdateRuntimeDependencies(
        IPersonalReleaseClient Client,
        PersonalReleasePackageDownloader Downloader,
        PersonalUpdateLaunchOperationHandler LaunchHandler,
        string CurrentExecutablePath);
}
