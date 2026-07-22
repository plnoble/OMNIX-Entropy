using System.Security.Cryptography;
using System.Windows;
using Css.Core.Uninstall;

namespace Css.App;

public partial class OfficialUninstallFinalConsentWindow : Window
{
    private readonly OfficialUninstallFinalConsentViewModel _viewModel;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly OfficialUninstallVisualGateReceiptIssuer _visualGateIssuer;
    private readonly IOfficialUninstallFinalConsentVisualCapture _visualCapture;

    public OfficialUninstallFinalConsentWindow(
        OfficialUninstallFinalConsentViewModel viewModel,
        OfficialUninstallVisualGateReceiptIssuer visualGateIssuer,
        IOfficialUninstallFinalConsentVisualCapture visualCapture,
        Func<DateTimeOffset>? utcNow = null)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _visualGateIssuer = visualGateIssuer
            ?? throw new ArgumentNullException(nameof(visualGateIssuer));
        _visualCapture = visualCapture ?? throw new ArgumentNullException(nameof(visualCapture));
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        InitializeComponent();
        DataContext = viewModel;
        UpdateReadiness();
    }

    public OfficialUninstallFinalUserConsent? Consent { get; private set; }
    public string? VisualTicketId { get; private set; }

    private void Acknowledgement_Changed(object sender, RoutedEventArgs e)
    {
        UpdateReadiness();
    }

    private void UpdateReadiness()
    {
        var confirmedCount = new[]
        {
            OfficialUninstallFinalConsentCommandCheckBox.IsChecked == true,
            OfficialUninstallFinalConsentUndoCheckBox.IsChecked == true,
            OfficialUninstallFinalConsentPostScanCheckBox.IsChecked == true
        }.Count(value => value);
        var missingCount = 3 - confirmedCount;
        OfficialUninstallFinalConsentConfirmButton.IsEnabled = missingCount == 0;
        OfficialUninstallFinalConsentReadinessTextBlock.Text = missingCount == 0
            ? "\u5df2\u786e\u8ba4 3 \u9879\uff0c\u53ef\u4ee5\u7ee7\u7eed"
            : $"\u8fd8\u9700\u786e\u8ba4 {missingCount} \u9879";
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        Consent = null;
        VisualTicketId = null;
        var now = _utcNow();
        var result = OfficialUninstallFinalConsentBuilder.Create(
            _viewModel,
            new OfficialUninstallFinalConsentSelection
            {
                OfficialCommandConfirmed = OfficialUninstallFinalConsentCommandCheckBox.IsChecked == true,
                AppsClosedConfirmed = OfficialUninstallFinalConsentCommandCheckBox.IsChecked == true,
                NoAutomaticUndoAcknowledged = OfficialUninstallFinalConsentUndoCheckBox.IsChecked == true,
                PostUninstallRescanConfirmed = OfficialUninstallFinalConsentPostScanCheckBox.IsChecked == true
            },
            now);

        if (!result.CanSubmit || result.Consent is null)
        {
            Consent = null;
            OfficialUninstallFinalConsentReadinessTextBlock.Text =
                result.MissingRequirements.FirstOrDefault()
                ?? "\u786e\u8ba4\u8fd8\u4e0d\u5b8c\u6574\u3002";
            return;
        }

        OfficialUninstallVisualGateIssueRequest capture;
        try
        {
            capture = _visualCapture.Capture(this, now);
        }
        catch
        {
            OfficialUninstallFinalConsentReadinessTextBlock.Text =
                "\u65e0\u6cd5\u9a8c\u8bc1\u5f53\u524d\u786e\u8ba4\u754c\u9762\uff0c\u672c\u6b21\u4e0d\u4f1a\u7ee7\u7eed\u3002";
            return;
        }

        OfficialUninstallVisualGateIssueResult issued;
        try
        {
            issued = _visualGateIssuer.Issue(capture, now);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(capture.ScreenshotPng);
        }
        if (issued.Status != OfficialUninstallVisualGateIssueStatus.Issued
            || string.IsNullOrWhiteSpace(issued.TicketId))
        {
            OfficialUninstallFinalConsentReadinessTextBlock.Text =
                issued.MissingRequirements.FirstOrDefault()
                ?? "\u5f53\u524d\u786e\u8ba4\u754c\u9762\u6ca1\u6709\u901a\u8fc7\u5b89\u5168\u9a8c\u8bc1\u3002";
            return;
        }

        Consent = result.Consent;
        VisualTicketId = issued.TicketId;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Consent = null;
        VisualTicketId = null;
        DialogResult = false;
    }
}
