using System.Text;
using System.Security.Cryptography;
using Css.Win32.Security;

namespace Css.InstallGuard.Installers;

public enum InstallerPackageInspectionStatus
{
    Ready,
    Missing,
    Unsupported,
    ProbeFailed
}

public enum InstallerKindConfidence
{
    High,
    Medium,
    Low,
    Unknown
}

public sealed record InstallerPackageEvidence
{
    public required InstallerPackageInspectionStatus Status { get; init; }
    public required string PackagePath { get; init; }
    public string? FileName { get; init; }
    public long LengthBytes { get; init; }
    public DateTimeOffset? LastWriteUtc { get; init; }
    public string? Sha256 { get; init; }
    public AuthenticodeSignatureStatus SignatureStatus { get; init; }
    public string? SignerSubject { get; init; }
    public InstallerKind DetectedKind { get; init; } = InstallerKind.Unknown;
    public InstallerKindConfidence KindConfidence { get; init; }
    public IReadOnlyList<string> KindEvidence { get; init; } = [];

    public bool HasStableIdentity =>
        Status == InstallerPackageInspectionStatus.Ready
        && LengthBytes > 0
        && LastWriteUtc.HasValue
        && Sha256 is { Length: 64 }
        && Sha256.All(Uri.IsHexDigit);
}

public interface IInstallerPackageInspector
{
    InstallerPackageEvidence Inspect(string packagePath);
}

