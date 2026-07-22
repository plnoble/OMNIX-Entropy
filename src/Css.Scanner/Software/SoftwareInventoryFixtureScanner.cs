using System.Text.Json;
using Css.Core.Software;

namespace Css.Scanner.Software;

public sealed class SoftwareInventoryFixtureScanner
{
    private readonly IReadOnlyList<IReadOnlyList<SoftwareProfile>> _scans;
    private int _nextScanIndex;

    private SoftwareInventoryFixtureScanner(IReadOnlyList<IReadOnlyList<SoftwareProfile>> scans)
    {
        _scans = scans;
    }

    public static SoftwareInventoryFixtureScanner? TryCreate(string? fixturePath)
    {
        if (string.IsNullOrWhiteSpace(fixturePath))
            return null;

        var json = File.ReadAllText(fixturePath);
        var document = JsonSerializer.Deserialize<SoftwareInventoryFixtureDocument>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (document is null || document.Scans.Count == 0)
            throw new InvalidDataException("Software inventory fixture must contain at least one scan.");

        return new SoftwareInventoryFixtureScanner(document.Scans);
    }

    public Task<IReadOnlyList<SoftwareProfile>> ScanAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var index = Math.Min(_nextScanIndex, _scans.Count - 1);
        _nextScanIndex++;
        return Task.FromResult(_scans[index]);
    }

    private sealed class SoftwareInventoryFixtureDocument
    {
        public List<List<SoftwareProfile>> Scans { get; init; } = [];
    }
}
