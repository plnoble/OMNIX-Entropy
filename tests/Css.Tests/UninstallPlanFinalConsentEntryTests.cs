using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Css.App;
using Css.Core.Apps;
using Css.Core.Recovery;
using Css.Core.Software;
using Css.Snapshot.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public sealed class UninstallPlanFinalConsentEntryTests
{
    [Fact]
    public void Ready_verified_draft_exposes_continue_but_not_a_run_control()
    {
        var result = RunSta(() =>
        {
            var window = Window();
            ApplyDraft(window, Draft(UninstallFinalConfirmationDraftStatus.ReadyForFinalConfirmation));
            return new EntryState
            {
                ContinueVisibility = Find<Button>(window, "UninstallPlanContinueFinalConsentButton").Visibility,
                ContinueEnabled = Find<Button>(window, "UninstallPlanContinueFinalConsentButton").IsEnabled,
                RequestStatusVisibility = Find<TextBlock>(window, "UninstallPlanRequestStatusTextBlock").Visibility,
                HasRunControl = window.FindName("UninstallPlanRunOfficialUninstallerButton") is not null
            };
        });

        result.ContinueVisibility.Should().Be(Visibility.Visible);
        result.ContinueEnabled.Should().BeTrue();
        result.RequestStatusVisibility.Should().Be(Visibility.Collapsed);
        result.HasRunControl.Should().BeFalse();
    }

    [Fact]
    public void Refused_draft_keeps_final_consent_entry_hidden()
    {
        var result = RunSta(() =>
        {
            var window = Window();
            ApplyDraft(window, Draft(UninstallFinalConfirmationDraftStatus.Refused));
            var button = Find<Button>(window, "UninstallPlanContinueFinalConsentButton");
            return (button.Visibility, button.IsEnabled);
        });

        result.Visibility.Should().Be(Visibility.Collapsed);
        result.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void Missing_package_readiness_keeps_a_ready_draft_fail_closed()
    {
        var result = RunSta(() =>
        {
            var window = Window(trustedPackage: false);
            ApplyDraft(window, Draft(UninstallFinalConfirmationDraftStatus.ReadyForFinalConfirmation));
            var button = Find<Button>(window, "UninstallPlanContinueFinalConsentButton");
            return (button.Visibility, button.IsEnabled);
        });

        result.Visibility.Should().Be(Visibility.Collapsed);
        result.IsEnabled.Should().BeFalse();
    }

    private static UninstallPlanWindow Window(bool trustedPackage = true)
    {
        var profile = Profile();
        return new UninstallPlanWindow(
            profile,
            UninstallPlanPresentationBuilder.Create(profile),
            [],
            WindowsRestorePointScanState.Completed,
            executionCoordinator: null,
            productionReadiness: trustedPackage ? TrustedReadiness() : null);
    }

    private static ProductionExecutionReadinessViewModel TrustedReadiness() =>
        new()
        {
            Title = "正式卸载已准备",
            StatusLabel = "身份已确认",
            Conclusion = "fixture",
            NextStep = "fixture",
            SafetyText = "fixture",
            CanPrepareExecution = true
        };

    private static void ApplyDraft(
        UninstallPlanWindow window,
        UninstallFinalConfirmationDraft draft)
    {
        var method = typeof(UninstallPlanWindow).GetMethod(
            "ApplyFinalChecklist",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException("ApplyFinalChecklist was not found.");
        method.Invoke(window, [draft]);
    }

    private static UninstallFinalConfirmationDraft Draft(
        UninstallFinalConfirmationDraftStatus status) =>
        new()
        {
            Status = status,
            Headline = "fixture",
            Summary = "fixture",
            ReadyFacts = [],
            PendingConfirmations = [],
            MissingRequirements = status == UninstallFinalConfirmationDraftStatus.Refused
                ? ["missing"]
                : [],
            CanExecuteDirectly = false,
            RecoveryEvidence = new OfficialUninstallRecoveryEvidence
            {
                Method = OfficialUninstallRecoveryMethod.ReinstallSource,
                Reference = @"D:\Installers\ExampleSetup.exe",
                CanRecoverApplication = true,
                UserDataBackupConfirmed = true
            },
            SnapshotEvidence = new OfficialUninstallSnapshotEvidence
            {
                SnapshotId = "snapshot-fixture",
                ManifestPath = @"D:\Evidence\snapshot.json",
                SoftwareName = "Example App",
                CreatedAtUtc = DateTimeOffset.UtcNow,
                Sha256 = new string('A', 64),
                CanRestoreApplication = false
            },
            SnapshotValidation = new OfficialUninstallSnapshotValidationResult
            {
                IsValid = true,
                Reasons = []
            }
        };

    private static SoftwareProfile Profile() =>
        new()
        {
            Name = "Example App",
            Publisher = "Example Publisher",
            InstallPath = @"D:\Software\Example",
            UninstallCommand = @"""D:\Software\Example\Uninstall.exe"" /remove"
        };

    private static T Find<T>(Window window, string name) where T : FrameworkElement =>
        (T)(window.FindName(name)
            ?? throw new InvalidOperationException($"Missing control {name}."));

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
            throw new TimeoutException("The uninstall-plan WPF test did not finish.");
        if (failure is not null)
            throw new InvalidOperationException("The uninstall-plan WPF test failed.", failure);
        return result!;
    }

    private sealed class EntryState
    {
        public required Visibility ContinueVisibility { get; init; }
        public required bool ContinueEnabled { get; init; }
        public required Visibility RequestStatusVisibility { get; init; }
        public required bool HasRunControl { get; init; }
    }
}
