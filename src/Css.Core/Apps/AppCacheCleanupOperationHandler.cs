using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Timeline;

namespace Css.Core.Apps;

public sealed class AppCacheCleanupOperationHandler
{
    private readonly QuarantineOperationHandler _quarantineHandler;
    private readonly IReadOnlyList<string> _approvedUserDataRoots;
    private readonly Func<string, bool> _directoryExists;
    private readonly Func<string, bool> _isReparsePoint;

    public AppCacheCleanupOperationHandler(
        FileQuarantineService quarantine,
        ActionTimelineStore timeline,
        IReadOnlyList<string> approvedUserDataRoots,
        Func<string, bool> directoryExists,
        Func<string, bool> isReparsePoint,
        IQuarantineCandidateIdentityReader identityReader)
    {
        ArgumentNullException.ThrowIfNull(quarantine);
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(approvedUserDataRoots);
        ArgumentNullException.ThrowIfNull(directoryExists);
        ArgumentNullException.ThrowIfNull(isReparsePoint);
        ArgumentNullException.ThrowIfNull(identityReader);
        _quarantineHandler = new QuarantineOperationHandler(quarantine, timeline, identityReader);
        _approvedUserDataRoots = approvedUserDataRoots.ToArray();
        _directoryExists = directoryExists;
        _isReparsePoint = isReparsePoint;
    }

    public Task<OperationResult> ExecuteAsync(
        OperationDescriptor descriptor,
        CancellationToken ct = default)
    {
        var gate = AppCacheCleanupPlanBuilder.ValidateForExecution(
            descriptor,
            _approvedUserDataRoots,
            _directoryExists,
            _isReparsePoint);
        return gate.Success
            ? _quarantineHandler.ExecuteAsync(descriptor, ct)
            : Task.FromResult(gate);
    }
}
