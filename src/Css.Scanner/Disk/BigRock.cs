using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;

namespace Css.Scanner.Disk;

/// <summary>A single "big rock" system file/store that occupies space without crawling.</summary>
public sealed class BigRock
{
    public string Name { get; init; } = "";
    public long SizeBytes { get; init; }
    public string? Note { get; init; }
    public bool NeedsAdmin { get; init; }
}

/// <summary>
/// Top-down probe of large system files/stores that are super-hidden or hardlinked-inflated,
/// so Explorer/FileInfo can't read them directly. Uses WMI where FileInfo fails and degrades
/// gracefully when admin is unavailable (needs-admin entries are still reported with a flag).
/// </summary>
public sealed class BigRocksProbe
{
    public List<BigRock> Probe(string systemRoot)
    {
        var rocks = new List<BigRock>();
        rocks.Add(GetPageFile());
        rocks.Add(GetHiberFile(systemRoot));
        rocks.Add(GetShadowStorage());
        rocks.Add(GetRecycleBin());
        rocks.RemoveAll(r => r.SizeBytes == 0 && r.Name is not "Hibernation file");
        return rocks;
    }

    private BigRock GetPageFile()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT AllocatedBaseSize FROM Win32_PageFileUsage");
            long total = 0;
            foreach (var o in searcher.Get())
            {
                var mb = o["AllocatedBaseSize"];
                if (mb != null && int.TryParse(mb.ToString(), out var m)) total += m * 1024L * 1024L;
            }
            return new BigRock { Name = "Page file (pagefile.sys / swapfile.sys)", SizeBytes = total };
        }
        catch (Exception ex)
        {
            return new BigRock { Name = "Page file", SizeBytes = 0, Note = "WMI query failed: " + ex.Message, NeedsAdmin = true };
        }
    }

    private BigRock GetHiberFile(string systemRoot)
    {
        // hiberfil.sys is super-hidden; FileInfo may throw or report 0. Try anyway, degrade gracefully.
        try
        {
            var path = Path.Combine(systemRoot, "hiberfil.sys");
            var fi = new FileInfo(path);
            if (fi.Exists) return new BigRock { Name = "Hibernation file (hiberfil.sys)", SizeBytes = fi.Length };
        }
        catch { /* access denied when not elevated */ }
        return new BigRock { Name = "Hibernation file", SizeBytes = 0, Note = "Needs admin to read (or hibernation disabled)", NeedsAdmin = true };
    }

    private BigRock GetShadowStorage()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT AllocatedSpace FROM Win32_ShadowStorage");
            long total = 0;
            foreach (var o in searcher.Get())
            {
                var v = o["AllocatedSpace"];
                if (v != null && long.TryParse(v.ToString(), out var n)) total += n;
            }
            return new BigRock
            {
                Name = "System Volume Information (shadow copies / restore points)",
                SizeBytes = total,
                NeedsAdmin = total == 0
            };
        }
        catch (Exception ex)
        {
            return new BigRock { Name = "Shadow storage", SizeBytes = 0, Note = "WMI query failed: " + ex.Message, NeedsAdmin = true };
        }
    }

    private BigRock GetRecycleBin()
    {
        // SHQueryRecycleBin per drive gives size of the recycle bin for the current user.
        try
        {
            var info = new SHQUERYRBINFO { cbSize = (uint)Marshal.SizeOf<SHQUERYRBINFO>() };
            if (SHQueryRecycleBin("C:\\", ref info) == 0)
            {
                long size = info.i64Size;
                return new BigRock { Name = "Recycle Bin (C:)", SizeBytes = size };
            }
        }
        catch { /* ignore */ }
        return new BigRock { Name = "Recycle Bin (C:)", SizeBytes = 0, Note = "Unable to query" };
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SHQUERYRBINFO
    {
        public uint cbSize;
        public long i64Size;
        public long i64NumItems;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);
}
