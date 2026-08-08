using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using Css.Core.Software;
using Css.Rules.Winapp2;
using Microsoft.Win32;

namespace Css.App;

public partial class CommunityRuleCenterWindow : Window
{
    private readonly Winapp2RulePackStore _rulePackStore;
    private readonly Winapp2RulePreferenceStore _preferenceStore;
    private readonly IReadOnlyList<SoftwareProfile> _profiles;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private CancellationTokenSource? _operationCancellation;
    private string? _selectedFilePath;
    private Winapp2RulePackStatus? _status;
    private Winapp2RulePreferences _preferences = Winapp2RulePreferences.Empty;

    public CommunityRuleCenterWindow(
        Winapp2RulePackStore rulePackStore,
        Winapp2RulePreferenceStore preferenceStore,
        IReadOnlyList<SoftwareProfile> profiles,
        HttpClient? httpClient = null)
    {
        _rulePackStore = rulePackStore ?? throw new ArgumentNullException(nameof(rulePackStore));
        _preferenceStore = preferenceStore ?? throw new ArgumentNullException(nameof(preferenceStore));
        _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        _ownsHttpClient = httpClient is null;
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        InitializeComponent();
        RefreshView();
    }

    public bool InventoryRefreshRequested { get; private set; }

    private void RefreshView()
    {
        string? loadError = null;
        try
        {
            _status = _rulePackStore.GetStatus();
            _preferences = _preferenceStore.Load();
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or ArgumentException)
        {
            _status = null;
            _preferences = Winapp2RulePreferences.Empty;
            loadError = exception.Message;
        }

        var view = Winapp2RuleCenterPresenter.Create(_status, _profiles, _preferences, loadError);
        RuleCenterStatusHeadlineTextBlock.Text = view.StatusHeadline;
        RuleCenterStatusSummaryTextBlock.Text = view.StatusSummary;
        RuleCenterSourceTextBlock.Text = view.SourceSummary;
        RuleCenterLicenseTextBlock.Text = view.LicenseSummary;
        RuleCenterVersionTextBlock.Text = view.VersionSummary;
        RuleCenterRollbackSummaryTextBlock.Text = view.RollbackSummary;
        RuleCenterRollbackButton.IsEnabled = view.CanRollback;
        RuleCenterRollbackCheckBox.IsEnabled = view.CanRollback;
        RuleCenterPreviewSummaryTextBlock.Text = view.PreviewSummary;
        RuleCenterPreviewListBox.ItemsSource = view.PreviewRows;
        RuleCenterIgnoredRulesListBox.ItemsSource = view.IgnoredRules;
        RuleCenterTechnicalDetailsListBox.Visibility = Visibility.Collapsed;
        RuleCenterTechnicalDetailsListBox.ItemsSource = Array.Empty<string>();
        RuleCenterIgnoreRuleButton.IsEnabled = false;
        RuleCenterTechnicalDetailsButton.IsEnabled = false;
        RuleCenterRestoreRuleButton.IsEnabled = false;
        if (_status is not null && string.IsNullOrWhiteSpace(RuleCenterSourceNameTextBox.Text))
        {
            var descriptor = _status.ActiveDescriptor;
            RuleCenterSourceNameTextBox.Text = descriptor.SourceName;
            RuleCenterSourceUriTextBox.Text = descriptor.SourceUri.AbsoluteUri;
            RuleCenterVersionTextBox.Text = descriptor.Version;
            RuleCenterLicenseNameTextBox.Text = descriptor.LicenseName;
            RuleCenterLicenseUriTextBox.Text = descriptor.LicenseUri.AbsoluteUri;
            RuleCenterSha256TextBox.Text = descriptor.ExpectedSha256;
        }
    }

