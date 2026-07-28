namespace Css.Core.Software;

public enum SoftwareSystemFootprintKind
{
    ContextMenu,
    ExplorerEntry,
    BrowserIntegration,
    FileAssociation
}

/// <summary>
/// Read-only evidence that an installed application adds an entry to a Windows
/// or browser surface. Presence is not a risk verdict and grants no mutation
/// authority.
/// </summary>
public sealed class SoftwareSystemFootprintObservation
{
    public required SoftwareSystemFootprintKind Kind { get; init; }
    public required string DisplayName { get; init; }
    public required string SourceLocator { get; init; }
    public required string Evidence { get; init; }
}
