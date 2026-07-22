using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Css.Win32.Processes;

public interface IWindowsProcessImagePathResolver
{
    string Resolve(int processId);
}

public sealed class WindowsProcessImagePathResolver : IWindowsProcessImagePathResolver
{
    private const uint ProcessQueryLimitedInformation = 0x1000;
    private const int MaximumWindowsPathCharacters = 32_768;

    public string Resolve(int processId)
    {
        if (processId <= 0)
            throw new ArgumentOutOfRangeException(nameof(processId));

        using var handle = OpenProcess(
            ProcessQueryLimitedInformation,
            inheritHandle: false,
            processId);
        if (handle.IsInvalid)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        var imagePath = new StringBuilder(MaximumWindowsPathCharacters);
        var length = (uint)imagePath.Capacity;
        if (!QueryFullProcessImageName(handle, 0, imagePath, ref length) || length == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        return Path.GetFullPath(imagePath.ToString(0, checked((int)length)));
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern SafeProcessHandle OpenProcess(
        uint desiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
        int processId);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool QueryFullProcessImageName(
        SafeProcessHandle process,
        uint flags,
        StringBuilder executablePath,
        ref uint size);
}
