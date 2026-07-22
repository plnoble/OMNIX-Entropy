using System.Security.Cryptography;
using System.Text;
using Css.InstallGuard.Installers;
using Css.Win32.Security;
using FluentAssertions;

namespace Css.Tests;

public sealed class InstallerPackageInspectionTests : IDisposable
{
    private readonly string _fixtureRoot = Path.Combine(
        Path.GetTempPath(),
        "omnix-installer-fixture-" + Guid.NewGuid().ToString("N"));

    public InstallerPackageInspectionTests() => Directory.CreateDirectory(_fixtureRoot);

    [Fact]
    public void Package_path_policy_only_accepts_existing_files_on_fixed_local_non_reparse_paths()
    {
        var packagePath = Path.Combine(_fixtureRoot, "local.exe");
        File.WriteAllText(packagePath, "fixture");

        InstallerPackagePathPolicy.TryResolveFixedLocalPath(packagePath, out var resolved)
            .Should().BeTrue();
        resolved.Should().Be(Path.GetFullPath(packagePath));
        InstallerPackagePathPolicy.IsExistingFileWithoutReparsePoints(resolved)
            .Should().BeTrue();
        InstallerPackagePathPolicy.TryResolveFixedLocalPath("relative.exe", out _)
            .Should().BeFalse();
        InstallerPackagePathPolicy.TryResolveFixedLocalPath(
                @"\\server\share\setup.exe",
                out _)
            .Should().BeFalse();
        InstallerPackagePathPolicy.TryResolveFixedLocalPath(
                packagePath + ":stream",
                out _)
            .Should().BeFalse();
    }

    [Fact]
    public void Msi_extension_is_high_confidence_and_bound_to_hash_metadata()
    {
        var path = WritePackage("ExampleTool.msi", "fixture-msi");
        var inspector = new WindowsInstallerPackageInspector(
            new HashingFakeVerifier(AuthenticodeSignatureStatus.Trusted));

        var evidence = inspector.Inspect(path);

        evidence.Status.Should().Be(InstallerPackageInspectionStatus.Ready);
        evidence.DetectedKind.Should().Be(InstallerKind.Msi);
        evidence.KindConfidence.Should().Be(InstallerKindConfidence.High);
        evidence.SignatureStatus.Should().Be(AuthenticodeSignatureStatus.Trusted);
        evidence.HasStableIdentity.Should().BeTrue();
        evidence.Sha256.Should().HaveLength(64);
        evidence.LengthBytes.Should().Be(new FileInfo(path).Length);
        evidence.LastWriteUtc.Should().NotBeNull();
    }

    [Theory]
    [InlineData("PhotoTool.exe", "Inno Setup Setup Data", InstallerKind.InnoSetup)]
    [InlineData("PhotoTool.exe", "Nullsoft.NSIS", InstallerKind.Nsis)]
    [InlineData("PhotoTool.exe", "WixBundle", InstallerKind.Burn)]
    public void Binary_markers_override_filename_hints(
        string fileName,
        string marker,
        InstallerKind expected)
    {
        var path = WritePackage(fileName, "prefix\0" + marker + "\0suffix");
        var inspector = new WindowsInstallerPackageInspector(
            new HashingFakeVerifier(AuthenticodeSignatureStatus.Trusted));

        var evidence = inspector.Inspect(path);

        evidence.DetectedKind.Should().Be(expected);
        evidence.KindConfidence.Should().Be(InstallerKindConfidence.High);
        evidence.KindEvidence.Should().ContainSingle();
    }

    [Fact]
    public void Filename_hint_without_binary_marker_stays_generic_exe_and_gets_no_arguments()
    {
        var path = WritePackage("PhotoTool-inno-setup.exe", "ordinary executable fixture");
        var inspector = new WindowsInstallerPackageInspector(
            new HashingFakeVerifier(AuthenticodeSignatureStatus.Trusted));

        var evidence = inspector.Inspect(path);
        var analysis = InstallerAnalyzer.AnalyzePackage(evidence);
        var capability = InstallerRoutingCapabilityPolicy.Evaluate(analysis, evidence);

        evidence.DetectedKind.Should().Be(InstallerKind.Exe);
        evidence.KindConfidence.Should().Be(InstallerKindConfidence.Low);
        analysis.Kind.Should().Be(InstallerKind.Exe);
        analysis.CandidateInstallArguments.Should().BeEmpty();
        capability.Mode.Should().Be(InstallerRoutingCapabilityMode.GuidedInteractiveRoute);
        capability.InteractiveArguments.Should().BeEmpty();
        capability.CanApplyTargetAutomatically.Should().BeFalse();
    }

