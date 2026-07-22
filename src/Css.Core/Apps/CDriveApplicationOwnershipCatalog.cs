using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class CDriveApplicationOwnershipCatalog
{
    public required IReadOnlyList<SoftwareProfile> AllProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> OrdinaryProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> SystemProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> OwnershipPendingProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> ReadOnlyProfiles { get; init; }

    public string BeginnerSummary => ApplicationOwnershipSummaryFormatter.Create(
        OrdinaryProfiles.Count,
        SystemProfiles.Count,
        OwnershipPendingProfiles.Count);

    public static CDriveApplicationOwnershipCatalog Create(
        IEnumerable<SoftwareProfile>? profiles)
    {
        var all = profiles?
            .Where(AppPresentationBuilder.HasCDriveFootprint)
            .ToArray() ?? [];
        var ordinary = all
            .Where(AppPresentationBuilder.CanUseOrdinaryApplicationActions)
            .ToArray();
        var system = all
            .Where(profile => profile.Category == SoftwareCategory.SystemTool)
            .ToArray();
        var ownershipPending = all
            .Where(profile => profile.Category != SoftwareCategory.SystemTool)
            .Where(profile => !AppPresentationBuilder.CanUseOrdinaryApplicationActions(profile))
            .ToArray();
        var readOnly = all
            .Where(profile => !AppPresentationBuilder.CanUseOrdinaryApplicationActions(profile))
            .ToArray();

        return new CDriveApplicationOwnershipCatalog
        {
            AllProfiles = all,
            OrdinaryProfiles = ordinary,
            SystemProfiles = system,
            OwnershipPendingProfiles = ownershipPending,
            ReadOnlyProfiles = readOnly
        };
    }
}
