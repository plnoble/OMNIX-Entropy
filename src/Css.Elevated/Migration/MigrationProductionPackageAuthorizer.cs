using Css.Elevated.Uninstall;
using Css.Ipc.Uninstall;

namespace Css.Elevated.Migration;

public enum MigrationProductionPackageTrustStatus
{
    Trusted,
    WorkerIdentityMismatch,
    ClientImageUnavailable,
    WorkerImageUnavailable,
    ClientNotTrusted,
    WorkerNotTrusted,
    SignerMismatch
}

public sealed record MigrationProductionPackageTrustResult
{
    public required MigrationProductionPackageTrustStatus Status { get; init; }
    public bool CanAuthorize => Status == MigrationProductionPackageTrustStatus.Trusted;
}

public interface IMigrationProductionPackageAuthorizer
{
    MigrationProductionPackageTrustResult Authorize(
        OfficialUninstallPipePeerIdentity actualClient,
        OfficialUninstallPipePeerIdentity worker);
}

public sealed class WindowsMigrationProductionPackageAuthorizer
    : IMigrationProductionPackageAuthorizer
{
    private readonly IOfficialUninstallProductionPackageAuthorizer _inner;

    public WindowsMigrationProductionPackageAuthorizer(
        IOfficialUninstallProductionPackageAuthorizer inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public MigrationProductionPackageTrustResult Authorize(
        OfficialUninstallPipePeerIdentity actualClient,
        OfficialUninstallPipePeerIdentity worker)
    {
        var result = _inner.Authorize(actualClient, worker);
        return new MigrationProductionPackageTrustResult
        {
            Status = result.Status switch
            {
                OfficialUninstallProductionPackageTrustStatus.Trusted =>
                    MigrationProductionPackageTrustStatus.Trusted,
                OfficialUninstallProductionPackageTrustStatus.WorkerIdentityMismatch =>
                    MigrationProductionPackageTrustStatus.WorkerIdentityMismatch,
                OfficialUninstallProductionPackageTrustStatus.ClientImageUnavailable =>
                    MigrationProductionPackageTrustStatus.ClientImageUnavailable,
                OfficialUninstallProductionPackageTrustStatus.WorkerImageUnavailable =>
                    MigrationProductionPackageTrustStatus.WorkerImageUnavailable,
                OfficialUninstallProductionPackageTrustStatus.ClientNotTrusted =>
                    MigrationProductionPackageTrustStatus.ClientNotTrusted,
                OfficialUninstallProductionPackageTrustStatus.WorkerNotTrusted =>
                    MigrationProductionPackageTrustStatus.WorkerNotTrusted,
                OfficialUninstallProductionPackageTrustStatus.SignerMismatch =>
                    MigrationProductionPackageTrustStatus.SignerMismatch,
                _ => MigrationProductionPackageTrustStatus.WorkerNotTrusted
            }
        };
    }
}
