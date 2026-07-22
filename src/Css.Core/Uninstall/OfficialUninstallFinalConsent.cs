using Css.Core.Operations;

namespace Css.Core.Uninstall;

public sealed record OfficialUninstallFinalUserConsent
{
    public required string ConfirmationText { get; init; }
    public required DateTimeOffset ConfirmedAtUtc { get; init; }
    public bool OfficialCommandConfirmed { get; init; }
    public bool AppsClosedConfirmed { get; init; }
    public bool NoAutomaticUndoAcknowledged { get; init; }
    public bool PostUninstallRescanConfirmed { get; init; }
    public bool ExecutionRequested { get; init; }
}

public sealed class OfficialUninstallFinalConsentViewModel
{
    public required string Title { get; init; }
    public required string SoftwareName { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<string> ImpactLines { get; init; }
    public required string ConfirmationText { get; init; }
    public required string SafetyText { get; init; }
    public bool CanExecuteDirectly => false;

    public string VisibleText => string.Join(
        Environment.NewLine,
        new[] { Title, SoftwareName, Summary }
            .Concat(ImpactLines)
            .Concat([SafetyText]));
}

public static class OfficialUninstallFinalConsentPresenter
{
    private const string ChineseTitleSuffix = "\u5b98\u65b9\u5378\u8f7d\u5668";
    private const string EnglishTitleSuffix = "official uninstaller";

    public static OfficialUninstallFinalConsentViewModel Create(OperationDescriptor operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (!string.Equals(operation.Kind, "uninstall.official.run", StringComparison.Ordinal)
            || operation.Source != OperationSource.Manual
            || operation.Risk != RiskLevel.High
            || !operation.IsDestructive
            || operation.ConfirmationAccepted
            || string.IsNullOrWhiteSpace(operation.ConfirmationText))
        {
            throw new ArgumentException(
                "The operation is not an unconfirmed official-uninstall request.",
                nameof(operation));
        }

        return CreatePending(ResolveSoftwareName(operation), operation.ConfirmationText);
    }

    public static OfficialUninstallFinalConsentViewModel CreatePending(
        string softwareName,
        string confirmationText)
    {
        if (string.IsNullOrWhiteSpace(softwareName))
            throw new ArgumentException("The software name is required.", nameof(softwareName));
        if (string.IsNullOrWhiteSpace(confirmationText))
            throw new ArgumentException("The confirmation text is required.", nameof(confirmationText));

        softwareName = softwareName.Trim();
        return new OfficialUninstallFinalConsentViewModel
        {
            Title = "\u6700\u540e\u4e00\u6b65\uff1a\u786e\u8ba4\u662f\u5426\u8fd0\u884c\u5b98\u65b9\u5378\u8f7d\u5668",
            SoftwareName = softwareName,
            Summary = "Computer Agent \u5df2\u5b8c\u6210\u5b89\u5168\u68c0\u67e5\u3002\u8bf7\u53ea\u786e\u8ba4\u4e0b\u9762 3 \u4ef6\u4e8b\u3002",
            ImpactLines =
            [
                "\u5148\u5173\u95ed " + softwareName + " \u548c\u6258\u76d8\u7a97\u53e3\uff0c\u518d\u6253\u5f00\u5b83\u7684\u5b98\u65b9\u5378\u8f7d\u5668\u3002",
                "\u5378\u8f7d\u8f6f\u4ef6\u672c\u8eab\u4e0d\u80fd\u9760\u9694\u79bb\u533a\u6062\u590d\uff1b\u540e\u6094\u65f6\u901a\u5e38\u9700\u8981\u91cd\u65b0\u5b89\u88c5\u3002",
                "\u5378\u8f7d\u540e\u590d\u67e5\u53ea\u4f1a\u5148\u7ed9\u51fa\u7ed3\u8bba\uff1b\u4efb\u4f55\u6b8b\u7559\u5904\u7406\u90fd\u9700\u8981\u518d\u6b21\u786e\u8ba4\u3002"
            ],
            ConfirmationText = confirmationText,
            SafetyText = "\u4efb\u4f55\u4e00\u9879\u6ca1\u6709\u786e\u8ba4\uff0c\u90fd\u4e0d\u4f1a\u751f\u6210\u53ef\u63d0\u4ea4\u7684\u5378\u8f7d\u8bf7\u6c42\u3002"
        };
    }