    [Fact]
    public void Conflicting_binary_markers_fail_closed_to_guided_exe()
    {
        var path = WritePackage(
            "Conflicting.exe",
            "Inno Setup Setup Data\0Nullsoft.NSIS");
        var inspector = new WindowsInstallerPackageInspector(
            new HashingFakeVerifier(AuthenticodeSignatureStatus.Trusted));

        var evidence = inspector.Inspect(path);
        var capability = InstallerRoutingCapabilityPolicy.Evaluate(
            InstallerAnalyzer.AnalyzePackage(evidence),
            evidence);

        evidence.DetectedKind.Should().Be(InstallerKind.Exe);
        evidence.KindConfidence.Should().Be(InstallerKindConfidence.Low);
        capability.Mode.Should().Be(InstallerRoutingCapabilityMode.GuidedInteractiveRoute);
        capability.InteractiveArguments.Should().BeEmpty();
    }

    [Theory]
    [InlineData("ConfirmedInno.exe", "Inno Setup Setup Data", InstallerKind.InnoSetup, "/DIR=")]
    [InlineData("ConfirmedNsis.exe", "Nullsoft.NSIS", InstallerKind.Nsis, "/D=")]
    public void Trusted_high_confidence_inno_and_nsis_can_only_request_interactive_routing(
        string fileName,
        string marker,
        InstallerKind expected,
        string argumentPrefix)
    {
        var path = WritePackage(fileName, marker);
        var inspector = new WindowsInstallerPackageInspector(
            new HashingFakeVerifier(AuthenticodeSignatureStatus.Trusted));
        var evidence = inspector.Inspect(path);
        var analysis = InstallerAnalyzer.AnalyzePackage(evidence);

        var capability = InstallerRoutingCapabilityPolicy.Evaluate(analysis, evidence);

        evidence.DetectedKind.Should().Be(expected);
        capability.Mode.Should().Be(InstallerRoutingCapabilityMode.AutomaticInteractiveRoute);
        capability.CanRequestInstallerLaunch.Should().BeTrue();
        capability.CanApplyTargetAutomatically.Should().BeTrue();
        capability.InteractiveArguments.Should().ContainSingle()
            .Which.Should().StartWith(argumentPrefix);
        capability.SafetyText.Should().Contain("不会使用静默安装参数");
        capability.RequiresBeforeSnapshot.Should().BeTrue();
        capability.RequiresFinalConfirmation.Should().BeTrue();
    }

    [Theory]
    [InlineData("ExampleTool.msi", "fixture", InstallerRoutingCapabilityMode.GuidedInteractiveRoute, true)]
    [InlineData("Bundle.exe", "BurnBootstrapperApplication", InstallerRoutingCapabilityMode.GuidedInteractiveRoute, true)]
    [InlineData("Store.msix", "fixture", InstallerRoutingCapabilityMode.WindowsManagedStorage, false)]
    public void Msi_burn_and_msix_never_receive_guessed_directory_arguments(
        string fileName,
        string content,
        InstallerRoutingCapabilityMode expectedMode,
        bool canRequestLaunch)
    {
        var path = WritePackage(fileName, content);
        var inspector = new WindowsInstallerPackageInspector(
            new HashingFakeVerifier(AuthenticodeSignatureStatus.Trusted));
        var evidence = inspector.Inspect(path);

        var capability = InstallerRoutingCapabilityPolicy.Evaluate(
            InstallerAnalyzer.AnalyzePackage(evidence),
            evidence);

        capability.Mode.Should().Be(expectedMode);
        capability.CanRequestInstallerLaunch.Should().Be(canRequestLaunch);
        capability.CanApplyTargetAutomatically.Should().BeFalse();
        capability.InteractiveArguments.Should().BeEmpty();
        capability.SettingsShortcutId.Should().Be(
            expectedMode == InstallerRoutingCapabilityMode.WindowsManagedStorage
                ? InstallerRoutingCapabilityPolicy.WindowsManagedStorageShortcutId
                : null);
    }

    [Theory]
    [InlineData(AuthenticodeSignatureStatus.NotSigned)]
    [InlineData(AuthenticodeSignatureStatus.Invalid)]
    [InlineData(AuthenticodeSignatureStatus.Untrusted)]
    [InlineData(AuthenticodeSignatureStatus.ProbeFailed)]
    public void Anything_other_than_a_windows_trusted_signature_is_refused(
        AuthenticodeSignatureStatus signatureStatus)
    {
        var path = WritePackage("Unsigned.exe", "Inno Setup Setup Data");
        var inspector = new WindowsInstallerPackageInspector(
            new HashingFakeVerifier(signatureStatus));
        var evidence = inspector.Inspect(path);

        var capability = InstallerRoutingCapabilityPolicy.Evaluate(
            InstallerAnalyzer.AnalyzePackage(evidence),
            evidence);

        capability.Mode.Should().Be(InstallerRoutingCapabilityMode.Refused);
        capability.CanRequestInstallerLaunch.Should().BeFalse();
        capability.InteractiveArguments.Should().BeEmpty();
        capability.SettingsShortcutId.Should().BeNull();
    }

