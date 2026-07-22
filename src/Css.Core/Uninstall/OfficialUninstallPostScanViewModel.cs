namespace Css.Core.Uninstall;

public enum OfficialUninstallPostScanState
{
    ScanFailed,
    SoftwareStillPresent,
    NoVisibleResidue,
    ReviewNeeded
}

public enum OfficialUninstallPostScanAction
{
    Close,
    RetryReadOnlyScan,
    ReviewResidue
}

public sealed class OfficialUninstallPostScanViewModel
{
    public required OfficialUninstallPostScanState State { get; init; }
    public required string Title { get; init; }
    public required string StatusLabel { get; init; }
    public required string Conclusion { get; init; }
    public IReadOnlyList<string> Facts { get; init; } = [];
    public required string AgentAdvice { get; init; }
    public required string PrimaryActionText { get; init; }
    public OfficialUninstallPostScanAction PrimaryAction { get; init; } =
        OfficialUninstallPostScanAction.Close;
    public bool CanReviewResidue { get; init; }
    public bool TechnicalDetailsAvailable { get; init; }
    public bool CanExecuteDirectly => false;
    public bool HasPrimaryAction => PrimaryAction != OfficialUninstallPostScanAction.Close;

    public string VisibleText => string.Join(
        Environment.NewLine,
        new[] { Title, StatusLabel, Conclusion }
            .Concat(Facts)
            .Concat([AgentAdvice, PrimaryActionText]));
}
