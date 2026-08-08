using System.Security.Cryptography;
using System.Text;
using Css.Rules.Winapp2;
using FluentAssertions;

namespace Css.Tests;

public sealed class Winapp2RulePackStoreTests
{
    [Fact]
    public async Task Store_requires_descriptor_bound_source_license_version_and_hash_consent()
    {
        var root = TemporaryRoot();
        const string content = "[Example]\nFileKey1=C:\\Fixture\\Cache|*\n";
        var descriptor = Descriptor(content, "v1");
        var store = new Winapp2RulePackStore(root);

        try
        {
            var missingConsent = () => store.ActivateAsync(
                Stream(content),
                descriptor,
                Consent(descriptor) with { UserConfirmedActivation = false });
            var staleHash = () => store.ActivateAsync(
                Stream(content),
                descriptor,
                Consent(descriptor) with { ReviewedSha256 = new string('0', 64) });
            var changedLicense = () => store.ActivateAsync(
                Stream(content),
                descriptor,
                Consent(descriptor) with { ReviewedLicenseUri = new Uri("https://example.invalid/other-license") });

            await missingConsent.Should().ThrowAsync<InvalidOperationException>().WithMessage("*confirmation*");
            await staleHash.Should().ThrowAsync<InvalidOperationException>().WithMessage("*reviewed metadata*");
            await changedLicense.Should().ThrowAsync<InvalidOperationException>().WithMessage("*reviewed metadata*");
            store.GetStatus().Should().BeNull();
            Directory.Exists(root).Should().BeFalse("refused consent must not create managed state");
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task Store_activates_validated_immutable_packs_and_rolls_back_with_stale_safe_confirmation()
    {
        var root = TemporaryRoot();
        const string firstContent = "[First]\nFileKey1=C:\\Fixture\\First|*\n";
        const string secondContent = "[Second]\nFileKey1=C:\\Fixture\\Second|*\n";
        var first = Descriptor(firstContent, "v1");
        var second = Descriptor(secondContent, "v2");
        var store = new Winapp2RulePackStore(root, new FixedTimeProvider(DateTimeOffset.Parse("2026-08-01T01:02:03Z")));

        try
        {
            var initial = await store.ActivateAsync(Stream(firstContent), first, Consent(first));
            var updated = await store.ActivateAsync(Stream(secondContent), second, Consent(second));

            initial.ActiveVersion.Should().Be("v1");
            initial.PreviousVersion.Should().BeNull();
            updated.ActiveVersion.Should().Be("v2");
            updated.PreviousVersion.Should().Be("v1");
            updated.IsExecutionAuthorized.Should().BeFalse();
            File.Exists(initial.ActivePackPath).Should().BeTrue();
            File.Exists(updated.ActivePackPath).Should().BeTrue();
            initial.ActivePackPath.Should().NotBe(updated.ActivePackPath);
            Path.GetFileNameWithoutExtension(initial.ActivePackPath).Should().Be(first.ExpectedSha256);
            Path.GetFileNameWithoutExtension(updated.ActivePackPath).Should().Be(second.ExpectedSha256);
            store.LoadActiveCatalog().Rules.Should().ContainSingle(rule => rule.Name == "Second");

            var staleRollback = () => store.RollbackAsync(new Winapp2RulePackRollbackConsent
            {
                UserConfirmedRollback = true,
                ExpectedActiveSha256 = first.ExpectedSha256,
                ExpectedPreviousSha256 = second.ExpectedSha256
            });
            await staleRollback.Should().ThrowAsync<InvalidOperationException>().WithMessage("*changed since review*");

            var rolledBack = await store.RollbackAsync(new Winapp2RulePackRollbackConsent
            {
                UserConfirmedRollback = true,
                ExpectedActiveSha256 = second.ExpectedSha256,
                ExpectedPreviousSha256 = first.ExpectedSha256
            });
            rolledBack.ActiveVersion.Should().Be("v1");
            rolledBack.PreviousVersion.Should().Be("v2");
            store.LoadActiveCatalog().Rules.Should().ContainSingle(rule => rule.Name == "First");
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task Invalid_or_cancelled_activation_leaves_the_known_good_pointer_unchanged()
    {
        var root = TemporaryRoot();
        const string firstContent = "[First]\nFileKey1=C:\\Fixture\\First|*\n";
        const string malformed = "[Broken]\nnot-an-assignment\n";
        var first = Descriptor(firstContent, "v1");
        var broken = Descriptor(malformed, "broken");
        var store = new Winapp2RulePackStore(root);

        try
        {
            await store.ActivateAsync(Stream(firstContent), first, Consent(first));
            var statePath = store.GetStatus()!.StatePath;
            var stateBefore = await File.ReadAllBytesAsync(statePath);
            var invalid = () => store.ActivateAsync(Stream(malformed), broken, Consent(broken));
            using var cancellation = new CancellationTokenSource();
            using var interrupted = new CancelAfterReadStream(
                Encoding.UTF8.GetBytes(firstContent),
                cancellation);
            var cancelledDescriptor = first with { Version = "cancelled" };
            var cancelled = () => store.ActivateAsync(
                interrupted,
                cancelledDescriptor,
                Consent(cancelledDescriptor),
                cancellation.Token);

            await invalid.Should().ThrowAsync<InvalidDataException>();
            await cancelled.Should().ThrowAsync<OperationCanceledException>();
            (await File.ReadAllBytesAsync(statePath)).Should().Equal(stateBefore);
            store.GetStatus()!.ActiveDescriptor.Version.Should().Be("v1");
            Directory.EnumerateFiles(root, "*.tmp-*", SearchOption.AllDirectories).Should().BeEmpty();
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task Active_pack_corruption_fails_closed_and_store_has_no_maintenance_authority()
    {
        var root = TemporaryRoot();
        const string content = "[Example]\nFileKey1=C:\\Fixture\\Cache|*\n";
        var descriptor = Descriptor(content, "v1");
        var store = new Winapp2RulePackStore(root);

        try
        {
            var receipt = await store.ActivateAsync(Stream(content), descriptor, Consent(descriptor));
            await File.AppendAllTextAsync(receipt.ActivePackPath, "changed");

            var load = store.LoadActiveCatalog;
            load.Should().Throw<InvalidDataException>().WithMessage("*SHA-256*");
            receipt.IsExecutionAuthorized.Should().BeFalse();
            store.GetStatus()!.IsExecutionAuthorized.Should().BeFalse();
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void Store_source_is_limited_to_managed_data_and_has_no_maintenance_or_background_authority()
    {
        var sourceRoot = FindRepositoryDirectory("src", "Css.Rules", "Winapp2");
        var store = File.ReadAllText(Path.Combine(sourceRoot, "Winapp2RulePackStore.cs"));
        var download = File.ReadAllText(Path.Combine(sourceRoot, "Winapp2RulePackDownloadClient.cs"));
        var source = store + Environment.NewLine + download;

        source.Should().NotContain("OperationDescriptor");
        source.Should().NotContain("RegistryKey");
        source.Should().NotContain("Process.Start");
        source.Should().NotContain("Directory.Delete");
        source.Should().NotContain("PeriodicTimer");
        source.Should().NotContain("Task.Delay");
    }

    private static Winapp2RulePackActivationConsent Consent(Winapp2RulePackDescriptor descriptor) =>
        new()
        {
            UserConfirmedActivation = true,
            UserAcceptedLicense = true,
            ReviewedSourceUri = descriptor.SourceUri,
            ReviewedLicenseUri = descriptor.LicenseUri,
            ReviewedVersion = descriptor.Version,
            ReviewedSha256 = descriptor.ExpectedSha256
        };

    private static Winapp2RulePackDescriptor Descriptor(string content, string version) =>
        new()
        {
            SourceName = "Fixture pack",
            SourceUri = new Uri("https://example.invalid/winapp2.ini"),
            Version = version,
            LicenseName = "Fixture-only",
            LicenseUri = new Uri("https://example.invalid/license"),
            ExpectedSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)))
        };

    private static MemoryStream Stream(string content) => new(Encoding.UTF8.GetBytes(content));

    private static string TemporaryRoot() =>
        Path.Combine(Path.GetTempPath(), "omnix-winapp2-store-" + Guid.NewGuid().ToString("N"));

    private static void DeleteRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private static string FindRepositoryDirectory(params string[] segments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine([current.FullName, .. segments]);
            if (Directory.Exists(candidate))
                return candidate;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException(Path.Combine(segments));
    }

    private sealed class CancelAfterReadStream(byte[] content, CancellationTokenSource cancellation)
        : MemoryStream(content)
    {
        private bool _cancelled;

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            var read = await base.ReadAsync(buffer, cancellationToken);
            if (!_cancelled && read > 0)
            {
                _cancelled = true;
                cancellation.Cancel();
            }

            return read;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
