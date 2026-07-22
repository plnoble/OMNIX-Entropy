using System.Threading;
using System.Threading.Tasks;

namespace Css.Core.Operations;

/// <summary>
/// The single chokepoint through which every operation — manual UI click or agent tool call — must flow.
/// Enforces: authorize → snapshot (if destructive) → confirm (if destructive) → elevate (if required)
/// → execute → quarantine (if destructive) → journal. No code path may bypass this.
/// </summary>
public interface IOperationPipeline
{
    /// <summary>Executes <paramref name="descriptor"/> through the full pipeline.</summary>
    Task<OperationResult> ExecuteAsync(OperationDescriptor descriptor, CancellationToken ct = default);
}
