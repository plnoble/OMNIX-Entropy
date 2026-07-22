using System.Diagnostics;
using System.Security.Principal;
using Css.Ipc.Uninstall;
using Css.Win32.Processes;
using Css.Win32.Security;

namespace Css.Elevated.Uninstall;

public enum OfficialUninstallProductionPackageTrustStatus
{
    Trusted,
    WorkerIdentityMismatch,
    ClientImageUnavailable,
    WorkerImageUnavailable,
    ClientNotTrusted,
    WorkerNotTrusted,
    SignerMismatch
}

public sealed record OfficialUninstallProductionPackageTrustResult
{
    public required OfficialUninstallProductionPackageTrustStatus Status { get; init; }
    public AuthenticodeSignatureEvidence? ClientEvidence { get; init; }
    public AuthenticodeSignatureEvidence? WorkerEvidence { get; init; }
    public bool CanAuthorize => Status == OfficialUninstallProductionPackageTrustStatus.Trusted;
}

public interface IOfficialUninstallProductionPackageAuthorizer
{
    OfficialUninstallProductionPackageTrustResult Authorize(
        OfficialUninstallPipePeerIdentity actualClient,
        OfficialUninstallPipePeerIdentity worker);
}

public sealed class WindowsOfficialUninstallProductionPackageAuthorizer
    : IOfficialUninstallProductionPackageAuthorizer
{
    private readonly IWindowsProcessImagePathResolver _pathResolver;
    private readonly IAuthenticodeSignatureVerifier _signatureVerifier;

    public WindowsOfficialUninstallProductionPackageAuthorizer(
        IWindowsProcessImagePathResolver pathResolver,
        IAuthenticodeSignatureVerifier signatureVerifier)
    {
        _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
        _signatureVerifier = signatureVerifier
            ?? throw new ArgumentNullException(nameof(signatureVerifier));
    }

    public OfficialUninstallProductionPackageTrustResult Authorize(
        OfficialUninstallPipePeerIdentity actualClient,
        OfficialUninstallPipePeerIdentity worker)
    {
        ArgumentNullException.ThrowIfNull(actualClient);
        ArgumentNullException.ThrowIfNull(worker);
        if (!IsCurrentWorker(worker))
            return Result(OfficialUninstallProductionPackageTrustStatus.WorkerIdentityMismatch);

        string clientPath;
        try
        {
            clientPath = _pathResolver.Resolve(actualClient.ProcessId);
        }
        catch
        {
            return Result(OfficialUninstallProductionPackageTrustStatus.ClientImageUnavailable);
        }

        string workerPath;
        try
        {
            workerPath = _pathResolver.Resolve(worker.ProcessId);
        }
        catch
        {
            return Result(OfficialUninstallProductionPackageTrustStatus.WorkerImageUnavailable);
        }

        var clientEvidence = _signatureVerifier.Verify(clientPath);
        if (!clientEvidence.IsTrusted)
        {
            return Result(
                OfficialUninstallProductionPackageTrustStatus.ClientNotTrusted,
                clientEvidence);
        }

        var workerEvidence = _signatureVerifier.Verify(workerPath);
        if (!workerEvidence.IsTrusted)
        {
            return Result(
                OfficialUninstallProductionPackageTrustStatus.WorkerNotTrusted,
                clientEvidence,
                workerEvidence);
        }

        return string.Equals(
                clientEvidence.SignerThumbprint,
                workerEvidence.SignerThumbprint,
                StringComparison.OrdinalIgnoreCase)
            ? Result(
                OfficialUninstallProductionPackageTrustStatus.Trusted,
                clientEvidence,
                workerEvidence)
            : Result(
                OfficialUninstallProductionPackageTrustStatus.SignerMismatch,
                clientEvidence,
                workerEvidence);
    }

    private static bool IsCurrentWorker(OfficialUninstallPipePeerIdentity worker)
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            using var process = Process.GetCurrentProcess();
            return worker.ProcessId == Environment.ProcessId
                && worker.WindowsSessionId == process.SessionId
                && string.Equals(
                    worker.UserSid,
                    identity.User?.Value,
                    StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static OfficialUninstallProductionPackageTrustResult Result(
        OfficialUninstallProductionPackageTrustStatus status,
        AuthenticodeSignatureEvidence? client = null,
        AuthenticodeSignatureEvidence? worker = null) =>
        new()
        {
            Status = status,
            ClientEvidence = client,
            WorkerEvidence = worker
        };
}
