using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Css.Scanner.Disk;
using Microsoft.Data.Sqlite;

namespace Css.Scanner.Persistence;

/// <summary>
/// SQLite storage for a bounded history of disk scan snapshots.
/// </summary>
public sealed class ScanSnapshotStore
{
    public const int MaximumSnapshotsPerDrive = 90;
    public const int DefaultTrendSnapshotCount = 8;
    private readonly string _dbPath;

    public ScanSnapshotStore(string dbPath)
    {
        _dbPath = dbPath;
    }

    public async Task SaveAsync(string driveRoot, ScanSnapshot snapshot, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(driveRoot);
        ArgumentNullException.ThrowIfNull(snapshot);
        ValidateSnapshot(snapshot);
        await using var connection = await OpenAsync(ct);
        await EnsureSchemaAsync(connection, ct);
        await using var tx = await connection.BeginTransactionAsync(ct);

        await using var insertScan = connection.CreateCommand();
        insertScan.Transaction = (SqliteTransaction)tx;
        insertScan.CommandText = """
            INSERT INTO scan_snapshots (drive_root, captured_at)
            VALUES ($drive_root, $captured_at);
            SELECT last_insert_rowid();
            """;
        insertScan.Parameters.AddWithValue("$drive_root", driveRoot);
        insertScan.Parameters.AddWithValue("$captured_at", snapshot.CapturedAt.ToUnixTimeMilliseconds());
        var scanId = (long)(await insertScan.ExecuteScalarAsync(ct) ?? 0L);
        if (scanId <= 0)
            throw new InvalidOperationException("Snapshot header was not persisted.");

        foreach (var item in snapshot.Items)
        {
            await using var insertItem = connection.CreateCommand();
            insertItem.Transaction = (SqliteTransaction)tx;
            insertItem.CommandText = """
                INSERT INTO scan_snapshot_items (scan_id, path, owner_software, size_bytes)
                VALUES ($scan_id, $path, $owner_software, $size_bytes);
                """;
            insertItem.Parameters.AddWithValue("$scan_id", scanId);
            insertItem.Parameters.AddWithValue("$path", item.Path);
            insertItem.Parameters.AddWithValue("$owner_software", item.OwnerSoftware);
            insertItem.Parameters.AddWithValue("$size_bytes", item.SizeBytes);
            await insertItem.ExecuteNonQueryAsync(ct);
        }

        await using var trimHistory = connection.CreateCommand();
        trimHistory.Transaction = (SqliteTransaction)tx;
        trimHistory.CommandText = """
            DELETE FROM scan_snapshots
            WHERE id IN (
                SELECT id
                FROM scan_snapshots
                WHERE drive_root = $drive_root
                ORDER BY captured_at DESC, id DESC
                LIMIT -1 OFFSET $keep_count
            );
            """;
        trimHistory.Parameters.AddWithValue("$drive_root", driveRoot);
        trimHistory.Parameters.AddWithValue("$keep_count", MaximumSnapshotsPerDrive);
        await trimHistory.ExecuteNonQueryAsync(ct);

        await tx.CommitAsync(ct);
    }

    public async Task<ScanSnapshot?> LoadLatestAsync(string driveRoot, CancellationToken ct = default)
    {
        var snapshots = await LoadRecentAsync(driveRoot, 1, ct);
        return snapshots.Count == 0 ? null : snapshots[0];
    }

    public async Task<IReadOnlyList<ScanSnapshot>> LoadRecentAsync(
        string driveRoot,
        int limit = DefaultTrendSnapshotCount,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(driveRoot);
        var boundedLimit = Math.Clamp(limit, 1, MaximumSnapshotsPerDrive);
        await using var connection = await OpenAsync(ct);
        await EnsureSchemaAsync(connection, ct);

        await using var scanCommand = connection.CreateCommand();
        scanCommand.CommandText = """
            SELECT id, captured_at
            FROM scan_snapshots
            WHERE drive_root = $drive_root
            ORDER BY captured_at DESC, id DESC
            LIMIT $limit;
            """;
        scanCommand.Parameters.AddWithValue("$drive_root", driveRoot);
        scanCommand.Parameters.AddWithValue("$limit", boundedLimit);

        var headers = new List<(long Id, DateTimeOffset CapturedAt)>();
        await using (var reader = await scanCommand.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                headers.Add((
                    reader.GetInt64(0),
                    DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(1))));
            }
        }

        var snapshots = new List<ScanSnapshot>(headers.Count);
        foreach (var header in headers)
        {
            var items = new List<ScanSnapshotItem>();
            await using var itemCommand = connection.CreateCommand();
            itemCommand.CommandText = """
                SELECT path, owner_software, size_bytes
                FROM scan_snapshot_items
                WHERE scan_id = $scan_id
                ORDER BY size_bytes DESC, path ASC;
                """;
            itemCommand.Parameters.AddWithValue("$scan_id", header.Id);

            await using var itemReader = await itemCommand.ExecuteReaderAsync(ct);
            while (await itemReader.ReadAsync(ct))
            {
                items.Add(new ScanSnapshotItem(
                    itemReader.GetString(0),
                    itemReader.GetString(1),
                    itemReader.GetInt64(2)));
            }
            snapshots.Add(new ScanSnapshot(header.CapturedAt, items));
        }

        return snapshots;
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
        await using var foreignKeys = connection.CreateCommand();
        foreignKeys.CommandText = "PRAGMA foreign_keys = ON;";
        await foreignKeys.ExecuteNonQueryAsync(ct);
        return connection;
    }

    private static void ValidateSnapshot(ScanSnapshot snapshot)
    {
        if (snapshot.Items.Count > ScanSnapshotBuilder.MaximumSnapshotItems)
            throw new InvalidDataException("Snapshot contains too many monitored locations.");
        foreach (var item in snapshot.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Path) || item.Path.Length > 32_768)
                throw new InvalidDataException("Snapshot contains an invalid monitored path.");
            if (string.IsNullOrWhiteSpace(item.OwnerSoftware) || item.OwnerSoftware.Length > 256)
                throw new InvalidDataException("Snapshot contains an invalid owner label.");
            if (item.SizeBytes < 0)
                throw new InvalidDataException("Snapshot contains a negative size.");
        }
    }

    private static async Task EnsureSchemaAsync(SqliteConnection connection, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS scan_snapshots (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                drive_root TEXT NOT NULL,
                captured_at INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS scan_snapshot_items (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                scan_id INTEGER NOT NULL,
                path TEXT NOT NULL,
                owner_software TEXT NOT NULL,
                size_bytes INTEGER NOT NULL,
                FOREIGN KEY(scan_id) REFERENCES scan_snapshots(id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS ix_scan_snapshots_drive_time
                ON scan_snapshots(drive_root, captured_at DESC);

            CREATE INDEX IF NOT EXISTS ix_scan_snapshot_items_scan
                ON scan_snapshot_items(scan_id);
            """;
        await command.ExecuteNonQueryAsync(ct);
    }
}
