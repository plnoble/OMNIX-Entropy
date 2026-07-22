using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Css.App;
using Css.Core.Operations;
using Css.Core.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public sealed class OfficialUninstallFinalConsentVisualCaptureTests
{
    [Fact]
    public void Modal_consent_captures_nonblank_png_issues_ticket_and_zeroes_pixels()
    {
        var result = RunSta(() =>
        {
            var now = DateTimeOffset.UtcNow;
            var issuer = new OfficialUninstallVisualGateReceiptIssuer(
                ticketIdFactory: () => "runtime-window-ticket");
            var capture = new RecordingVisualCapture(
                new OfficialUninstallFinalConsentVisualCapture());
            var window = new OfficialUninstallFinalConsentWindow(
                OfficialUninstallFinalConsentPresenter.Create(Operation()),
                issuer,
                capture,
                () => now);

            window.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
            {
                Check(window, "OfficialUninstallFinalConsentCommandCheckBox").IsChecked = true;
                Check(window, "OfficialUninstallFinalConsentUndoCheckBox").IsChecked = true;
                Check(window, "OfficialUninstallFinalConsentPostScanCheckBox").IsChecked = true;
                FindButton(window, "OfficialUninstallFinalConsentConfirmButton")
                    .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            });

            var accepted = window.ShowDialog();
            var consumed = issuer.Consume("runtime-window-ticket", now.AddSeconds(1));
            return new CaptureResult
            {
                DialogAccepted = accepted == true,
                Consent = window.Consent,
                TicketId = window.VisualTicketId,
                Receipt = consumed.Receipt,
                PngLength = capture.PngLength,
                PngSha256 = capture.PngSha256,
                PngBufferWasZeroed = capture.PngBuffer?.All(value => value == 0) == true
            };
        });

        result.DialogAccepted.Should().BeTrue();
        result.Consent.Should().NotBeNull();
        result.TicketId.Should().Be("runtime-window-ticket");
        result.PngLength.Should().BeGreaterThan(1_000);
        result.PngSha256.Should().MatchRegex("^[0-9A-F]{64}$");
        result.PngBufferWasZeroed.Should().BeTrue();
        result.Receipt.Should().NotBeNull();
        result.Receipt!.ScreenshotSha256.Should().Be(result.PngSha256);
        result.Receipt.RecoveryTruthVisible.Should().BeTrue();
        result.Receipt.FinalConfirmationVisible.Should().BeTrue();
        result.Receipt.TechnicalDetailsCollapsedByDefault.Should().BeTrue();
        result.Receipt.NoExecutionControlDuringPreparation.Should().BeTrue();
    }

    [Fact]
    public void Capture_refuses_a_window_that_is_not_shown()
    {
        var error = RunSta(() =>
        {
            var window = new OfficialUninstallFinalConsentWindow(
                OfficialUninstallFinalConsentPresenter.Create(Operation()),
                new OfficialUninstallVisualGateReceiptIssuer(),
                new OfficialUninstallFinalConsentVisualCapture());
            var action = () => new OfficialUninstallFinalConsentVisualCapture()
                .Capture(window, DateTimeOffset.UtcNow);
            return action.Should().Throw<InvalidOperationException>().Which.Message;
        });

        error.Should().Contain("not visible");
    }

    private static T RunSta<T>(Func<T> action)
    {
        T? result = default;
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        if (!thread.Join(TimeSpan.FromSeconds(15)))
            throw new TimeoutException("The final-consent WPF test did not finish.");
        if (failure is not null)
            throw new InvalidOperationException("The final-consent WPF test failed.", failure);
        return result!;
    }

    private static CheckBox Check(Window window, string name) =>
        (CheckBox)(window.FindName(name)
            ?? throw new InvalidOperationException($"Missing checkbox {name}."));

    private static Button FindButton(Window window, string name) =>
        (Button)(window.FindName(name)
            ?? throw new InvalidOperationException($"Missing button {name}."));

    private static OperationDescriptor Operation() =>
        new()
        {
            Kind = "uninstall.official.run",
            Title = "Runtime Capture App official uninstaller",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = "runtime-capture-snapshot",
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = "runtime capture evidence",
            ConfirmationText = "Run Runtime Capture App official uninstaller?",
            Arguments = new Dictionary<string, object?>
            {
                ["softwareName"] = "Runtime Capture App"
            }
        };

    private sealed class RecordingVisualCapture(
        IOfficialUninstallFinalConsentVisualCapture inner)
        : IOfficialUninstallFinalConsentVisualCapture
    {
        public byte[]? PngBuffer { get; private set; }
        public int PngLength { get; private set; }
        public string? PngSha256 { get; private set; }

        public OfficialUninstallVisualGateIssueRequest Capture(
            OfficialUninstallFinalConsentWindow window,
            DateTimeOffset capturedAtUtc)
        {
            var request = inner.Capture(window, capturedAtUtc);
            PersistEvidenceIfRequested(request.ScreenshotPng);
            PngBuffer = request.ScreenshotPng;
            PngLength = request.ScreenshotPng.Length;
            PngSha256 = Convert.ToHexString(SHA256.HashData(request.ScreenshotPng));
            return request;
        }

        private static void PersistEvidenceIfRequested(byte[] png)
        {
            var requestedPath = Environment.GetEnvironmentVariable(
                "OMNIX_FINAL_CONSENT_RENDER_OUTPUT");
            if (string.IsNullOrWhiteSpace(requestedPath))
                return;

            var repositoryRoot = FindRepositoryRoot();
            var evidenceRoot = Path.GetFullPath(Path.Combine(repositoryRoot, ".omx"))
                .TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            var outputPath = Path.GetFullPath(requestedPath);
            if (!outputPath.StartsWith(evidenceRoot, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(Path.GetExtension(outputPath), ".png", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The visual evidence output must be a PNG inside the repository .omx directory.");
            }

            File.WriteAllBytes(outputPath, png);
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
                    return directory.FullName;
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Could not locate the repository root.");
        }
    }

    private sealed class CaptureResult
    {
        public required bool DialogAccepted { get; init; }
        public OfficialUninstallFinalUserConsent? Consent { get; init; }
        public string? TicketId { get; init; }
        public OfficialUninstallVisualGateReceipt? Receipt { get; init; }
        public required int PngLength { get; init; }
        public string? PngSha256 { get; init; }
        public required bool PngBufferWasZeroed { get; init; }
    }
}
