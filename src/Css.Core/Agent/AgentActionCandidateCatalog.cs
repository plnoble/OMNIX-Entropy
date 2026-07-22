using Css.Core.Apps;
using Css.Core.Software;
using Css.Core.Startup;

namespace Css.Core.Agent;

public sealed class AgentActionCandidateCatalog
{
    public required IReadOnlyList<SoftwareProfile> CDriveProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> OrdinaryCDriveProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> MigrationReviewProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> DataLocationReviewProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> ReadOnlyCDriveProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> ResidentProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> OrdinaryResidentProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> ReadOnlyResidentProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> UninstallReviewProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> ReadOnlyUninstallProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> StartupProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> OrdinaryStartupProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> StartupReviewProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> UnsupportedOrdinaryStartupProfiles { get; init; }
    public required IReadOnlyList<SoftwareProfile> ReadOnlyStartupProfiles { get; init; }

    public static AgentActionCandidateCatalog Create(IEnumerable<SoftwareProfile>? profiles)
    {
        var all = profiles?.ToArray() ?? [];
        var cDriveOwnership = CDriveApplicationOwnershipCatalog.Create(all);
        var cDrive = cDriveOwnership.AllProfiles;
        var ordinaryCDrive = cDriveOwnership.OrdinaryProfiles;
        var migration = ordinaryCDrive
            .Where(AppPresentationBuilder.CanReviewMigration)
            .ToArray();
        var migrationSet = migration.ToHashSet();
        var startup = all.Where(profile => profile.StartupEntries.Count > 0).ToArray();
        var startupReview = startup
            .Where(AppPresentationBuilder.CanUseOrdinaryApplicationActions)
            .Where(StartupEntryControlPolicy.HasSingleSupportedObservation)
            .ToArray();
        var startupReviewSet = startupReview.ToHashSet();
        var backgroundOwnership = BackgroundApplicationOwnershipCatalog.Create(all);

        return new AgentActionCandidateCatalog
        {
            CDriveProfiles = cDrive,
            OrdinaryCDriveProfiles = ordinaryCDrive,
            MigrationReviewProfiles = migration,
            DataLocationReviewProfiles = ordinaryCDrive
                .Where(profile => !migrationSet.Contains(profile))
                .ToArray(),
            ReadOnlyCDriveProfiles = cDriveOwnership.ReadOnlyProfiles,
            ResidentProfiles = backgroundOwnership.AllProfiles,
            OrdinaryResidentProfiles = backgroundOwnership.OrdinaryProfiles,
            ReadOnlyResidentProfiles = backgroundOwnership.ReadOnlyProfiles,
            UninstallReviewProfiles = all
                .Where(AppPresentationBuilder.CanReviewUninstall)
                .ToArray(),
            ReadOnlyUninstallProfiles = all
                .Where(profile => !string.IsNullOrWhiteSpace(profile.UninstallCommand))
                .Where(profile => !AppPresentationBuilder.CanUseOrdinaryApplicationActions(profile))
                .ToArray(),
            StartupProfiles = startup,
            OrdinaryStartupProfiles = startup
                .Where(AppPresentationBuilder.CanUseOrdinaryApplicationActions)
                .ToArray(),
            StartupReviewProfiles = startupReview,
            UnsupportedOrdinaryStartupProfiles = startup
                .Where(AppPresentationBuilder.CanUseOrdinaryApplicationActions)
                .Where(profile => !startupReviewSet.Contains(profile))
                .ToArray(),
            ReadOnlyStartupProfiles = startup
                .Where(profile => !AppPresentationBuilder.CanUseOrdinaryApplicationActions(profile))
                .ToArray()
        };
    }
}
