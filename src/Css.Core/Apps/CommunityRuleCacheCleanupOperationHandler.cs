using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Software;
using Css.Core.Timeline;

namespace Css.Core.Apps;

public sealed class CommunityRuleCacheCleanupOperationHandler
{
    private readonly QuarantineOperationHandler _quarantineHandler;
    private readonly SoftwareProfile _currentProfile;
    private readonly Func<string?> _activeRulePackSha256Resolver;

    public CommunityRuleCacheCleanupOperationHandler(
        FileQuarantineService quarantine,
        ActionTimelineStore timeline,
        SoftwareProfile currentProfile,
        Func<string?> activeRulePackSha256Resolver,
        IQuarantineCandidateIdentityReader identityReader)
    {
        ArgumentNullException.ThrowIfNull(quarantine);
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(currentProfile);
        ArgumentNullException.ThrowIfNull(activeRulePackSha256Resolver);
        ArgumentNullException.ThrowIfNull(identityReader);
        _quarantineHandler = new QuarantineOperationHandler(quarantine, timeline, identityReader);
        _currentProfile = currentProfile;
        _activeRulePackSha256Resolver = activeRulePackSha256Resolver;
    }

    public Task<OperationResult> ExecuteAsync(
        OperationDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        string? activeSha256;
        try
        {
            activeSha256 = _activeRulePackSha256Resolver();
        }
        catch
        {
            return Task.FromResult(OperationResult.Fail("当前扩展规则版本无法读取，操作已停止。"));
        }

        if (string.IsNullOrWhiteSpace(activeSha256))
            return Task.FromResult(OperationResult.Fail("扩展规则已停用或不可用，操作已停止。"));
        var gate = CommunityRuleCacheCleanupPlanBuilder.ValidateForExecution(
            descriptor,
            _currentProfile,
            activeSha256);
        return gate.Success
            ? _quarantineHandler.ExecuteAsync(descriptor, cancellationToken)
            : Task.FromResult(gate);
    }
}