    private static string ResolveSoftwareName(OperationDescriptor operation)
    {
        if (operation.Arguments.TryGetValue("softwareName", out var value)
            && value is string explicitName
            && !string.IsNullOrWhiteSpace(explicitName))
        {
            return explicitName.Trim();
        }

        var title = operation.Title.Trim();
        if (title.EndsWith(ChineseTitleSuffix, StringComparison.Ordinal))
            return title[..^ChineseTitleSuffix.Length].Trim();
        if (title.EndsWith(EnglishTitleSuffix, StringComparison.OrdinalIgnoreCase))
            return title[..^EnglishTitleSuffix.Length].Trim();

        return title;
    }
}

public sealed record OfficialUninstallFinalConsentSelection
{
    public bool OfficialCommandConfirmed { get; init; }
    public bool AppsClosedConfirmed { get; init; }
    public bool NoAutomaticUndoAcknowledged { get; init; }
    public bool PostUninstallRescanConfirmed { get; init; }
}

public sealed class OfficialUninstallFinalConsentBuildResult
{
    public required IReadOnlyList<string> MissingRequirements { get; init; }
    public OfficialUninstallFinalUserConsent? Consent { get; init; }
    public bool CanSubmit => Consent is not null && MissingRequirements.Count == 0;
}

public static class OfficialUninstallFinalConsentBuilder
{
    public static OfficialUninstallFinalConsentBuildResult Create(
        OfficialUninstallFinalConsentViewModel viewModel,
        OfficialUninstallFinalConsentSelection selection,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(selection);

        var missing = new List<string>();
        if (!selection.OfficialCommandConfirmed)
            missing.Add("\u8fd8\u6ca1\u6709\u786e\u8ba4\u5c06\u8fd0\u884c\u8be5\u8f6f\u4ef6\u7684\u5b98\u65b9\u5378\u8f7d\u5668\u3002");
        if (!selection.AppsClosedConfirmed)
            missing.Add("\u8fd8\u6ca1\u6709\u786e\u8ba4\u8f6f\u4ef6\u548c\u76f8\u5173\u6258\u76d8\u7a97\u53e3\u5df2\u5173\u95ed\u3002");
        if (!selection.NoAutomaticUndoAcknowledged)
            missing.Add("\u8fd8\u6ca1\u6709\u786e\u8ba4\u5378\u8f7d\u8f6f\u4ef6\u672c\u8eab\u4e0d\u80fd\u9760\u9694\u79bb\u533a\u81ea\u52a8\u6062\u590d\u3002");
        if (!selection.PostUninstallRescanConfirmed)
            missing.Add("\u8fd8\u6ca1\u6709\u540c\u610f\u5378\u8f7d\u540e\u8fdb\u884c\u53ea\u8bfb\u590d\u67e5\u3002");

        if (missing.Count > 0)
        {
            return new OfficialUninstallFinalConsentBuildResult
            {
                MissingRequirements = missing
            };
        }

        return new OfficialUninstallFinalConsentBuildResult
        {
            MissingRequirements = [],
            Consent = new OfficialUninstallFinalUserConsent
            {
                ConfirmationText = viewModel.ConfirmationText,
                ConfirmedAtUtc = now,
                OfficialCommandConfirmed = true,
                AppsClosedConfirmed = true,
                NoAutomaticUndoAcknowledged = true,
                PostUninstallRescanConfirmed = true,
                ExecutionRequested = true
            }
        };
    }
}

public enum OfficialUninstallElevatedResponseState
{
    InvalidResponse,
    NotStarted,
    UninstallFailed,
    PostScanReady
}

public sealed class OfficialUninstallElevatedResponseViewModel
{
    public required OfficialUninstallElevatedResponseState State { get; init; }
    public required string Title { get; init; }
    public required string Conclusion { get; init; }
    public required string AgentAdvice { get; init; }
    public OfficialUninstallPostScanViewModel? PostScan { get; init; }
    public bool CanExecuteDirectly => false;

    public string VisibleText => string.Join(
        Environment.NewLine,
        new[] { Title, Conclusion, AgentAdvice }
            .Concat(PostScan is null ? [] : [PostScan.VisibleText]));
}
