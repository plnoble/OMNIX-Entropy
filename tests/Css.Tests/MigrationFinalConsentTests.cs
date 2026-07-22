using Css.Core.Apps;
using Css.Core.Migration;
using Css.Core.Operations;
using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationFinalConsentTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 13, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Presenter_explains_impact_without_exposing_paths()
    {
        var view = MigrationFinalConsentPresenter.Create(ReadyGate());

        view.Title.Should().Be("迁移前最后确认");
        view.SoftwareName.Should().Be("Demo");
        view.ImpactLines.Should().HaveCountGreaterThanOrEqualTo(4);
        var visible = string.Join("\n", view.ImpactLines.Append(view.SafetyText));
        visible.Should().NotContain(@"C:\Users\Private");
        visible.Should().NotContain(@"D:\Software\Private");
        visible.Should().Contain("D 盘");
        visible.Should().Contain("回滚");
        visible.Should().Contain("观察");
    }

    [Fact]
    public void Composer_requires_the_plan_acknowledgement_from_final_consent()
    {
        var gate = ReadyGate();
        var operation = gate.Operation!;
        var missingPlan = Consent(operation) with { PlanReviewedConfirmed = false };

        var refused = MigrationElevatedRequestComposer.Create(
            gate,
            missingPlan,
            "migration-final-refused",
            Now);
        var ready = MigrationElevatedRequestComposer.Create(
            gate,
            Consent(operation),
            "migration-final-ready",
            Now);

        refused.CanSubmit.Should().BeFalse();
        refused.MissingRequirements.Should().NotBeEmpty();
        ready.CanSubmit.Should().BeTrue();
        ready.Operation!.ConfirmationAccepted.Should().BeTrue();
    }

    [Fact]
    public void Final_conclusion_and_acknowledgements_have_stable_first_view_order()
    {
        var xaml = Read("src", "Css.App", "MigrationFinalConsentWindow.xaml");
        var code = Read("src", "Css.App", "MigrationFinalConsentWindow.xaml.cs");
        var planXaml = Read("src", "Css.App", "MigrationPlanWindow.xaml");
        var planCode = Read("src", "Css.App", "MigrationPlanWindow.xaml.cs");

        var ids = new[]
        {
            "MigrationFinalConsentTitleTextBlock",
            "MigrationFinalConsentSoftwareTextBlock",
            "MigrationFinalConsentSummaryTextBlock",
            "MigrationFinalConsentImpactListBox",
            "MigrationFinalConsentPlanCheckBox",
            "MigrationFinalConsentClosedCheckBox",
            "MigrationFinalConsentRollbackCheckBox",
            "MigrationFinalConsentMonitoringCheckBox",
            "MigrationFinalConsentReadinessTextBlock",
            "MigrationFinalConsentSafetyTextBlock",
            "MigrationFinalConsentCancelButton",
            "MigrationFinalConsentConfirmButton"
        };
        var previous = -1;
        foreach (var id in ids)
        {
            xaml.Should().Contain($"AutomationProperties.AutomationId=\"{id}\"");
            var current = xaml.IndexOf(id, StringComparison.Ordinal);
            current.Should().BeGreaterThan(previous);
            previous = current;
        }

        code.Should().Contain("PlanReviewedConfirmed = true");
        code.Should().Contain("AppComponentsClosedConfirmed = true");
        code.Should().Contain("RollbackAcknowledged = true");
        code.Should().Contain("MonitoringConfirmed = true");
        planXaml.Should().Contain("MigrationRequestButton");
        planCode.Should().Contain("IMigrationProductionExecutionCoordinator");
        planCode.Should().Contain("MigrationElevatedRequestComposer.Create");
        planCode.Should().Contain("ExecuteAsync(request)");
        planCode.Should().Contain("ProductionCompleted = outcome.CompletedProduction");
        planCode.Should().NotContain("migration-production-worker");
        planCode.Should().NotContain("WindowsMigrationProductionWorkerLauncher");
        planCode.Should().NotContain("RunProductionOnceAsync");
    }

    private static MigrationExecutionGateResult ReadyGate()
    {
        var operation = new OperationDescriptor
        {
            Kind = "migration.execute",
            Title = "Demo migration",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = "snapshot-final",
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = "verified final consent fixture",
            EstimatedImpactBytes = 2L * 1024 * 1024 * 1024,
            ConfirmationText = "Migrate Demo?",
            AffectedPaths = [@"C:\Users\Private\Demo"],
            Arguments = new Dictionary<string, object?>
            {
                ["destinationRoot"] = @"D:\Software\Private\Demo",
                ["rollbackManifestPath"] = @"D:\Evidence\migration.json",
                ["rollbackManifestSha256"] = new string('A', 64),
                ["snapshotEvidencePath"] = @"D:\Evidence\snapshot.json",
                ["snapshotEvidenceSha256"] = new string('B', 64),
                ["affectedProcesses"] = Array.Empty<string>(),
                ["scheduledTasks"] = Array.Empty<string>(),
                ["startupEntries"] = Array.Empty<string>(),
                ["monitorPaths"] = new[] { @"C:\Users\Private\Demo" }
            }
        };
        return new MigrationExecutionGateResult
        {
            CanRequestExecution = true,
            PrimaryButtonText = "Request migration",
            BlockingReasons = [],
            RequiredBytes = operation.EstimatedImpactBytes,
            Operation = operation
        };
    }

    private static MigrationFinalUserConsent Consent(OperationDescriptor operation) =>
        new()
        {
            ConfirmationText = operation.ConfirmationText!,
            PlanReviewedConfirmed = true,
            AppComponentsClosedConfirmed = true,
            RollbackAcknowledged = true,
            MonitoringConfirmed = true,
            ExecutionRequested = true,
            ConfirmedAtUtc = Now
        };

    private static string Read(params string[] segments) =>
        File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. segments]));

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
