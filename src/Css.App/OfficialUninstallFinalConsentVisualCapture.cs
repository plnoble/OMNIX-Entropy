using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Css.Core.Uninstall;

namespace Css.App;

public interface IOfficialUninstallFinalConsentVisualCapture
{
    OfficialUninstallVisualGateIssueRequest Capture(
        OfficialUninstallFinalConsentWindow window,
        DateTimeOffset capturedAtUtc);
}

public sealed class OfficialUninstallFinalConsentVisualCapture
    : IOfficialUninstallFinalConsentVisualCapture
{
    public OfficialUninstallVisualGateIssueRequest Capture(
        OfficialUninstallFinalConsentWindow window,
        DateTimeOffset capturedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (!window.Dispatcher.CheckAccess())
            throw new InvalidOperationException("Final-consent capture must run on the UI thread.");
        if (!window.IsVisible || window.WindowState == WindowState.Minimized)
            throw new InvalidOperationException("The final-consent window is not visible.");

        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
        var recoveryTruthVisible = IsFullyVisible(
            window.OfficialUninstallFinalConsentImpactListBox,
            window)
            && window.OfficialUninstallFinalConsentImpactListBox.Items.Count >= 3;
        var finalConfirmationVisible = new FrameworkElement[]
        {
            window.OfficialUninstallFinalConsentCommandCheckBox,
            window.OfficialUninstallFinalConsentUndoCheckBox,
            window.OfficialUninstallFinalConsentPostScanCheckBox,
            window.OfficialUninstallFinalConsentReadinessTextBlock,
            window.OfficialUninstallFinalConsentSafetyTextBlock
        }.All(element => IsFullyVisible(element, window));
        var technicalDetails = window.FindName(
            "OfficialUninstallTechnicalDetailsExpander") as FrameworkElement;
        var technicalDetailsCollapsed = technicalDetails is null || !technicalDetails.IsVisible;
        var noRunControl = window.FindName("OfficialUninstallRunButton") is null;

        return new OfficialUninstallVisualGateIssueRequest
        {
            UiContractVersion = OfficialUninstallElevatedRequestComposer.RequiredUiContractVersion,
            ScreenshotPng = RenderVisibleContent(window),
            CapturedAtUtc = capturedAtUtc.ToUniversalTime(),
            RecoveryTruthVisible = recoveryTruthVisible,
            FinalConfirmationVisible = finalConfirmationVisible,
            TechnicalDetailsCollapsedByDefault = technicalDetailsCollapsed,
            NoExecutionControlDuringPreparation = noRunControl
        };
    }

    private static byte[] RenderVisibleContent(Window window)
    {
        if (window.Content is not FrameworkElement content
            || content.ActualWidth <= 0
            || content.ActualHeight <= 0)
        {
            throw new InvalidOperationException("The final-consent content has no renderable size.");
        }

        var dpi = VisualTreeHelper.GetDpi(content);
        var pixelWidth = Math.Max(1, (int)Math.Ceiling(content.ActualWidth * dpi.DpiScaleX));
        var pixelHeight = Math.Max(1, (int)Math.Ceiling(content.ActualHeight * dpi.DpiScaleY));
        var bitmap = new RenderTargetBitmap(
            pixelWidth,
            pixelHeight,
            dpi.PixelsPerInchX,
            dpi.PixelsPerInchY,
            PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            context.DrawRectangle(
                new VisualBrush(content) { Stretch = Stretch.Fill },
                null,
                new Rect(0, 0, content.ActualWidth, content.ActualHeight));
        }
        bitmap.Render(visual);
        EnsureNonBlank(bitmap, pixelWidth, pixelHeight);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    private static void EnsureNonBlank(
        RenderTargetBitmap bitmap,
        int pixelWidth,
        int pixelHeight)
    {
        var stride = checked(pixelWidth * 4);
        var pixels = new byte[checked(stride * pixelHeight)];
        bitmap.CopyPixels(pixels, stride, 0);

        uint? firstOpaqueColor = null;
        var hasDifferentOpaqueColor = false;
        for (var index = 0; index < pixels.Length; index += 4)
        {
            if (pixels[index + 3] == 0)
                continue;
            var color = BitConverter.ToUInt32(pixels, index);
            if (firstOpaqueColor is null)
                firstOpaqueColor = color;
            else if (color != firstOpaqueColor.Value)
            {
                hasDifferentOpaqueColor = true;
                break;
            }
        }

        if (firstOpaqueColor is null || !hasDifferentOpaqueColor)
            throw new InvalidOperationException("The final-consent capture is blank.");
    }

    private static bool IsFullyVisible(FrameworkElement element, Window window)
    {
        if (!element.IsVisible || element.ActualWidth <= 0 || element.ActualHeight <= 0)
            return false;
        try
        {
            var origin = element.TransformToAncestor(window).Transform(new Point(0, 0));
            var bounds = new Rect(origin, new Size(element.ActualWidth, element.ActualHeight));
            var viewport = new Rect(0, 0, window.ActualWidth, window.ActualHeight);
            return viewport.Contains(bounds.TopLeft) && viewport.Contains(bounds.BottomRight);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
