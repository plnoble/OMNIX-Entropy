using System;
using System.Collections.Generic;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using Css.Core.Recovery;

namespace Css.Scanner.Recovery;

public sealed class WindowsRestorePointScanner
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(4);

    private readonly Func<CancellationToken, IReadOnlyList<WindowsRestorePointInfo>> _source;
    private readonly TimeSpan _timeout;

    public WindowsRestorePointScanner()
        : this(ReadFromWindows, DefaultTimeout)
    {
    }

    public WindowsRestorePointScanner(
        Func<CancellationToken, IReadOnlyList<WindowsRestorePointInfo>> source)
        : this(source, DefaultTimeout)
    {
    }

    public WindowsRestorePointScanner(
        Func<CancellationToken, IReadOnlyList<WindowsRestorePointInfo>> source,
        TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));
        _source = source;
        _timeout = timeout;
    }

    public async Task<IReadOnlyList<WindowsRestorePointInfo>> ScanAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await ScanWithStatusAsync(cancellationToken).ConfigureAwait(false);
        return result.Points;
    }

    public async Task<WindowsRestorePointScanResult> ScanWithStatusAsync(
        CancellationToken cancellationToken = default)
    {
        using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);
        timeoutCancellation.CancelAfter(_timeout);
        var sourceTask = Task.Run(
            () => _source(timeoutCancellation.Token),
            CancellationToken.None);

        try
        {
            var points = await sourceTask.WaitAsync(_timeout, cancellationToken)
                .ConfigureAwait(false);
            return new WindowsRestorePointScanResult
            {
                State = WindowsRestorePointScanState.Completed,
                Points = Sort(points)
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return TimedOut();
        }
        catch (TimeoutException)
        {
            timeoutCancellation.Cancel();
            return TimedOut();
        }
        catch
        {
            return new WindowsRestorePointScanResult
            {
                State = WindowsRestorePointScanState.Failed,
                Points = []
            };
        }
    }

    private static IReadOnlyList<WindowsRestorePointInfo> Sort(
        IReadOnlyList<WindowsRestorePointInfo> points) =>
        points
            .OrderByDescending(point => point.CreatedAt)
            .ThenByDescending(point => point.SequenceNumber)
            .ToList();

    private static WindowsRestorePointScanResult TimedOut() =>
        new()
        {
            State = WindowsRestorePointScanState.TimedOut,
            Points = []
        };

    private static IReadOnlyList<WindowsRestorePointInfo> ReadFromWindows(
        CancellationToken cancellationToken)
    {
        var points = new List<WindowsRestorePointInfo>();
        try
        {
            var scope = new ManagementScope(
                @"\\.\root\default",
                new ConnectionOptions { Timeout = DefaultTimeout });
            scope.Connect();
            using var searcher = new ManagementObjectSearcher(
                scope,
                new ObjectQuery(
                    "SELECT SequenceNumber, Description, CreationTime, RestorePointType, EventType FROM SystemRestore"),
                new System.Management.EnumerationOptions
                {
                    Timeout = DefaultTimeout,
                    ReturnImmediately = false,
                    Rewindable = false
                });

            foreach (ManagementObject item in searcher.Get())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var creationTime = item["CreationTime"]?.ToString();
                if (string.IsNullOrWhiteSpace(creationTime))
                    continue;

                DateTime createdAt;
                try
                {
                    createdAt = ManagementDateTimeConverter.ToDateTime(creationTime);
                }
                catch
                {
                    continue;
                }

                points.Add(new WindowsRestorePointInfo
                {
                    SequenceNumber = Convert.ToInt64(item["SequenceNumber"] ?? 0),
                    Description = item["Description"]?.ToString() ?? string.Empty,
                    CreatedAt = new DateTimeOffset(createdAt),
                    RestorePointType = Convert.ToInt32(item["RestorePointType"] ?? 0),
                    EventType = Convert.ToInt32(item["EventType"] ?? 0)
                });
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // System Restore can be disabled or inaccessible; an empty read-only result is valid.
        }

        return points;
    }
}
