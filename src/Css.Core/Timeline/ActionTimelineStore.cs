using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Css.Core.Operations;
using Microsoft.Data.Sqlite;

namespace Css.Core.Timeline;

public sealed class ActionTimelineStore
{
    private readonly string _dbPath;

    public ActionTimelineStore(string dbPath)
    {
        _dbPath = dbPath;
    }

    public async Task AddAsync(ActionTimelineEntry entry, CancellationToken ct = default)
    {
        await using var connection = await OpenAsync(ct);
        await EnsureSchemaAsync(connection, ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO action_timeline (
                occurred_at,
                source,
                title,
                evidence_summary,
                affected_paths_json,
                affected_registry_keys_json,
                restore_state,
                restore_operation_kind,
                restore_manifest_paths_json
            )
            VALUES (
                $occurred_at,
                $source,
                $title,
                $evidence_summary,
                $affected_paths_json,
                $affected_registry_keys_json,
                $restore_state,
                $restore_operation_kind,
                $restore_manifest_paths_json
            );
            """;
        command.Parameters.AddWithValue("$occurred_at", entry.OccurredAt.ToUnixTimeMilliseconds());
        command.Parameters.AddWithValue("$source", entry.Source.ToString());
        command.Parameters.AddWithValue("$title", entry.Title);
        command.Parameters.AddWithValue("$evidence_summary", entry.EvidenceSummary);
        command.Parameters.AddWithValue("$affected_paths_json", JsonSerializer.Serialize(entry.AffectedPaths));
        command.Parameters.AddWithValue("$affected_registry_keys_json", JsonSerializer.Serialize(entry.AffectedRegistryKeys));
        command.Parameters.AddWithValue("$restore_state", entry.RestoreState.ToString());
        command.Parameters.AddWithValue("$restore_operation_kind", (object?)entry.RestoreOperationKind ?? DBNull.Value);
        command.Parameters.AddWithValue("$restore_manifest_paths_json", JsonSerializer.Serialize(entry.RestoreManifestPaths));

        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateRestoreStateAsync(
        long id,
        RestoreState restoreState,
        string? restoreOperationKind,
        CancellationToken ct = default)
    {
        await using var connection = await OpenAsync(ct);
        await EnsureSchemaAsync(connection, ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE action_timeline
            SET restore_state = $restore_state,
                restore_operation_kind = $restore_operation_kind
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$restore_state", restoreState.ToString());
        command.Parameters.AddWithValue("$restore_operation_kind", (object?)restoreOperationKind ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<ActionTimelineEntry>> LoadRecentAsync(int limit, CancellationToken ct = default)
    {
        await using var connection = await OpenAsync(ct);
        await EnsureSchemaAsync(connection, ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, occurred_at, source, title, evidence_summary, affected_paths_json, affected_registry_keys_json, restore_state, restore_operation_kind, restore_manifest_paths_json
            FROM action_timeline
            ORDER BY occurred_at DESC, id DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 500));

        var entries = new List<ActionTimelineEntry>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            entries.Add(ReadEntry(reader));

        return entries;
    }

    public async Task<ActionTimelineEntry?> LoadByIdAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0)
            return null;

        await using var connection = await OpenAsync(ct);
        await EnsureSchemaAsync(connection, ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, occurred_at, source, title, evidence_summary, affected_paths_json, affected_registry_keys_json, restore_state, restore_operation_kind, restore_manifest_paths_json
            FROM action_timeline
            WHERE id = $id
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadEntry(reader) : null;
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

    private static async Task EnsureSchemaAsync(SqliteConnection connection, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS action_timeline (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                occurred_at INTEGER NOT NULL,
                source TEXT NOT NULL,
                title TEXT NOT NULL,
                evidence_summary TEXT NOT NULL,
                affected_paths_json TEXT NOT NULL,
                affected_registry_keys_json TEXT NOT NULL DEFAULT '[]',
                restore_state TEXT NOT NULL,
                restore_operation_kind TEXT NULL,
                restore_manifest_paths_json TEXT NOT NULL DEFAULT '[]'
            );

            CREATE INDEX IF NOT EXISTS ix_action_timeline_time
                ON action_timeline(occurred_at DESC, id DESC);
            """;
        await command.ExecuteNonQueryAsync(ct);

        await EnsureColumnAsync(
            connection,
            "affected_registry_keys_json",
            "ALTER TABLE action_timeline ADD COLUMN affected_registry_keys_json TEXT NOT NULL DEFAULT '[]';",
            ct);
        await EnsureColumnAsync(
            connection,
            "restore_manifest_paths_json",
            "ALTER TABLE action_timeline ADD COLUMN restore_manifest_paths_json TEXT NOT NULL DEFAULT '[]';",
            ct);
    }

    private static async Task EnsureColumnAsync(
        SqliteConnection connection,
        string columnName,
        string alterSql,
        CancellationToken ct)
    {
        await using var tableInfo = connection.CreateCommand();
        tableInfo.CommandText = "PRAGMA table_info(action_timeline);";

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await tableInfo.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            columns.Add(reader.GetString(1));

        if (columns.Contains(columnName))
            return;

        await using var alter = connection.CreateCommand();
        alter.CommandText = alterSql;
        await alter.ExecuteNonQueryAsync(ct);
    }

    private static IReadOnlyList<string> ParseStringList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static ActionTimelineEntry ReadEntry(SqliteDataReader reader) =>
        new()
        {
            Id = reader.GetInt64(0),
            OccurredAt = DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(1)),
            Source = ParseEnum(reader.GetString(2), OperationSource.Manual),
            Title = reader.GetString(3),
            EvidenceSummary = reader.GetString(4),
            AffectedPaths = ParseStringList(reader.GetString(5)),
            AffectedRegistryKeys = ParseStringList(reader.GetString(6)),
            RestoreState = ParseEnum(reader.GetString(7), RestoreState.Unknown),
            RestoreOperationKind = reader.IsDBNull(8) ? null : reader.GetString(8),
            RestoreManifestPaths = ParseStringList(reader.GetString(9))
        };

    private static TEnum ParseEnum<TEnum>(string value, TEnum fallback)
        where TEnum : struct
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed)
            ? parsed
            : fallback;
    }
}
