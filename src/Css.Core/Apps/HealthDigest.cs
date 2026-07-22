using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Css.Core.Apps;

public sealed class HealthDigest
{
    public required string ScanIdentity { get; init; }
    public DateTimeOffset CapturedAt { get; init; }
    public int OverallScore { get; init; }
    public required string Headline { get; init; }
    public required string Summary { get; init; }
    public IReadOnlyList<string> KeyFindings { get; init; } = [];
    public bool CanExecuteDirectly => false;
}

public sealed class HealthDigestStore
{
    public const int MaximumDigests = 90;
    private const int MaximumFindings = 5;
    private static readonly Regex LocalPathPattern = new(
        @"(?:[A-Za-z]:\\|\\\\)",
        RegexOptions.Compiled);
    private readonly string _dbPath;

    public HealthDigestStore(string dbPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dbPath);
        _dbPath = Path.GetFullPath(dbPath);
    }

    public async Task SaveAsync(HealthDigest digest, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(digest);
        Validate(digest);
        await using var connection = await OpenAsync(ct);
        await EnsureSchemaAsync(connection, ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        await using var upsert = connection.CreateCommand();
        upsert.Transaction = (SqliteTransaction)transaction;
        upsert.CommandText = """
            INSERT INTO health_digests (
                scan_identity,
                captured_at,
                overall_score,
                headline,
                summary,
                key_findings_json)
            VALUES (
                $scan_identity,
                $captured_at,
                $overall_score,
                $headline,
                $summary,
                $key_findings_json)
            ON CONFLICT(scan_identity) DO UPDATE SET
                captured_at = excluded.captured_at,
                overall_score = excluded.overall_score,
                headline = excluded.headline,
                summary = excluded.summary,
                key_findings_json = excluded.key_findings_json;
            """;
        upsert.Parameters.AddWithValue("$scan_identity", digest.ScanIdentity);
        upsert.Parameters.AddWithValue("$captured_at", digest.CapturedAt.ToUnixTimeMilliseconds());
        upsert.Parameters.AddWithValue("$overall_score", digest.OverallScore);
        upsert.Parameters.AddWithValue("$headline", digest.Headline);
        upsert.Parameters.AddWithValue("$summary", digest.Summary);
        upsert.Parameters.AddWithValue("$key_findings_json", JsonSerializer.Serialize(digest.KeyFindings));
        await upsert.ExecuteNonQueryAsync(ct);

        await using var trim = connection.CreateCommand();
        trim.Transaction = (SqliteTransaction)transaction;
        trim.CommandText = """
            DELETE FROM health_digests
            WHERE scan_identity IN (
                SELECT scan_identity
                FROM health_digests
                ORDER BY captured_at DESC, scan_identity DESC
                LIMIT -1 OFFSET $keep_count
            );
            """;
        trim.Parameters.AddWithValue("$keep_count", MaximumDigests);
        await trim.ExecuteNonQueryAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<HealthDigest>> LoadRecentAsync(
        int limit = 14,
        CancellationToken ct = default)
    {
        var boundedLimit = Math.Clamp(limit, 1, MaximumDigests);
        await using var connection = await OpenAsync(ct);
        await EnsureSchemaAsync(connection, ct);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT scan_identity, captured_at, overall_score, headline, summary, key_findings_json
            FROM health_digests
            ORDER BY captured_at DESC, scan_identity DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", boundedLimit);

        var result = new List<HealthDigest>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            try
            {
                var digest = new HealthDigest
                {
                    ScanIdentity = reader.GetString(0),
                    CapturedAt = DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(1)),
                    OverallScore = reader.GetInt32(2),
                    Headline = reader.GetString(3),
                    Summary = reader.GetString(4),
                    KeyFindings = DeserializeFindings(reader.GetString(5))
                };
                Validate(digest);
                result.Add(digest);
            }
            catch (Exception ex) when (ex is InvalidDataException
                or ArgumentOutOfRangeException
                or InvalidCastException
                or FormatException
                or OverflowException)
            {
                // Ignore a malformed local row instead of showing unsafe text.
            }
        }
        return result;
    }

    private async Task<SqliteConnection> OpenAsync(CancellationToken ct)
    {
        var directory = Path.GetDirectoryName(_dbPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _dbPath,
            Pooling = false
        }.ToString());
        await connection.OpenAsync(ct);
        return connection;
    }

    private static async Task EnsureSchemaAsync(
        SqliteConnection connection,
        CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS health_digests (
                scan_identity TEXT PRIMARY KEY,
                captured_at INTEGER NOT NULL,
                overall_score INTEGER NOT NULL,
                headline TEXT NOT NULL,
                summary TEXT NOT NULL,
                key_findings_json TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS ix_health_digests_time
                ON health_digests(captured_at DESC);
            """;
        await command.ExecuteNonQueryAsync(ct);
    }

    private static IReadOnlyList<string> DeserializeFindings(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static void Validate(HealthDigest digest)
    {
        if (string.IsNullOrWhiteSpace(digest.ScanIdentity)
            || digest.ScanIdentity.Length > 128)
        {
            throw new InvalidDataException("Health digest identity is invalid.");
        }
        if (digest.OverallScore is < 0 or > 100)
            throw new InvalidDataException("Health digest score is invalid.");
        ValidateVisibleText(digest.Headline, 300, "headline");
        ValidateVisibleText(digest.Summary, 1000, "summary");
        if (digest.KeyFindings.Count > MaximumFindings)
            throw new InvalidDataException("Health digest contains too many findings.");
        foreach (var finding in digest.KeyFindings)
            ValidateVisibleText(finding, 700, "finding");
    }

    private static void ValidateVisibleText(string text, int maximumLength, string field)
    {
        if (string.IsNullOrWhiteSpace(text)
            || text.Length > maximumLength
            || LocalPathPattern.IsMatch(text))
        {
            throw new InvalidDataException($"Health digest {field} is invalid or contains a local path.");
        }
    }
}

public sealed class HealthDigestRowViewModel
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
}

public sealed class HealthDigestHistoryViewModel
{
    public required string LatestHeadline { get; init; }
    public required string LatestSummary { get; init; }
    public required string WeeklySummary { get; init; }
    public required string MonitoringNotice { get; init; }
    public IReadOnlyList<HealthDigestRowViewModel> DailyRows { get; init; } = [];
    public bool HasEvidence { get; init; }
    public bool CanExecuteDirectly => false;
}

public static class HealthDigestHistoryPresenter
{
    public static HealthDigestHistoryViewModel Create(
        IReadOnlyList<HealthDigest> digests,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(digests);
        var daily = digests
            .OrderByDescending(item => item.CapturedAt)
            .GroupBy(item => item.CapturedAt.ToLocalTime().Date)
            .Select(group => group.First())
            .Take(7)
            .ToArray();
        if (daily.Length == 0)
        {
            return new HealthDigestHistoryViewModel
            {
                LatestHeadline = "还没有体检历史",
                LatestSummary = "完成一次手动体检后，这里会保存不含本机路径的摘要。",
                WeeklySummary = "近 7 天暂无真实扫描记录。",
                MonitoringNotice = "当前没有后台定时扫描；只有你主动体检时才会新增记录。",
                HasEvidence = false
            };
        }

        var latest = daily[0];
        var weeklyCutoff = now.AddDays(-7);
        var weekly = daily
            .Where(item => item.CapturedAt >= weeklyCutoff)
            .OrderBy(item => item.CapturedAt)
            .ToArray();
        var weeklySummary = weekly.Length <= 1
            ? "近 7 天只有 1 次真实体检，暂时无法判断趋势。"
            : ScoreTrend(weekly[0].OverallScore, weekly[^1].OverallScore, weekly.Length);

        return new HealthDigestHistoryViewModel
        {
            LatestHeadline = latest.CapturedAt.ToLocalTime().Date == now.ToLocalTime().Date
                ? "今日体检：" + latest.Headline
                : "最近体检：" + latest.Headline,
            LatestSummary = latest.Summary,
            WeeklySummary = weeklySummary,
            MonitoringNotice = "这是手动体检历史，不代表后台持续监控；没有自动执行任何处理。",
            DailyRows = daily.Select(item => new HealthDigestRowViewModel
            {
                Title = item.CapturedAt.ToLocalTime().ToString("MM-dd HH:mm") + $" · {item.OverallScore} 分",
                Summary = item.KeyFindings.FirstOrDefault() ?? item.Summary
            }).ToArray(),
            HasEvidence = true
        };
    }

    private static string ScoreTrend(int earliest, int latest, int count)
    {
        var delta = latest - earliest;
        if (delta > 0)
            return $"近 7 天有 {count} 次真实体检，综合评分提高 {delta} 分。";
        if (delta < 0)
            return $"近 7 天有 {count} 次真实体检，综合评分下降 {Math.Abs(delta)} 分，建议查看最新发现。";
        return $"近 7 天有 {count} 次真实体检，综合评分保持不变。";
    }
}
