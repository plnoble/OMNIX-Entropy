using Css.App;
using Css.Win32.Security;
using FluentAssertions;

namespace Css.Tests;

public sealed class ProductionExecutionReadinessPresentationTests
{
    [Theory]
    [InlineData(ProductionExecutionCapability.OfficialUninstall, "正式卸载已准备")]
    [InlineData(ProductionExecutionCapability.Migration, "正式迁移已准备")]
    public void Trusted_same_signer_package_can_prepare_but_does_not_claim_execution(
        ProductionExecutionCapability capability,
        string expectedTitle)
    {
        var view = ProductionExecutionReadinessPresenter.Create(
            Trust(
                OfficialUninstallWorkerTrustStatus.TrustedForProduction,
                AuthenticodeSignatureStatus.Trusted),
            capability);

        view.CanPrepareExecution.Should().BeTrue();
        view.Title.Should().Be(expectedTitle);
        view.StatusLabel.Should().Be("身份已确认");
        view.SafetyText.Should().Contain("执行前还会重新核对");
        view.VisibleText.Should().NotContain(@"D:\OMNIX");
        view.VisibleText.Should().NotContain(new string('A', 40));
    }

    [Theory]
    [InlineData(ProductionExecutionCapability.OfficialUninstall, "真实卸载")]
    [InlineData(ProductionExecutionCapability.Migration, "真实迁移")]
    public void Unsigned_development_package_is_preview_only_before_preparation(
        ProductionExecutionCapability capability,
        string forbiddenCapability)
    {
        var view = ProductionExecutionReadinessPresenter.Create(
            Trust(
                OfficialUninstallWorkerTrustStatus.AppNotSigned,
                AuthenticodeSignatureStatus.NotSigned),
            capability);

        view.CanPrepareExecution.Should().BeFalse();
        view.Title.Should().Be("当前只能预览方案");
        view.StatusLabel.Should().Be("开发验证版");
        view.Conclusion.Should().Contain("没有正式签名");
        view.Conclusion.Should().Contain(forbiddenCapability);
        view.SafetyText.Should().Contain("不会生成最终执行证据");
        view.SafetyText.Should().Contain("不会请求管理员确认");
    }

    [Fact]
    public void Missing_worker_fails_closed_without_paths_or_hashes()
    {
        var view = ProductionExecutionReadinessPresenter.Create(
            Trust(
                OfficialUninstallWorkerTrustStatus.WorkerUnavailable,
                AuthenticodeSignatureStatus.Missing),
            ProductionExecutionCapability.OfficialUninstall);

        view.CanPrepareExecution.Should().BeFalse();
        view.Title.Should().Be("安全组件未准备好");
        view.StatusLabel.Should().Be("已停止");
        view.Conclusion.Should().Contain("找不到");
        view.VisibleText.Should().NotContain(@"D:\OMNIX");
        view.VisibleText.Should().NotContain(new string('1', 64));
    }

    [Fact]
    public void Plan_windows_show_readiness_before_preparation_and_gate_writes()
    {
        var uninstallXaml = Read("src", "Css.App", "UninstallPlanWindow.xaml");
        var uninstallCode = Read("src", "Css.App", "UninstallPlanWindow.xaml.cs");
        var migrationXaml = Read("src", "Css.App", "MigrationPlanWindow.xaml");
        var migrationCode = Read("src", "Css.App", "MigrationPlanWindow.xaml.cs");

        AssertBefore(
            uninstallXaml,
            "UninstallPlanProductionReadinessTitleTextBlock",
            "UninstallPlanBuildFinalChecklistButton");
        AssertBefore(
            migrationXaml,
            "MigrationPlanProductionReadinessTitleTextBlock",
            "CreateRollbackManifestButton");
        uninstallCode.Should().Contain("_productionReadiness.CanPrepareExecution");
        uninstallCode.Should().Contain("UninstallPlanBuildFinalChecklistButton.IsEnabled");
        migrationCode.Should().Contain("_productionReadiness.CanPrepareExecution");
        migrationCode.Should().Contain("CreateRollbackManifestButton.IsEnabled");
    }

    [Fact]
    public void Main_and_production_coordinators_share_assessment_but_execution_rechecks()
    {
        var main = Read("src", "Css.App", "MainWindow.xaml.cs");
        var uninstallCoordinator = Read(
            "src", "Css.App", "OfficialUninstallProductionExecutionCoordinator.cs");
        var migrationCoordinator = Read(
            "src", "Css.App", "MigrationProductionExecutionCoordinator.cs");

        main.Should().Contain("CurrentPackageWorkerTrustProvider.Assess()");
        main.Should().Contain("ProductionExecutionReadinessPresenter.Create");
        uninstallCoordinator.Should().Contain("CurrentPackageWorkerTrustProvider.Assess");
        uninstallCoordinator.Should().Contain("_assessTrust()");
        migrationCoordinator.Should().Contain("CurrentPackageWorkerTrustProvider.Assess");
        migrationCoordinator.Should().Contain("_assessTrust()");
    }

    private static OfficialUninstallWorkerTrustAssessment Trust(
        OfficialUninstallWorkerTrustStatus status,
        AuthenticodeSignatureStatus signatureStatus) =>
        new()
        {
            Status = status,
            AppEvidence = new AuthenticodeSignatureEvidence
            {
                Status = signatureStatus,
                SignerThumbprint = signatureStatus == AuthenticodeSignatureStatus.Trusted
                    ? new string('A', 40)
                    : null,
                FileSha256 = new string('1', 64)
            },
            WorkerEvidence = new AuthenticodeSignatureEvidence
            {
                Status = signatureStatus,
                SignerThumbprint = signatureStatus == AuthenticodeSignatureStatus.Trusted
                    ? new string('A', 40)
                    : null,
                FileSha256 = new string('2', 64)
            },
            WorkerExecutablePath = @"D:\OMNIX\Css.Elevated.exe"
        };

    private static void AssertBefore(string source, string first, string second)
    {
        var firstIndex = source.IndexOf(first, StringComparison.Ordinal);
        var secondIndex = source.IndexOf(second, StringComparison.Ordinal);
        firstIndex.Should().BeGreaterThanOrEqualTo(0);
        secondIndex.Should().BeGreaterThan(firstIndex);
    }

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
