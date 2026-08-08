using System.Security.Cryptography;
using System.Text;
using Css.Core.Software;
using Css.Rules.Winapp2;
using FluentAssertions;

namespace Css.Tests;

public sealed class Winapp2RuleCatalogTests
{
    [Fact]
    public void Catalog_preserves_pinned_source_and_read_only_rule_evidence()
    {
        const string content = """
            ; fixture only - no third-party rule data
            [Example Browser Cache]
            LangSecRef=3029
            Section=Browsers
            DetectFile=%LOCALAPPDATA%\Example\Browser\browser.exe
            FileKey1=%LOCALAPPDATA%\Example\Browser\User Data\Default\Cache|*
            RegKey1=HKCU\Software\Example\Browser
            ExcludeKey1=FILE|%LOCALAPPDATA%\Example\Browser\User Data\Default\Cache|keep.db
            Warning=Signing in again may be required.
            Default=False
            FutureKey=preserve-as-diagnostic
            """;
        var descriptor = Descriptor(content);

        var catalog = Load(content, descriptor);

        catalog.Descriptor.Should().BeSameAs(descriptor);
        catalog.ContentSha256.Should().Be(descriptor.ExpectedSha256);
        catalog.IsExecutionAuthorized.Should().BeFalse();
        catalog.Rules.Should().ContainSingle();
        var rule = catalog.Rules[0];
        rule.Name.Should().Be("Example Browser Cache");
        rule.LanguageSection.Should().Be("3029");
        rule.Section.Should().Be("Browsers");
        rule.DetectFilePaths.Should().Equal("%LOCALAPPDATA%\\Example\\Browser\\browser.exe");
        rule.FileTargets.Should().Equal("%LOCALAPPDATA%\\Example\\Browser\\User Data\\Default\\Cache|*");
        rule.RegistryTargets.Should().Equal("HKCU\\Software\\Example\\Browser");
        rule.ExclusionTargets.Should().ContainSingle();
        rule.Warning.Should().Be("Signing in again may be required.");
        rule.DefaultSelected.Should().BeFalse();
        rule.RawSource.Should().Contain("FileKey1=").And.Contain("RegKey1=");
        rule.IsExecutionAuthorized.Should().BeFalse();
        catalog.Diagnostics.Should().ContainSingle(diagnostic =>
            diagnostic.RuleName == rule.Name
            && diagnostic.Key == "FutureKey");
    }

    [Fact]
    public void Catalog_fails_closed_for_unpinned_malformed_or_oversized_input()
    {
        const string valid = "[Example]\nFileKey1=C:\\Users\\Example\\Cache|*\n";
        var wrongHash = Descriptor(valid) with { ExpectedSha256 = new string('0', 64) };
        var malformed = "[Example]\nthis line has no assignment\n";
        var excessiveTargets = new StringBuilder("[Example]\n");
        for (var index = 0; index <= Winapp2RuleCatalogLoader.MaxTargetsPerRule; index++)
            excessiveTargets.Append("FileKey").Append(index + 1).Append("=C:\\Fixture\\Cache|").Append(index).AppendLine();

        var hashAction = () => Load(valid, wrongHash);
        var malformedAction = () => Load(malformed, Descriptor(malformed));
        var targetAction = () => Load(excessiveTargets.ToString(), Descriptor(excessiveTargets.ToString()));
        var oversized = new byte[Winapp2RuleCatalogLoader.MaxPackBytes + 1];
        var oversizedAction = () => new Winapp2RuleCatalogLoader().Load(
            new MemoryStream(oversized),
            Descriptor(oversized));

        hashAction.Should().Throw<InvalidDataException>().WithMessage("*SHA-256*");
        malformedAction.Should().Throw<InvalidDataException>().WithMessage("*line 2*");
        targetAction.Should().Throw<InvalidDataException>().WithMessage("*target limit*");
        oversizedAction.Should().Throw<InvalidDataException>().WithMessage("*size limit*");
    }

    [Fact]
    public void Descriptor_requires_source_version_license_and_valid_sha256()
    {
        const string content = "[Example]\nFileKey1=C:\\Fixture\\Cache|*\n";
        var descriptor = Descriptor(content) with { LicenseName = "" };
        var invalidHash = Descriptor(content) with { ExpectedSha256 = "ABC" };

        var licenseAction = () => Load(content, descriptor);
        var hashAction = () => Load(content, invalidHash);

        licenseAction.Should().Throw<ArgumentException>().WithMessage("*license*");
        hashAction.Should().Throw<ArgumentException>().WithMessage("*SHA-256*");
    }