public static class InstallerPackagePathPolicy
{
    public static bool TryResolveFixedLocalPath(
        string? packagePath,
        out string fullPath)
    {
        fullPath = string.Empty;
        try
        {
            if (string.IsNullOrWhiteSpace(packagePath)
                || packagePath.StartsWith("\\\\", StringComparison.Ordinal)
                || !Path.IsPathFullyQualified(packagePath))
            {
                return false;
            }

            var candidate = Path.GetFullPath(packagePath);
            var root = Path.GetPathRoot(candidate);
            if (string.IsNullOrWhiteSpace(root)
                || !root.EndsWith(":\\", StringComparison.Ordinal)
                || candidate.AsSpan(root.Length).Contains(':'))
            {
                return false;
            }

            var drive = new DriveInfo(root);
            if (!drive.IsReady || drive.DriveType != DriveType.Fixed)
                return false;

            fullPath = candidate;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsExistingFileWithoutReparsePoints(string fullPath)
    {
        try
        {
            if (!File.Exists(fullPath)
                || File.GetAttributes(fullPath).HasFlag(FileAttributes.ReparsePoint))
            {
                return false;
            }

            var directory = new DirectoryInfo(
                Path.GetDirectoryName(fullPath)
                    ?? throw new InvalidOperationException("Package directory is unavailable."));
            while (directory is not null)
            {
                if (directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    return false;
                directory = directory.Parent;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public sealed class WindowsInstallerPackageInspector : IInstallerPackageInspector
{
    private const int MaximumMarkerBytesPerEdge = 4 * 1024 * 1024;
    private static readonly string[] SupportedExtensions = [".exe", ".msi", ".msix", ".appx"];

    private readonly IAuthenticodeSignatureVerifier _signatures;

    public WindowsInstallerPackageInspector(
        IAuthenticodeSignatureVerifier? signatures = null)
    {
        _signatures = signatures ?? new WindowsAuthenticodeSignatureVerifier();
    }

    public InstallerPackageEvidence Inspect(string packagePath)
    {
        if (string.IsNullOrWhiteSpace(packagePath))
            return Failed(InstallerPackageInspectionStatus.Missing, packagePath ?? string.Empty);

        if (!InstallerPackagePathPolicy.TryResolveFixedLocalPath(
                packagePath,
                out var fullPath))
        {
            return Failed(InstallerPackageInspectionStatus.Unsupported, packagePath);
        }

        if (!File.Exists(fullPath))
            return Failed(InstallerPackageInspectionStatus.Missing, fullPath);
        if (!InstallerPackagePathPolicy.IsExistingFileWithoutReparsePoints(fullPath))
            return Failed(InstallerPackageInspectionStatus.Unsupported, fullPath);
        var extension = Path.GetExtension(fullPath);
        if (!SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return Failed(InstallerPackageInspectionStatus.Unsupported, fullPath);

        try
        {
            var before = new FileInfo(fullPath);
            if (before.Length <= 0)
                return Failed(InstallerPackageInspectionStatus.ProbeFailed, fullPath);

            using var package = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                64 * 1024,
                FileOptions.SequentialScan);
            var detection = DetectKind(package, extension);
            package.Position = 0;
            var inspectedHash = Convert.ToHexString(SHA256.HashData(package));

            // Keep the read handle open without write/delete sharing while Windows
            // verifies the same path, then require both hash observations to agree.
            var signature = _signatures.Verify(fullPath);
            var after = new FileInfo(fullPath);
            if (before.Length != after.Length
                || before.LastWriteTimeUtc != after.LastWriteTimeUtc
                || signature.FileSha256 is not { Length: 64 } hash
                || !hash.All(Uri.IsHexDigit)
                || !string.Equals(hash, inspectedHash, StringComparison.OrdinalIgnoreCase))
            {
                return Failed(InstallerPackageInspectionStatus.ProbeFailed, fullPath);
            }

            return new InstallerPackageEvidence
            {
                Status = InstallerPackageInspectionStatus.Ready,
                PackagePath = fullPath,
                FileName = after.Name,
                LengthBytes = after.Length,
                LastWriteUtc = new DateTimeOffset(after.LastWriteTimeUtc, TimeSpan.Zero),
                Sha256 = hash.ToUpperInvariant(),
                SignatureStatus = signature.Status,
                SignerSubject = signature.SignerSubject,
                DetectedKind = detection.Kind,
                KindConfidence = detection.Confidence,
                KindEvidence = detection.Evidence
            };
        }
        catch
        {
            return Failed(InstallerPackageInspectionStatus.ProbeFailed, fullPath);
        }
    }

    private static InstallerKindDetection DetectKind(Stream package, string extension)
    {
        if (extension.Equals(".msi", StringComparison.OrdinalIgnoreCase))
            return Detection(InstallerKind.Msi, InstallerKindConfidence.High, "MSI 文件扩展名");
        if (extension.Equals(".msix", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".appx", StringComparison.OrdinalIgnoreCase))
        {
            return Detection(InstallerKind.Msix, InstallerKindConfidence.High, "Windows 应用包扩展名");
        }

        var segments = ReadMarkerSegments(package);
        var matches = new List<(InstallerKind Kind, string Evidence)>();
        if (ContainsMarker(segments, "Inno Setup Setup Data"))
            matches.Add((InstallerKind.InnoSetup, "检测到 Inno Setup 安装包标记"));
        if (ContainsMarker(segments, "Nullsoft.NSIS")
            || ContainsMarker(segments, "Nullsoft Install System"))
        {
            matches.Add((InstallerKind.Nsis, "检测到 NSIS 安装包标记"));
        }
        if (ContainsMarker(segments, "WixBundle")
            || ContainsMarker(segments, "BurnBootstrapperApplication"))
        {
            matches.Add((InstallerKind.Burn, "检测到 WiX Burn 引导包标记"));
        }

        var distinct = matches
            .GroupBy(match => match.Kind)
            .Select(group => group.First())
            .ToArray();
        return distinct.Length == 1
            ? Detection(distinct[0].Kind, InstallerKindConfidence.High, distinct[0].Evidence)
            : distinct.Length > 1
                ? new InstallerKindDetection(
                    InstallerKind.Exe,
                    InstallerKindConfidence.Low,
                    ["安装包包含互相冲突的类型标记，只能按普通 EXE 引导"])
                : Detection(
                    InstallerKind.Exe,
                    InstallerKindConfidence.Low,
                    "没有检测到受支持的安装器类型标记");
    }

    private static IReadOnlyList<byte[]> ReadMarkerSegments(Stream stream)
    {
        stream.Position = 0;
        var firstLength = checked((int)Math.Min(stream.Length, MaximumMarkerBytesPerEdge));
        var first = new byte[firstLength];
        stream.ReadExactly(first);
        if (stream.Length <= MaximumMarkerBytesPerEdge * 2L)
            return [first];

        stream.Position = stream.Length - MaximumMarkerBytesPerEdge;
        var last = new byte[MaximumMarkerBytesPerEdge];
        stream.ReadExactly(last);
        return [first, last];
    }

    private static bool ContainsMarker(IReadOnlyList<byte[]> segments, string marker)
    {
        var ascii = Encoding.ASCII.GetBytes(marker);
        var unicode = Encoding.Unicode.GetBytes(marker);
        return segments.Any(segment =>
            segment.AsSpan().IndexOf(ascii) >= 0
            || segment.AsSpan().IndexOf(unicode) >= 0);
    }

    private static InstallerKindDetection Detection(
        InstallerKind kind,
        InstallerKindConfidence confidence,
        string evidence) =>
        new(kind, confidence, [evidence]);

    private static InstallerPackageEvidence Failed(
        InstallerPackageInspectionStatus status,
        string path) =>
        new()
        {
            Status = status,
            PackagePath = path,
            SignatureStatus = status == InstallerPackageInspectionStatus.Missing
                ? AuthenticodeSignatureStatus.Missing
                : AuthenticodeSignatureStatus.ProbeFailed,
            KindConfidence = InstallerKindConfidence.Unknown
        };

    private sealed record InstallerKindDetection(
        InstallerKind Kind,
        InstallerKindConfidence Confidence,
        IReadOnlyList<string> Evidence);
}
