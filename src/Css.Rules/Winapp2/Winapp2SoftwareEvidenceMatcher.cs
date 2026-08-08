using Css.Core.Software;

namespace Css.Rules.Winapp2;

public static class Winapp2SoftwareEvidenceMatcher
{
    public static IReadOnlyList<Winapp2SoftwareEvidence> Match(
        Winapp2RuleCatalog catalog,
        SoftwareProfile profile,
        Func<string, string>? expandVariables = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(profile);
        expandVariables ??= Environment.ExpandEnvironmentVariables;

        var profilePaths = ProfilePaths(profile, expandVariables);
        if (profilePaths.Count == 0)
            return [];

        var evidence = new List<Winapp2SoftwareEvidence>();
        foreach (var rule in catalog.Rules)
        {
            var matchedProfilePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var ownedFileTargets = new List<string>();

            foreach (var expression in rule.DetectFilePaths)
            {
                if (TryOwnedPathExpression(expression, expandVariables, profilePaths, out var owners))
                {
                    foreach (var owner in owners)
                        matchedProfilePaths.Add(owner);
                }
            }

            foreach (var expression in rule.FileTargets)
            {
                if (TryOwnedFileExpression(expression, expandVariables, profilePaths, out var owners))
                {
                    foreach (var owner in owners)
                        matchedProfilePaths.Add(owner);
                }
            }

            if (matchedProfilePaths.Count == 0)
                continue;

            foreach (var expression in rule.FileTargets)
            {
                if (Winapp2FileTargetPattern.TryParse(expression, expandVariables, out var target)
                    && matchedProfilePaths.Any(target.IsOwnedBy))
                {
                    ownedFileTargets.Add(expression);
                }
            }

            evidence.Add(new Winapp2SoftwareEvidence
            {
                SoftwareName = profile.Name,
                RuleName = rule.Name,
                RulePackSource = catalog.Descriptor.SourceName,
                RulePackVersion = catalog.Descriptor.Version,
                RulePackSha256 = catalog.ContentSha256,
                Warning = rule.Warning,
                MatchedProfilePaths = matchedProfilePaths.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                CandidateFileTargets = ownedFileTargets.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                ExclusionTargets = rule.ExclusionTargets,
                DefaultSelected = rule.DefaultSelected,
                RegistryTargetCount = rule.RegistryTargets.Count
            });
        }

        return evidence;
    }

    private static IReadOnlyList<string> ProfilePaths(
        SoftwareProfile profile,
        Func<string, string> expandVariables)
    {
        var values = new List<string>();
        if (!string.IsNullOrWhiteSpace(profile.InstallPath))
            values.Add(profile.InstallPath);
        values.AddRange(profile.DataPaths);
        values.AddRange(profile.CachePaths);
        values.AddRange(profile.LogPaths);
        values.AddRange(profile.CDriveWritePaths);

        return values
            .Select(value => TryCanonicalPath(value, expandVariables, out var path) ? path : null)
            .Where(path => path is not null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool TryOwnedFileExpression(
        string expression,
        Func<string, string> expandVariables,
        IReadOnlyList<string> profilePaths,
        out IReadOnlyList<string> owners)
    {
        owners = [];
        if (!Winapp2FileTargetPattern.TryParse(expression, expandVariables, out var fileTarget))
            return false;
        owners = profilePaths.Where(fileTarget.IsOwnedBy).ToArray();
        return owners.Count > 0;
    }

    private static bool TryOwnedPathExpression(
        string expression,
        Func<string, string> expandVariables,
        IReadOnlyList<string> profilePaths,
        out IReadOnlyList<string> owners)
    {
        owners = [];
        if (!Winapp2PathPattern.TryCreate(expression, expandVariables, out var pathTarget))
            return false;
        owners = profilePaths.Where(pathTarget.IsOwnedBy).ToArray();
        return owners.Count > 0;
    }

    private static bool TryCanonicalPath(
        string value,
        Func<string, string> expandVariables,
        out string path)
    {
        path = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            var expanded = expandVariables(value).Trim().Trim('"');
            if (!Path.IsPathFullyQualified(expanded))
                return false;
            path = Path.TrimEndingDirectorySeparator(Path.GetFullPath(expanded));
            return path.Length > 0;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

}
