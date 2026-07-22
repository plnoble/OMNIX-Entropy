using System.Runtime.InteropServices;
using Css.Core.Quarantine;
using Microsoft.Win32.SafeHandles;

namespace Css.Win32.Quarantine;

public sealed class WindowsQuarantineCandidateIdentityReader : IQuarantineCandidateIdentityReader
{
    private const uint FileReadAttributes = 0x0080;
    private const uint FileShareRead = 0x00000001;
    private const uint FileShareWrite = 0x00000002;
    private const uint FileShareDelete = 0x00000004;
    private const uint OpenExisting = 3;
    private const uint FileFlagBackupSemantics = 0x02000000;
    private const uint FileFlagOpenReparsePoint = 0x00200000;

    public QuarantineCandidateInspection Inspect(string path)
    {
        if (!OperatingSystem.IsWindows())
            return QuarantineCandidateInspection.Refused("Windows 文件身份读取器只能在 Windows 上使用。");

        if (!QuarantineCandidatePathPolicy.TryInspectCurrentPath(
                path,
                out var canonical,
                out var kind,
                out var error))
        {
            return QuarantineCandidateInspection.Refused(error);
        }

        try
        {
            using var handle = CreateFileW(
                canonical,
                FileReadAttributes,
                FileShareRead | FileShareWrite | FileShareDelete,
                IntPtr.Zero,
                OpenExisting,
                FileFlagBackupSemantics | FileFlagOpenReparsePoint,
                IntPtr.Zero);
            if (handle.IsInvalid)
                return RefusedFromLastError();

            if (!GetFileInformationByHandle(handle, out var info))
                return RefusedFromLastError();
            if (((FileAttributes)info.FileAttributes & FileAttributes.ReparsePoint) != 0)
                return QuarantineCandidateInspection.Refused("隔离候选本身是重解析点，已拒绝。");

            var handleKind = ((FileAttributes)info.FileAttributes & FileAttributes.Directory) != 0
                ? QuarantineCandidateKind.Directory
                : QuarantineCandidateKind.File;
            if (handleKind != kind
                || QuarantineCandidatePathPolicy.HasAlternateDataStream(canonical)
                || QuarantineCandidatePathPolicy.HasReparsePointInExistingChain(canonical))
            {
                return QuarantineCandidateInspection.Refused("隔离候选在身份读取期间发生变化或经过重解析点。");
            }

            return QuarantineCandidateInspection.Accepted(new QuarantineCandidateEvidence
            {
                CanonicalPath = canonical,
                Kind = kind,
                VolumeSerialNumber = info.VolumeSerialNumber,
                FileId = ((ulong)info.FileIndexHigh << 32) | info.FileIndexLow,
                CreationTimeUtcTicks = ToLong(info.CreationTime),
                LastWriteTimeUtcTicks = ToLong(info.LastWriteTime),
                LengthBytes = kind == QuarantineCandidateKind.File
                    ? ((long)info.FileSizeHigh << 32) | info.FileSizeLow
                    : 0
            });
        }
        catch
        {
            return QuarantineCandidateInspection.Refused("隔离候选身份无法安全读取。");
        }
    }

    private static QuarantineCandidateInspection RefusedFromLastError()
    {
        _ = Marshal.GetLastWin32Error();
        return QuarantineCandidateInspection.Refused("隔离候选身份无法安全读取。");
    }

    private static long ToLong(NativeFileTime value) =>
        ((long)value.HighDateTime << 32) | value.LowDateTime;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFileW(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandle(
        SafeFileHandle hFile,
        out ByHandleFileInformation lpFileInformation);

    [StructLayout(LayoutKind.Sequential)]
    private struct ByHandleFileInformation
    {
        public uint FileAttributes;
        public NativeFileTime CreationTime;
        public NativeFileTime LastAccessTime;
        public NativeFileTime LastWriteTime;
        public uint VolumeSerialNumber;
        public uint FileSizeHigh;
        public uint FileSizeLow;
        public uint NumberOfLinks;
        public uint FileIndexHigh;
        public uint FileIndexLow;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeFileTime
    {
        public uint LowDateTime;
        public uint HighDateTime;
    }
}
