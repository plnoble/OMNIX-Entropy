using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Css.Win32.Security;

public enum AuthenticodeSignatureStatus
{
    Trusted,
    Missing,
    NotSigned,
    Invalid,
    Untrusted,
    ProbeFailed
}

public sealed record AuthenticodeSignatureEvidence
{
    public required AuthenticodeSignatureStatus Status { get; init; }
    public string? SignerSubject { get; init; }
    public string? SignerThumbprint { get; init; }
    public string? FileSha256 { get; init; }
    public bool IsTrusted =>
        Status == AuthenticodeSignatureStatus.Trusted
        && !string.IsNullOrWhiteSpace(SignerThumbprint)
        && IsSha256(FileSha256);

    private static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public interface IAuthenticodeSignatureVerifier
{
    AuthenticodeSignatureEvidence Verify(string filePath);
}

public sealed class WindowsAuthenticodeSignatureVerifier : IAuthenticodeSignatureVerifier
{
    private static readonly Guid GenericVerifyV2 =
        new("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

    private const uint WtdUiNone = 2;
    private const uint WtdRevokeWholeChain = 1;
    private const uint WtdChoiceFile = 1;
    private const uint WtdStateActionVerify = 1;
    private const uint WtdStateActionClose = 2;
    private const uint WtdRevocationCheckChainExcludeRoot = 0x80;
    private const uint WtdCacheOnlyUrlRetrieval = 0x1000;
    private const uint WtdDisableMd2Md4 = 0x2000;
    private const uint TrustENoSignature = 0x800B0100;
    private const uint TrustEProviderUnknown = 0x800B0001;
    private const uint TrustESubjectFormUnknown = 0x800B0003;
    private const uint TrustEBadDigest = 0x80096010;

    public AuthenticodeSignatureEvidence Verify(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Result(AuthenticodeSignatureStatus.Missing);

        string fullPath;
        string sha256;
        try
        {
            fullPath = Path.GetFullPath(filePath);
            if (!File.Exists(fullPath))
                return Result(AuthenticodeSignatureStatus.Missing);
            using var stream = File.Open(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            sha256 = Convert.ToHexString(SHA256.HashData(stream));
        }
        catch
        {
            return Result(AuthenticodeSignatureStatus.ProbeFailed);
        }

        var status = VerifyWindowsTrust(fullPath);
        if (status != AuthenticodeSignatureStatus.Trusted)
            return Result(status, sha256);

        try
        {
            using var certificate = X509Certificate.CreateFromSignedFile(fullPath);
            using var signer = new X509Certificate2(certificate);
            var thumbprint = NormalizeThumbprint(signer.Thumbprint);
            if (string.IsNullOrWhiteSpace(thumbprint))
                return Result(AuthenticodeSignatureStatus.Invalid, sha256);
            return new AuthenticodeSignatureEvidence
            {
                Status = AuthenticodeSignatureStatus.Trusted,
                SignerSubject = signer.Subject,
                SignerThumbprint = thumbprint,
                FileSha256 = sha256
            };
        }
        catch
        {
            return Result(AuthenticodeSignatureStatus.Invalid, sha256);
        }
    }

    private static AuthenticodeSignatureStatus VerifyWindowsTrust(string fullPath)
    {
        var pathPointer = IntPtr.Zero;
        var fileInfoPointer = IntPtr.Zero;
        var trustData = new WinTrustData
        {
            StructureSize = checked((uint)Marshal.SizeOf<WinTrustData>()),
            UiChoice = WtdUiNone,
            RevocationChecks = WtdRevokeWholeChain,
            UnionChoice = WtdChoiceFile,
            StateAction = WtdStateActionVerify,
            ProviderFlags = WtdRevocationCheckChainExcludeRoot
                | WtdCacheOnlyUrlRetrieval
                | WtdDisableMd2Md4
        };
        try
        {
            pathPointer = Marshal.StringToCoTaskMemUni(fullPath);
            var fileInfo = new WinTrustFileInfo
            {
                StructureSize = checked((uint)Marshal.SizeOf<WinTrustFileInfo>()),
                FilePath = pathPointer
            };
            fileInfoPointer = Marshal.AllocHGlobal(Marshal.SizeOf<WinTrustFileInfo>());
            Marshal.StructureToPtr(fileInfo, fileInfoPointer, fDeleteOld: false);
            trustData.FileInfo = fileInfoPointer;

            var action = GenericVerifyV2;
            var nativeStatus = unchecked((uint)WinVerifyTrust(
                new IntPtr(-1),
                ref action,
                ref trustData));
            return nativeStatus switch
            {
                0 => AuthenticodeSignatureStatus.Trusted,
                TrustENoSignature or TrustEProviderUnknown or TrustESubjectFormUnknown =>
                    AuthenticodeSignatureStatus.NotSigned,
                TrustEBadDigest => AuthenticodeSignatureStatus.Invalid,
                _ => AuthenticodeSignatureStatus.Untrusted
            };
        }
        catch
        {
            return AuthenticodeSignatureStatus.ProbeFailed;
        }
        finally
        {
            if (fileInfoPointer != IntPtr.Zero)
            {
                try
                {
                    trustData.StateAction = WtdStateActionClose;
                    var action = GenericVerifyV2;
                    _ = WinVerifyTrust(new IntPtr(-1), ref action, ref trustData);
                }
                catch
                {
                    // Trust state cleanup is best effort after a failed native call.
                }
                Marshal.DestroyStructure<WinTrustFileInfo>(fileInfoPointer);
                Marshal.FreeHGlobal(fileInfoPointer);
            }
            if (pathPointer != IntPtr.Zero)
                Marshal.FreeCoTaskMem(pathPointer);
        }
    }

    private static string? NormalizeThumbprint(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var normalized = new string(value.Where(Uri.IsHexDigit).ToArray()).ToUpperInvariant();
        return normalized.Length > 0 ? normalized : null;
    }

    private static AuthenticodeSignatureEvidence Result(
        AuthenticodeSignatureStatus status,
        string? sha256 = null) =>
        new() { Status = status, FileSha256 = sha256 };

    [DllImport("wintrust.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int WinVerifyTrust(
        IntPtr windowHandle,
        [In] ref Guid actionId,
        [In, Out] ref WinTrustData trustData);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WinTrustFileInfo
    {
        public uint StructureSize;
        public IntPtr FilePath;
        public IntPtr FileHandle;
        public IntPtr KnownSubject;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WinTrustData
    {
        public uint StructureSize;
        public IntPtr PolicyCallbackData;
        public IntPtr SipClientData;
        public uint UiChoice;
        public uint RevocationChecks;
        public uint UnionChoice;
        public IntPtr FileInfo;
        public uint StateAction;
        public IntPtr StateData;
        public IntPtr UrlReference;
        public uint ProviderFlags;
        public uint UiContext;
        public IntPtr SignatureSettings;
    }
}