    private async void ChooseFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择 Winapp2 兼容规则文件",
            Filter = "规则文件 (*.ini)|*.ini|文本文件 (*.txt)|*.txt",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialog.ShowDialog(this) != true) return;

        try
        {
            var info = new FileInfo(dialog.FileName);
            if (info.Length is <= 0 or > Winapp2RuleCatalogLoader.MaxPackBytes)
                throw new InvalidDataException("规则文件为空或超过 8 MB 安全上限。");
            RuleCenterChooseFileButton.IsEnabled = false;
            var hash = await Task.Run(() => ComputeSha256(dialog.FileName));
            _selectedFilePath = dialog.FileName;
            RuleCenterSelectedFileTextBlock.Text = Path.GetFileName(dialog.FileName);
            RuleCenterSelectedFileTextBlock.ToolTip = dialog.FileName;
            RuleCenterSha256TextBox.Text = hash;
            RuleCenterOperationStatusTextBlock.Text = "文件已读取校验值；尚未启用。请继续填写来源和许可证。";
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException
            or InvalidDataException)
        {
            _selectedFilePath = null;
            RuleCenterSelectedFileTextBlock.Text = "文件无法读取";
            RuleCenterOperationStatusTextBlock.Text = "没有导入：" + exception.Message;
        }
        finally
        {
            RuleCenterChooseFileButton.IsEnabled = true;
        }
    }

    private async void ActivateFile_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedFilePath) || !File.Exists(_selectedFilePath))
        {
            RuleCenterOperationStatusTextBlock.Text = "请先选择一个仍然存在的本地规则文件。";
            return;
        }
        var request = BuildActivationRequest();
        if (!request.CanActivate)
        {
            ShowMissing(request);
            return;
        }

        await RunGuardedDataOperationAsync(async cancellationToken =>
        {
            await using var stream = new FileStream(
                _selectedFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            var receipt = await _rulePackStore.ActivateAsync(
                stream,
                request.Descriptor!,
                request.Consent!,
                cancellationToken);
            return $"已启用 {receipt.ActiveVersion}；请关闭窗口并重新扫描应用。";
        });
    }

    private async void Download_Click(object sender, RoutedEventArgs e)
    {
        var request = BuildActivationRequest();
        if (!request.CanActivate)
        {
            ShowMissing(request);
            return;
        }

        await RunGuardedDataOperationAsync(async cancellationToken =>
        {
            var receipt = await new Winapp2RulePackDownloadClient(_httpClient, _rulePackStore)
                .DownloadAndActivateAsync(
                    request.Descriptor!,
                    request.Consent!,
                    cancellationToken);
            return $"已下载、校验并启用 {receipt.ActiveVersion}；请关闭窗口并重新扫描应用。";
        });
    }

    private async void Rollback_Click(object sender, RoutedEventArgs e)
    {
        if (RuleCenterRollbackCheckBox.IsChecked != true)
        {
            RuleCenterOperationStatusTextBlock.Text = "请先勾选确认回到上一个版本。";
            return;
        }
        var status = _rulePackStore.GetStatus();
        if (status?.PreviousDescriptor is null)
        {
            RuleCenterOperationStatusTextBlock.Text = "当前没有可回退的上一版本。";
            RefreshView();
            return;
        }

        await RunGuardedDataOperationAsync(async cancellationToken =>
        {
            var receipt = await _rulePackStore.RollbackAsync(
                new Winapp2RulePackRollbackConsent
                {
                    UserConfirmedRollback = true,
                    ExpectedActiveSha256 = status.ActiveDescriptor.ExpectedSha256,
                    ExpectedPreviousSha256 = status.PreviousDescriptor.ExpectedSha256
                },
                cancellationToken);
            return $"已回到 {receipt.ActiveVersion}；请关闭窗口并重新扫描应用。";
        });
    }

    private async void IgnoreRule_Click(object sender, RoutedEventArgs e)
    {
        if (RuleCenterPreviewListBox.SelectedItem is not Winapp2RulePreviewRow row) return;
        await RunGuardedDataOperationAsync(async cancellationToken =>
        {
            await _preferenceStore.IgnoreAsync(row.Evidence, cancellationToken);
            return $"已忽略“{row.RuleName}”；这只影响以后显示的只读发现。";
        });
    }

    private async void RestoreRule_Click(object sender, RoutedEventArgs e)
    {
        if (RuleCenterIgnoredRulesListBox.SelectedItem is not Winapp2IgnoredRuleRow row) return;
        await RunGuardedDataOperationAsync(async cancellationToken =>
        {
            await _preferenceStore.RestoreAsync(row.RuleKey, cancellationToken);
            return $"已恢复“{row.RuleName}”；重新扫描后会再次评估。";
        });
    }

    private Winapp2RuleActivationRequest BuildActivationRequest() =>
        Winapp2RuleActivationRequestBuilder.Build(new Winapp2RuleActivationInput
        {
            SourceName = RuleCenterSourceNameTextBox.Text,
            SourceUriText = RuleCenterSourceUriTextBox.Text,
            Version = RuleCenterVersionTextBox.Text,
            LicenseName = RuleCenterLicenseNameTextBox.Text,
            LicenseUriText = RuleCenterLicenseUriTextBox.Text,
            Sha256 = RuleCenterSha256TextBox.Text,
            UserAcceptedLicense = RuleCenterLicenseCheckBox.IsChecked == true,
            UserConfirmedActivation = RuleCenterActivationCheckBox.IsChecked == true
        });

    private void ShowMissing(Winapp2RuleActivationRequest request)
    {
        RuleCenterOperationStatusTextBlock.Text = string.Join(" ", request.MissingRequirements);
    }

    private async Task RunGuardedDataOperationAsync(
        Func<CancellationToken, Task<string>> operation)
    {
        if (_operationCancellation is not null) return;
        _operationCancellation = new CancellationTokenSource();
        SetBusy(true);
        RuleCenterOperationStatusTextBlock.Text = "正在处理规则数据；不会执行电脑清理。";
        try
        {
            var message = await operation(_operationCancellation.Token);
            InventoryRefreshRequested = true;
            RefreshView();
            RuleCenterOperationStatusTextBlock.Text = message;
        }
        catch (OperationCanceledException)
        {
            RuleCenterOperationStatusTextBlock.Text = "操作已取消；活动版本和电脑文件没有被当作清理对象。";
        }
        catch (Exception exception) when (exception is IOException
            or HttpRequestException
            or UnauthorizedAccessException
            or InvalidDataException
            or InvalidOperationException
            or ArgumentException)
        {
            RuleCenterOperationStatusTextBlock.Text = "没有完成：" + exception.Message;
            RefreshView();
        }
        finally
        {
            _operationCancellation.Dispose();
            _operationCancellation = null;
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        RuleCenterChooseFileButton.IsEnabled = !busy;
        RuleCenterActivateFileButton.IsEnabled = !busy;
        RuleCenterDownloadButton.IsEnabled = !busy;
        RuleCenterRollbackButton.IsEnabled = !busy && _status?.CanRollback == true;
        RuleCenterIgnoreRuleButton.IsEnabled = !busy && RuleCenterPreviewListBox.SelectedItem is not null;
        RuleCenterRestoreRuleButton.IsEnabled = !busy && RuleCenterIgnoredRulesListBox.SelectedItem is not null;
        RuleCenterCancelOperationButton.IsEnabled = busy;
        RuleCenterCloseButton.IsEnabled = !busy;
    }

    private void PreviewList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = RuleCenterPreviewListBox.SelectedItem is Winapp2RulePreviewRow;
        RuleCenterIgnoreRuleButton.IsEnabled = selected && _operationCancellation is null;
        RuleCenterTechnicalDetailsButton.IsEnabled = selected;
        RuleCenterTechnicalDetailsListBox.Visibility = Visibility.Collapsed;
    }

    private void IgnoredRulesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RuleCenterRestoreRuleButton.IsEnabled =
            RuleCenterIgnoredRulesListBox.SelectedItem is Winapp2IgnoredRuleRow
            && _operationCancellation is null;
    }

    private void ToggleTechnicalDetails_Click(object sender, RoutedEventArgs e)
    {
        if (RuleCenterPreviewListBox.SelectedItem is not Winapp2RulePreviewRow row) return;
        var show = RuleCenterTechnicalDetailsListBox.Visibility != Visibility.Visible;
        RuleCenterTechnicalDetailsListBox.ItemsSource = row.TechnicalDetails;
        RuleCenterTechnicalDetailsListBox.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        RuleCenterTechnicalDetailsButton.Content = show ? "收起技术详情" : "查看技术详情";
    }

    private void CancelOperation_Click(object sender, RoutedEventArgs e)
    {
        _operationCancellation?.Cancel();
        RuleCenterOperationStatusTextBlock.Text = "正在取消...";
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_operationCancellation is not null)
        {
            e.Cancel = true;
            _operationCancellation.Cancel();
            RuleCenterOperationStatusTextBlock.Text = "正在取消当前操作，完成后即可关闭。";
        }
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        _operationCancellation?.Cancel();
        _operationCancellation?.Dispose();
        if (_ownsHttpClient) _httpClient.Dispose();
        base.OnClosed(e);
    }

    private static string ComputeSha256(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
