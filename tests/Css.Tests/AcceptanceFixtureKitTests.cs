using Css.AcceptanceFixtures;
using Css.Core.Apps;
using Css.Core.Software;
using Css.Rules;
using Css.Scanner.Disk;
using Css.Scanner.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AcceptanceFixtureKitTests
{
    private const string SessionId = "01234567-89ab-cdef-8123-456789abcdef";

    [Theory]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("01234567-89AB-CDEF-8123-456789ABCDEF")]
    [InlineData("{01234567-89ab-cdef-8123-456789abcdef}")]
    public void Layout_requires_a_canonical_lowercase_guid(string sessionId)
    {
        var action = () => CreateLayout(sessionId);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Layout_derives_only_session_scoped_roots_and_exact_registry_names()
    {
        var layout = CreateLayout();

        layout.CSessionRoot.Should().Be($@"C:\OMNIX-Entropy-Acceptance\{SessionId}");
        layout.LocalDataRoot.Should().Be(@"C:\Users\Fixture\AppData\Local\OMNIX Acceptance Uninstall Fixture 01234567");
        layout.TempRoot.Should().Be(@"C:\Temp");
        layout.MigrationDestinationInstallRoot.Should().StartWith(@"D:\Software\OMNIX Acceptance Migration Fixture ")
            .And.EndWith(@"\Install");
        layout.UninstallRegistryKeyName.Should().Be("OMNIX-Entropy-Acceptance-Uninstall-01234567");
        layout.MigrationRegistryKeyName.Should().Be("OMNIX-Entropy-Acceptance-Migration-01234567");
        layout.StartupValueName.Should().Be("OMNIX-Entropy-Acceptance-Startup-01234567");
        layout.FailureLockFile.Should().StartWith(layout.MigrationInstallRoot + "\\");
    }

    [Fact]
    public void Provision_refuses_any_collision_before_the_first_write()
    {
        var layout = CreateLayout();
        var files = new FakeFixtureFileSystem();
        var registry = new FakeFixtureRegistry();
        files.ExistingPaths.Add(layout.TempRoot);
        var service = new AcceptanceFixtureOperator(files, registry, () => FixedNow);

        var action = () => service.Provision(
            layout,
            Payload(files),
            AcceptanceFixtureAuthority.RequiredAttestation);

        action.Should().Throw<InvalidOperationException>().WithMessage("*collision*");
        files.MutationCount.Should().Be(0);
        registry.MutationCount.Should().Be(0);
    }

    [Fact]
    public void Provision_compensates_a_root_when_its_owner_marker_write_fails()
    {
        var layout = CreateLayout();
        var files = new FakeFixtureFileSystem
        {
            FailWritePath = layout.CSessionOwnershipMarker
        };
        var registry = new FakeFixtureRegistry();
        var service = new AcceptanceFixtureOperator(files, registry, () => FixedNow);

        var action = () => service.Provision(
            layout,
            Payload(files),
            AcceptanceFixtureAuthority.RequiredAttestation);

        action.Should().Throw<IOException>();
        files.ExistingPaths.Should().NotContain(layout.CSessionRoot);
        files.DeletedTrees.Should().ContainSingle().Which.Should().Be(layout.CSessionRoot);
        registry.MutationCount.Should().Be(0);
    }

    [Fact]
    public void Provision_creates_two_owned_apps_cache_temp_and_exact_hkcu_records()
    {
        var layout = CreateLayout();
        var files = new FakeFixtureFileSystem();
        var registry = new FakeFixtureRegistry();
        var service = new AcceptanceFixtureOperator(files, registry, () => FixedNow);

        var result = service.Provision(
            layout,
            Payload(files),
            AcceptanceFixtureAuthority.RequiredAttestation);

        result.SessionId.Should().Be(SessionId);
        result.CreatedPaths.Should().Contain([
            layout.CSessionRoot,
            layout.LocalDataRoot,
            layout.TempRoot
        ]);
        files.TextFiles[layout.CSessionOwnershipMarker].Should().Contain(SessionId);
        files.TextFiles[layout.UninstallInstallOwnershipMarker].Should().Contain(SessionId);
        files.TextFiles[layout.MigrationInstallOwnershipMarker].Should().Contain(SessionId);
        files.TextFiles[layout.CacheFixtureFile].Should().Contain("cache-fixture");
        files.TextFiles[layout.TempFixtureFile].Should().Contain("cleanup-fixture");
        files.TextFiles[layout.FailureLockFile].Should().Contain("migration-lock-fixture");

        registry.UninstallRecords.Keys.Should().BeEquivalentTo([
            layout.UninstallRegistryKeyName,
            layout.MigrationRegistryKeyName
        ]);
        registry.UninstallRecords[layout.UninstallRegistryKeyName].SessionId.Should().Be(SessionId);
        registry.UninstallRecords[layout.UninstallRegistryKeyName].UninstallCommand
            .Should().Contain("uninstall").And.Contain(SessionId);
        registry.StartupValues.Should().ContainKey(layout.StartupValueName);
        registry.StartupValues[layout.StartupValueName].Should().Contain("status");
    }

    [Fact]
    public void Provisioned_cache_is_attributed_by_the_real_software_profile_builder()
    {
        var layout = CreateLayout();
        var files = new FakeFixtureFileSystem();
        var registry = new FakeFixtureRegistry();
        var service = new AcceptanceFixtureOperator(files, registry, () => FixedNow);
        service.Provision(layout, Payload(files), AcceptanceFixtureAuthority.RequiredAttestation);
        var record = registry.UninstallRecords[layout.UninstallRegistryKeyName];

        var startup = new StartupEntry(
            layout.StartupValueName,
            layout.ExpectedStartupCommand,
            @"HKCU64\Software\Microsoft\Windows\CurrentVersion\Run");
        var profile = SoftwareInventoryBuilder.Build(
            [new InstalledSoftwareRecord(
                record.DisplayName,
                record.Publisher,
                record.InstallLocation,
                record.UninstallCommand,
                record.DisplayIcon,
                @"HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\" + record.KeyName)],
            [startup],
            [],
            [],
            userDataRoots: [@"C:\Users\Fixture\AppData\Local"],
            pathExists: files.Exists,
            cacheSizeResolver: path => path.Equals(
                layout.CacheRoot,
                StringComparison.OrdinalIgnoreCase) ? 4096 : 0).Single();

        profile.DataPaths.Should().Contain(layout.LocalDataRoot);
        profile.CachePaths.Should().Contain(layout.CacheRoot);
        profile.CacheSizeBytes.Should().Be(4096);
        profile.CDriveWritePaths.Should().Contain([
            layout.LocalDataRoot,
            layout.CacheRoot
        ]);
        profile.StartupEntries.Should().ContainSingle()
            .Which.Should().Be(layout.StartupValueName);

        OfficialUninstallCommandTrustEvaluator.Evaluate(
                layout.UninstallExecutable,
                layout.UninstallInstallRoot,
                layout.ExpectedUninstallCommand(AcceptanceFixtureRole.Uninstall))
            .Decision.Should().Be(OfficialUninstallCommandTrustDecision.Trusted);
    }

    [Fact]
    public void Provisioned_cleanup_root_produces_a_low_risk_reversible_real_recommendation()
    {
        var layout = CreateLayout();
        var rules = new ScanRuleLoader().Load(Path.Combine(
            FindRepositoryRoot(),
            "src",
            "Css.App",
            "rules.scan.json"));
        var node = new CategoryNode
        {
            Name = "Temp",
            Path = layout.TempRoot,
            SizeBytes = 8192
        };
        new CategoryClassifier(rules).Classify([node], @"C:\");

        var recommendation = DiskRecommendationBuilder.Build(new DriveScanResult
        {
            Drive = @"C:\",
            TotalBytes = 100_000,
            FreeBytes = 50_000,
            TopLevel = [node]
        }).Single(item => item.Operation is not null);

        node.Category.Should().Be(UsageCategory.Temp);
        recommendation.Risk.Should().Be(Css.Core.Operations.RiskLevel.Low);
        recommendation.Reversibility.Should().Be(Css.Core.Recommendations.ReversibilityLevel.Reversible);
        recommendation.Operation!.RollbackRequired.Should().BeTrue();
        recommendation.Operation.AffectedPaths.Should().Equal(layout.TempRoot);
    }

    [Fact]
    public void Uninstall_refuses_registry_drift_and_leaves_everything_unchanged()
    {
        var layout = CreateLayout();
        var files = new FakeFixtureFileSystem();
        var registry = new FakeFixtureRegistry();
        var service = new AcceptanceFixtureOperator(files, registry, () => FixedNow);
        service.Provision(layout, Payload(files), AcceptanceFixtureAuthority.RequiredAttestation);
        registry.UninstallRecords[layout.UninstallRegistryKeyName] =
            registry.UninstallRecords[layout.UninstallRegistryKeyName] with { SessionId = Guid.NewGuid().ToString("D") };
        var fileMutations = files.MutationCount;
        var registryMutations = registry.MutationCount;

        var action = () => service.Uninstall(
            layout,
            AcceptanceFixtureRole.Uninstall,
            AcceptanceFixtureAuthority.RequiredAttestation);

        action.Should().Throw<InvalidOperationException>().WithMessage("*ownership*");
        files.MutationCount.Should().Be(fileMutations);
        registry.MutationCount.Should().Be(registryMutations);
    }

    [Fact]
    public void Uninstall_removes_only_exact_registration_and_leaves_owned_residue()
    {
        var layout = CreateLayout();
        var files = new FakeFixtureFileSystem();
        var registry = new FakeFixtureRegistry();
        var service = new AcceptanceFixtureOperator(files, registry, () => FixedNow);
        service.Provision(layout, Payload(files), AcceptanceFixtureAuthority.RequiredAttestation);

        var result = service.Uninstall(
            layout,
            AcceptanceFixtureRole.Uninstall,
            AcceptanceFixtureAuthority.RequiredAttestation);

        result.ResidueLeftForReview.Should().BeTrue();
        registry.UninstallRecords.Should().NotContainKey(layout.UninstallRegistryKeyName)
            .And.ContainKey(layout.MigrationRegistryKeyName);
        registry.StartupValues.Should().NotContainKey(layout.StartupValueName);
        files.ExistingPaths.Should().Contain(layout.UninstallInstallRoot);
        files.TextFiles[layout.UninstallResidueMarker].Should().Contain(SessionId);
        files.DeletedTrees.Should().BeEmpty();
    }

    [Fact]
    public void Failure_lock_requires_exact_owned_target_and_attestation()
    {
        var layout = CreateLayout();

        AcceptanceFixtureAuthority.ValidateFailureLockTarget(
                layout,
                layout.FailureLockFile,
                AcceptanceFixtureAuthority.RequiredAttestation)
            .Should().Be(layout.FailureLockFile);

        var wrongPath = Path.Combine(layout.MigrationInstallRoot, "other.bin");
        var wrongPathAction = () => AcceptanceFixtureAuthority.ValidateFailureLockTarget(
            layout,
            wrongPath,
            AcceptanceFixtureAuthority.RequiredAttestation);
        var wrongAttestationAction = () => AcceptanceFixtureAuthority.ValidateFailureLockTarget(
            layout,
            layout.FailureLockFile,
            "wrong");
        wrongPathAction.Should().Throw<InvalidOperationException>();
        wrongAttestationAction.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reset_refuses_one_owner_mismatch_before_any_delete_or_registry_change()
    {
        var layout = CreateLayout();
        var files = new FakeFixtureFileSystem();
        var registry = new FakeFixtureRegistry();
        var service = new AcceptanceFixtureOperator(files, registry, () => FixedNow);
        service.Provision(layout, Payload(files), AcceptanceFixtureAuthority.RequiredAttestation);
        files.TextFiles[layout.LocalDataOwnershipMarker] = "{\"sessionId\":\"wrong\"}";
        var fileMutations = files.MutationCount;
        var registryMutations = registry.MutationCount;

        var action = () => service.Reset(
            layout,
            AcceptanceFixtureAuthority.RequiredAttestation);

        action.Should().Throw<InvalidOperationException>().WithMessage("*ownership*");
        files.MutationCount.Should().Be(fileMutations);
        registry.MutationCount.Should().Be(registryMutations);
        files.DeletedTrees.Should().BeEmpty();
    }

    [Fact]
    public void Reset_deletes_only_verified_roots_with_no_follow_semantics()
    {
        var layout = CreateLayout();
        var files = new FakeFixtureFileSystem();
        var registry = new FakeFixtureRegistry();
        var service = new AcceptanceFixtureOperator(files, registry, () => FixedNow);
        service.Provision(layout, Payload(files), AcceptanceFixtureAuthority.RequiredAttestation);
        files.CreateOwnedDirectoryForTest(
            layout.MigrationDestinationInstallRoot,
            layout.MigrationDestinationOwnershipMarker,
            SessionId);

        var result = service.Reset(
            layout,
            AcceptanceFixtureAuthority.RequiredAttestation);

        result.RemovedRoots.Should().BeEquivalentTo([
            layout.CSessionRoot,
            layout.LocalDataRoot,
            layout.TempRoot,
            layout.MigrationDestinationInstallRoot
        ]);
        files.DeletedTrees.Should().BeEquivalentTo(result.RemovedRoots);
        files.DeleteTreeNoFollowCalls.Should().Be(result.RemovedRoots.Count);
        registry.UninstallRecords.Should().BeEmpty();
        registry.StartupValues.Should().BeEmpty();
    }

    [Fact]
    public void Product_packaging_does_not_publish_or_allow_fixture_payloads()
    {
        var root = FindRepositoryRoot();
        var portable = File.ReadAllText(Path.Combine(root, "scripts", "publish-portable-test-package.ps1"));
        var signed = File.ReadAllText(Path.Combine(root, "scripts", "publish-signed-release-package.ps1"));
        var verifier = File.ReadAllText(Path.Combine(root, "scripts", "verify-signed-release-candidate.ps1"));
        var appProject = File.ReadAllText(Path.Combine(root, "src", "Css.App", "Css.App.csproj"));
        var workerProject = File.ReadAllText(Path.Combine(root, "src", "Css.Elevated", "Css.Elevated.csproj"));

        portable.Should().NotContain("Css.AcceptanceFixtures");
        signed.Should().NotContain("Css.AcceptanceFixtures");
        appProject.Should().NotContain("Css.AcceptanceFixtures");
        workerProject.Should().NotContain("Css.AcceptanceFixtures");
        verifier.Should().Contain("Unlisted package payload file");
    }

    [Theory]
    [InlineData("provision")]
    [InlineData("uninstall")]
    [InlineData("lock")]
    [InlineData("reset")]
    public void Every_mutating_command_requires_the_exact_attestation(string commandName)
    {
        var args = new List<string>
        {
            commandName,
            "--session-id",
            SessionId
        };
        if (commandName == "uninstall")
            args.AddRange(["--role", "uninstall"]);

        var missing = () => AcceptanceFixtureCommand.Parse(args.ToArray());
        missing.Should().Throw<ArgumentException>().WithMessage("*attestation*");

        args.AddRange(["--attestation", "wrong"]);
        var wrong = () => AcceptanceFixtureCommand.Parse(args.ToArray());
        wrong.Should().Throw<ArgumentException>().WithMessage("*attestation*");
    }

    [Fact]
    public void Status_is_read_only_and_lock_duration_is_bounded()
    {
        var status = AcceptanceFixtureCommand.Parse([
            "status",
            "--session-id",
            SessionId
        ]);
        status.Kind.Should().Be(AcceptanceFixtureCommandKind.Status);
        status.IsMutating.Should().BeFalse();
        status.Attestation.Should().BeNull();

        var lockCommand = AcceptanceFixtureCommand.Parse([
            "lock",
            "--session-id",
            SessionId,
            "--duration-seconds",
            "60",
            "--attestation",
            AcceptanceFixtureAuthority.RequiredAttestation
        ]);
        lockCommand.Kind.Should().Be(AcceptanceFixtureCommandKind.Lock);
        lockCommand.IsMutating.Should().BeTrue();
        lockCommand.Duration.Should().Be(TimeSpan.FromSeconds(60));

        var tooLong = () => AcceptanceFixtureCommand.Parse([
            "lock",
            "--session-id",
            SessionId,
            "--duration-seconds",
            "601",
            "--attestation",
            AcceptanceFixtureAuthority.RequiredAttestation
        ]);
        tooLong.Should().Throw<ArgumentException>().WithMessage("*duration*");
    }

    [Fact]
    public void Command_parser_rejects_unknown_duplicate_and_role_drift()
    {
        var unknown = () => AcceptanceFixtureCommand.Parse([
            "status",
            "--session-id",
            SessionId,
            "--unknown",
            "value"
        ]);
        var duplicate = () => AcceptanceFixtureCommand.Parse([
            "status",
            "--session-id",
            SessionId,
            "--session-id",
            SessionId
        ]);
        var invalidRole = () => AcceptanceFixtureCommand.Parse([
            "uninstall",
            "--session-id",
            SessionId,
            "--role",
            "ordinary",
            "--attestation",
            AcceptanceFixtureAuthority.RequiredAttestation
        ]);

        unknown.Should().Throw<ArgumentException>();
        duplicate.Should().Throw<ArgumentException>();
        invalidRole.Should().Throw<ArgumentException>();
    }

    private static readonly DateTimeOffset FixedNow =
        new(2026, 7, 19, 4, 0, 0, TimeSpan.Zero);

    private static AcceptanceFixtureLayout CreateLayout(string sessionId = SessionId) =>
        AcceptanceFixtureLayout.Create(
            sessionId,
            @"C:\",
            @"D:\",
            @"C:\Users\Fixture\AppData\Local",
            @"C:\Temp");

    private static AcceptanceFixturePayload Payload(FakeFixtureFileSystem files)
    {
        var payload = new AcceptanceFixturePayload([
            new AcceptanceFixturePayloadFile(@"Q:\staging\Css.AcceptanceFixtures.exe", "Css.AcceptanceFixtures.exe"),
            new AcceptanceFixturePayloadFile(@"Q:\staging\Css.AcceptanceFixtures.dll", "Css.AcceptanceFixtures.dll"),
            new AcceptanceFixturePayloadFile(@"Q:\staging\Css.AcceptanceFixtures.deps.json", "Css.AcceptanceFixtures.deps.json"),
            new AcceptanceFixturePayloadFile(@"Q:\staging\Css.AcceptanceFixtures.runtimeconfig.json", "Css.AcceptanceFixtures.runtimeconfig.json")
        ]);
        foreach (var file in payload.Files)
            files.ExistingPaths.Add(file.SourcePath);
        return payload;
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
        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private sealed class FakeFixtureFileSystem : IAcceptanceFixtureFileSystem
    {
        public HashSet<string> ExistingPaths { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> TextFiles { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<string> DeletedTrees { get; } = [];
        public int MutationCount { get; private set; }
        public int DeleteTreeNoFollowCalls { get; private set; }
        public string? FailWritePath { get; init; }

        public bool Exists(string path) => ExistingPaths.Contains(path);

        public void CreateDirectory(string path)
        {
            MutationCount++;
            ExistingPaths.Add(path);
        }

        public void WriteAllTextExclusive(string path, string content)
        {
            if (path.Equals(FailWritePath, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Injected owner-marker write failure.");
            if (ExistingPaths.Contains(path))
                throw new IOException("File already exists.");
            MutationCount++;
            ExistingPaths.Add(path);
            TextFiles[path] = content;
        }

        public void CopyFileExclusive(string sourcePath, string destinationPath)
        {
            if (!ExistingPaths.Contains(sourcePath) || ExistingPaths.Contains(destinationPath))
                throw new IOException("Copy precondition failed.");
            MutationCount++;
            ExistingPaths.Add(destinationPath);
            TextFiles[destinationPath] = "fixture-payload";
        }

        public string ReadAllText(string path) => TextFiles[path];

        public void DeleteTreeNoFollow(string path)
        {
            MutationCount++;
            DeleteTreeNoFollowCalls++;
            DeletedTrees.Add(path);
            ExistingPaths.RemoveWhere(candidate => IsUnder(path, candidate));
            foreach (var key in TextFiles.Keys.Where(candidate => IsUnder(path, candidate)).ToArray())
                TextFiles.Remove(key);
        }

        public void CreateOwnedDirectoryForTest(string root, string marker, string sessionId)
        {
            ExistingPaths.Add(root);
            ExistingPaths.Add(marker);
            TextFiles[marker] = $"{{\"SessionId\":\"{sessionId}\",\"Kind\":\"test\"}}";
        }

        private static bool IsUnder(string root, string candidate) =>
            candidate.Equals(root, StringComparison.OrdinalIgnoreCase) ||
            candidate.StartsWith(
                root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeFixtureRegistry : IAcceptanceFixtureRegistry
    {
        public Dictionary<string, AcceptanceFixtureUninstallRecord> UninstallRecords { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> StartupValues { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> StartupApprovalValues { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public int MutationCount { get; private set; }

        public AcceptanceFixtureUninstallRecord? ReadUninstallRecord(string keyName) =>
            UninstallRecords.GetValueOrDefault(keyName);

        public string? ReadStartupValue(string valueName) =>
            StartupValues.GetValueOrDefault(valueName);

        public bool StartupApprovalValueExists(string valueName) =>
            StartupApprovalValues.Contains(valueName);

        public void CreateUninstallRecord(AcceptanceFixtureUninstallRecord record)
        {
            MutationCount++;
            UninstallRecords.Add(record.KeyName, record);
        }

        public void CreateStartupValue(string valueName, string command)
        {
            MutationCount++;
            StartupValues.Add(valueName, command);
        }

        public void DeleteUninstallRecord(string keyName)
        {
            MutationCount++;
            UninstallRecords.Remove(keyName);
        }

        public void DeleteStartupValue(string valueName)
        {
            MutationCount++;
            StartupValues.Remove(valueName);
        }

        public void DeleteStartupApprovalValue(string valueName)
        {
            MutationCount++;
            StartupApprovalValues.Remove(valueName);
        }
    }
}
