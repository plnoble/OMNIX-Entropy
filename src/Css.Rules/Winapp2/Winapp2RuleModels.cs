namespace Css.Rules.Winapp2;

public sealed record Winapp2RulePackDescriptor
{
    public required string SourceName { get; init; }
    public required Uri SourceUri { get; init; }
    public required string Version { get; init; }
    public required string LicenseName { get; init; }
    public required Uri LicenseUri { get; init; }
    public required string ExpectedSha256 { get; init; }
}

public static class Winapp2RulePackDescriptorPolicy
{
    public static void Validate(Winapp2RulePackDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        RequireText(descriptor.SourceName, "source name");
        RequireAbsoluteUri(descriptor.SourceUri, "source URI");
        RequireText(descriptor.Version, "version");
        RequireText(descriptor.LicenseName, "license name");
        RequireAbsoluteUri(descriptor.LicenseUri, "license URI");

        if (descriptor.ExpectedSha256.Length != 64
            || descriptor.ExpectedSha256.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException(
                "Expected SHA-256 must contain exactly 64 hexadecimal characters.",
                nameof(descriptor));
        }
    }

    private static void RequireText(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Rule-pack {field} is required.", "descriptor");
    }

    private static void RequireAbsoluteUri(Uri? value, string field)
    {
        if (value is null || !value.IsAbsoluteUri)
            throw new ArgumentException($"Rule-pack {field} must be an absolute URI.", "descriptor");
    }
}

public sealed class Winapp2RuleCatalog
{
    public required Winapp2RulePackDescriptor Descriptor { get; init; }
    public required string ContentSha256 { get; init; }
    public required IReadOnlyList<Winapp2RuleDefinition> Rules { get; init; }
    public required IReadOnlyList<Winapp2RuleDiagnostic> Diagnostics { get; init; }
    public bool IsExecutionAuthorized => false;
}

public sealed class Winapp2RuleDefinition
{
    public required string Name { get; init; }
    public string? LanguageSection { get; init; }
    public string? Section { get; init; }
    public IReadOnlyList<string> DetectPaths { get; init; } = [];
    public IReadOnlyList<string> DetectFilePaths { get; init; } = [];
    public IReadOnlyList<string> SpecialDetections { get; init; } = [];
    public IReadOnlyList<string> OperatingSystemConstraints { get; init; } = [];
    public IReadOnlyList<string> FileTargets { get; init; } = [];
    public IReadOnlyList<string> RegistryTargets { get; init; } = [];
    public IReadOnlyList<string> ExclusionTargets { get; init; } = [];
    public string? Warning { get; init; }
    public bool? DefaultSelected { get; init; }
    public required string RawSource { get; init; }
    public bool IsExecutionAuthorized => false;
}

public sealed class Winapp2RuleDiagnostic
{
    public required int LineNumber { get; init; }
    public required string RuleName { get; init; }
    public required string Key { get; init; }
    public required string Message { get; init; }
}

public sealed class Winapp2SoftwareEvidence
{
    public required string SoftwareName { get; init; }
    public required string RuleName { get; init; }
    public required string RulePackSource { get; init; }
    public required string RulePackVersion { get; init; }
    public required string RulePackSha256 { get; init; }
    public string? Warning { get; init; }
    public required IReadOnlyList<string> MatchedProfilePaths { get; init; }
    public required IReadOnlyList<string> CandidateFileTargets { get; init; }
    public IReadOnlyList<string> ExclusionTargets { get; init; } = [];
    public bool? DefaultSelected { get; init; }
    public int RegistryTargetCount { get; init; }
    public bool IsExecutionAuthorized => false;
}
