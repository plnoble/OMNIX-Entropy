using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Css.Scanner.Disk;

/// <summary>
/// Produces a compact human + LLM-readable summary of a <see cref="DriveScanResult"/>. This is what
/// the agent's <c>diagnose_c_drive</c> tool returns, so it must stay small enough for an LLM context
/// (top consumers + unexpected roots + big rocks) rather than dumping the full tree.
/// </summary>
public static class RootCauseReportBuilder
{
    public static string Build(DriveScanResult r)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"=== C盘根因报告: {r.Drive} ===");
        sb.AppendLine(CultureInfo.InvariantCulture, $"总容量: {Fmt(r.TotalBytes)}  已用: {Fmt(r.UsedBytes)} ({Pct(r.UsedBytes, r.TotalBytes)}%)  剩余: {Fmt(r.FreeBytes)}");
        sb.AppendLine();

        sb.AppendLine("【非预期根目录文件夹】（重点排查）");
        var unexpected = r.UnexpectedRoots.OrderByDescending(n => n.SizeBytes).ToList();
        if (unexpected.Count == 0)
            sb.AppendLine("  无");
        else
            foreach (var n in unexpected)
                sb.AppendLine(CultureInfo.InvariantCulture, $"  - {n.Name}  {Fmt(n.SizeBytes)}");
        sb.AppendLine();

        sb.AppendLine("【Top 10 占用项】");
        var top = r.TopLevel.OrderByDescending(n => n.SizeBytes).Take(10).ToList();
        foreach (var n in top)
            sb.AppendLine(CultureInfo.InvariantCulture, $"  - {n.Name,-30} {Fmt(n.SizeBytes),12}  [{n.Category}]{(n.IsUnexpectedRoot ? "  ⚠非预期" : "")}");
        sb.AppendLine();

        sb.AppendLine("【系统大文件/存储】");
        foreach (var b in r.BigRocks)
            sb.AppendLine(CultureInfo.InvariantCulture, $"  - {b.Name,-45} {Fmt(b.SizeBytes),12}{(b.NeedsAdmin ? "  (需管理员)" : "")}");
        sb.AppendLine();

        sb.AppendLine("【分类汇总】");
        var byCat = r.TopLevel.GroupBy(n => n.Category)
            .Select(g => (Cat: g.Key, Size: g.Sum(n => n.SizeBytes)))
            .OrderByDescending(x => x.Size);
        foreach (var c in byCat)
            sb.AppendLine(CultureInfo.InvariantCulture, $"  - {c.Cat,-15} {Fmt(c.Size),12}");
        return sb.ToString();
    }

    public static string Fmt(long bytes) => bytes switch
    {
        >= 1L << 30 => (bytes / (double)(1L << 30)).ToString("0.0", CultureInfo.InvariantCulture) + " GB",
        >= 1L << 20 => (bytes / (double)(1L << 20)).ToString("0.0", CultureInfo.InvariantCulture) + " MB",
        >= 1L << 10 => (bytes / (double)(1L << 10)).ToString("0.0", CultureInfo.InvariantCulture) + " KB",
        _ => bytes.ToString(CultureInfo.InvariantCulture) + " B"
    };

    private static string Pct(long part, long whole) =>
        whole == 0 ? "0" : (part * 100.0 / whole).ToString("0.0", CultureInfo.InvariantCulture);
}