    [Fact]
    public void Catalog_rejects_empty_duplicate_and_invalid_utf8_content()
    {
        const string duplicate = "[Example]\nFileKey1=C:\\Fixture\\Cache|*\n[example]\nFileKey1=C:\\Fixture\\Other|*\n";
        var invalidUtf8 = new byte[] { 0xFF, 0xFE, 0xFA };

        var emptyAction = () => Load(string.Empty, Descriptor(string.Empty));
        var duplicateAction = () => Load(duplicate, Descriptor(duplicate));
        var encodingAction = () => new Winapp2RuleCatalogLoader().Load(
            new MemoryStream(invalidUtf8),
            Descriptor(invalidUtf8));

        emptyAction.Should().Throw<InvalidDataException>().WithMessage("*at least one rule*");
        duplicateAction.Should().Throw<InvalidDataException>().WithMessage("*duplicate section*");
        encodingAction.Should().Throw<InvalidDataException>().WithMessage("*valid UTF-8*");
    }

    [Fact]
    public void Software_attribution_requires_owned_path_evidence_and_never_authorizes_execution()
    {
        const string content = """
            [Example Browser Cache]
            DetectFile=%LOCALAPPDATA%\Example\Browser*
            FileKey1=%LOCALAPPDATA%\Example\Browser*\User Data\Default\Cache|*
            RegKey1=HKCU\Software\Example\Browser
            ExcludeKey1=FILE|%LOCALAPPDATA%\Example\Browser*\User Data\Default\Cache|keep.db

            [Broad Vendor Cache]
            FileKey1=%LOCALAPPDATA%\Example|*
            """;
        var catalog = Load(content, Descriptor(content));
        var browserRoot = Path.GetFullPath(@"C:\Users\Fixture\AppData\Local\Example\Browser");
        var browser = new SoftwareProfile
        {
            Name = "Example Browser",
            DataPaths = [browserRoot]
        };
        var sameNameWithoutPaths = new SoftwareProfile { Name = "Example Browser" };
        var otherProduct = new SoftwareProfile
        {
            Name = "Example Drive",
            DataPaths = [Path.GetFullPath(@"C:\Users\Fixture\AppData\Local\Example\Drive")]
        };
        string Expand(string value) => value.Replace(
            "%LOCALAPPDATA%",
            Path.GetFullPath(@"C:\Users\Fixture\AppData\Local"),
            StringComparison.OrdinalIgnoreCase);

        var browserMatches = Winapp2SoftwareEvidenceMatcher.Match(catalog, browser, Expand);
        var nameOnlyMatches = Winapp2SoftwareEvidenceMatcher.Match(catalog, sameNameWithoutPaths, Expand);
        var otherMatches = Winapp2SoftwareEvidenceMatcher.Match(catalog, otherProduct, Expand);

        browserMatches.Should().ContainSingle();
        browserMatches[0].RuleName.Should().Be("Example Browser Cache");
        browserMatches[0].MatchedProfilePaths.Should().Equal(browserRoot);
        browserMatches[0].CandidateFileTargets.Should().ContainSingle();
        browserMatches[0].ExclusionTargets.Should().ContainSingle();
        browserMatches[0].RegistryTargetCount.Should().Be(1);
        browserMatches[0].IsExecutionAuthorized.Should().BeFalse();
        nameOnlyMatches.Should().BeEmpty("a matching name is not path ownership evidence");
        otherMatches.Should().BeEmpty("a broad vendor path must not claim a narrower sibling product");
    }

    [Fact]
    public void Winapp2_source_surface_has_no_mutation_process_or_operation_authority()
    {
        var sourceRoot = FindRepositoryDirectory("src", "Css.Rules", "Winapp2");
        var source = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => !Path.GetFileName(path).Equals(
                    "Winapp2RulePackStore.cs",
                    StringComparison.OrdinalIgnoreCase))
                .Where(path => !Path.GetFileName(path).Equals(
                    "Winapp2RulePackDownloadClient.cs",
                    StringComparison.OrdinalIgnoreCase))
                .Where(path => !Path.GetFileName(path).Equals(
                    "Winapp2RulePreferenceStore.cs",
                    StringComparison.OrdinalIgnoreCase))
                .Select(File.ReadAllText));

        source.Should().NotContain("Css.Core.Operations");
        source.Should().NotContain("OperationDescriptor");
        source.Should().NotContain("File.Delete");
        source.Should().NotContain("Directory.Delete");
        source.Should().NotContain("RegistryKey");
        source.Should().NotContain("Process.Start");
        source.Should().NotContain("HttpClient");
    }

    private static Winapp2RuleCatalog Load(string content, Winapp2RulePackDescriptor descriptor) =>
        new Winapp2RuleCatalogLoader().Load(
            new MemoryStream(Encoding.UTF8.GetBytes(content)),
            descriptor);

    private static Winapp2RulePackDescriptor Descriptor(string content) => Descriptor(Encoding.UTF8.GetBytes(content));

    private static Winapp2RulePackDescriptor Descriptor(byte[] content) =>
        new()
        {
            SourceName = "OMNIX test fixture",
            SourceUri = new Uri("https://example.invalid/winapp2.ini"),
            Version = "fixture-1",
            LicenseName = "Fixture-only",
            LicenseUri = new Uri("https://example.invalid/license"),
            ExpectedSha256 = Convert.ToHexString(SHA256.HashData(content))
        };

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
}