    [Fact]
    public void A_hash_observed_by_the_signature_probe_must_match_the_inspected_bytes()
    {
        var path = WritePackage("Changed.exe", "Inno Setup Setup Data");
        var inspector = new WindowsInstallerPackageInspector(new FixedHashVerifier(
            AuthenticodeSignatureStatus.Trusted,
            new string('A', 64)));

        var evidence = inspector.Inspect(path);

        evidence.Status.Should().Be(InstallerPackageInspectionStatus.ProbeFailed);
        evidence.HasStableIdentity.Should().BeFalse();
    }

    [Fact]
    public void Missing_and_unsupported_files_cannot_produce_stable_evidence()
    {
        var inspector = new WindowsInstallerPackageInspector(
            new HashingFakeVerifier(AuthenticodeSignatureStatus.Trusted));
        var unsupported = WritePackage("Archive.zip", "fixture");

        var missingEvidence = inspector.Inspect(Path.Combine(_fixtureRoot, "missing.exe"));
        var unsupportedEvidence = inspector.Inspect(unsupported);

        missingEvidence.Status.Should().Be(InstallerPackageInspectionStatus.Missing);
        unsupportedEvidence.Status.Should().Be(InstallerPackageInspectionStatus.Unsupported);
        missingEvidence.HasStableIdentity.Should().BeFalse();
        unsupportedEvidence.HasStableIdentity.Should().BeFalse();
    }

    [Fact]
    public void Capability_refuses_when_analysis_and_package_paths_do_not_match()
    {
        var path = WritePackage("Real.exe", "Inno Setup Setup Data");
        var inspector = new WindowsInstallerPackageInspector(
            new HashingFakeVerifier(AuthenticodeSignatureStatus.Trusted));
        var evidence = inspector.Inspect(path);
        var otherPath = Path.Combine(_fixtureRoot, "Other.exe");
        var analysis = InstallerAnalyzer.AnalyzePath(otherPath);

        var capability = InstallerRoutingCapabilityPolicy.Evaluate(analysis, evidence);

        capability.Mode.Should().Be(InstallerRoutingCapabilityMode.Refused);
        capability.CanRequestInstallerLaunch.Should().BeFalse();
    }

    [Fact]
    public void Unknown_type_is_refused_even_when_other_package_evidence_is_stable_and_trusted()
    {
        var path = WritePackage("Unknown.exe", "fixture");
        var info = new FileInfo(path);
        var evidence = new InstallerPackageEvidence
        {
            Status = InstallerPackageInspectionStatus.Ready,
            PackagePath = path,
            FileName = info.Name,
            LengthBytes = info.Length,
            LastWriteUtc = new DateTimeOffset(info.LastWriteTimeUtc, TimeSpan.Zero),
            Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
            SignatureStatus = AuthenticodeSignatureStatus.Trusted,
            SignerSubject = "CN=Fixture Publisher",
            DetectedKind = InstallerKind.Unknown,
            KindConfidence = InstallerKindConfidence.Unknown
        };

        var capability = InstallerRoutingCapabilityPolicy.Evaluate(
            InstallerAnalyzer.AnalyzePackage(evidence),
            evidence);

        capability.Mode.Should().Be(InstallerRoutingCapabilityMode.Refused);
        capability.CanRequestInstallerLaunch.Should().BeFalse();
        capability.InteractiveArguments.Should().BeEmpty();
    }

    public void Dispose()
    {
        if (Directory.Exists(_fixtureRoot))
            Directory.Delete(_fixtureRoot, recursive: true);
    }

    private string WritePackage(string fileName, string content)
    {
        var path = Path.Combine(_fixtureRoot, fileName);
        File.WriteAllBytes(path, Encoding.UTF8.GetBytes(content));
        return path;
    }

    private sealed class HashingFakeVerifier(AuthenticodeSignatureStatus status)
        : IAuthenticodeSignatureVerifier
    {
        public AuthenticodeSignatureEvidence Verify(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            return new AuthenticodeSignatureEvidence
            {
                Status = status,
                SignerSubject = status == AuthenticodeSignatureStatus.Trusted
                    ? "CN=Fixture Publisher"
                    : null,
                SignerThumbprint = status == AuthenticodeSignatureStatus.Trusted
                    ? "AA11"
                    : null,
                FileSha256 = Convert.ToHexString(SHA256.HashData(stream))
            };
        }
    }

    private sealed class FixedHashVerifier(
        AuthenticodeSignatureStatus status,
        string hash) : IAuthenticodeSignatureVerifier
    {
        public AuthenticodeSignatureEvidence Verify(string filePath) =>
            new()
            {
                Status = status,
                SignerSubject = "CN=Fixture Publisher",
                SignerThumbprint = "AA11",
                FileSha256 = hash
            };
    }
}
