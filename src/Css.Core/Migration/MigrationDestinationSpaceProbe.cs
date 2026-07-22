using System;
using System.IO;

namespace Css.Core.Migration;

public sealed class MigrationDestinationSpaceProbeResult
{
    public required string DestinationRoot { get; init; }
    public required string DriveRoot { get; init; }
    public required long RequiredBytes { get; init; }
    public long? AvailableBytes { get; init; }
    public required bool CanCheck { get; init; }
    public required bool HasEnoughSpace { get; init; }
    public required string Summary { get; init; }
    public string? Error { get; init; }
}

public static class MigrationDestinationSpaceProbe
{
    public static MigrationDestinationSpaceProbeResult CheckCurrentMachine(
        string destinationRoot,
        long requiredBytes) =>
        Check(destinationRoot, requiredBytes, driveRoot => new DriveInfo(driveRoot).AvailableFreeSpace);

    public static MigrationDestinationSpaceProbeResult Check(
        string destinationRoot,
        long requiredBytes,
        Func<string, long> availableBytesProvider)
    {
        ArgumentNullException.ThrowIfNull(availableBytesProvider);

        var normalizedDestination = string.IsNullOrWhiteSpace(destinationRoot)
            ? string.Empty
            : destinationRoot.Trim();
        var driveRoot = string.IsNullOrWhiteSpace(normalizedDestination)
            ? string.Empty
            : Path.GetPathRoot(normalizedDestination) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedDestination) || string.IsNullOrWhiteSpace(driveRoot))
        {
            return new MigrationDestinationSpaceProbeResult
            {
                DestinationRoot = normalizedDestination,
                DriveRoot = driveRoot,
                RequiredBytes = requiredBytes,
                CanCheck = false,
                HasEnoughSpace = false,
                Summary = "Destination drive cannot be identified.",
                Error = "Destination root is missing or invalid."
            };
        }

        try
        {
            var available = availableBytesProvider(driveRoot);
            var enough = available >= requiredBytes;
            return new MigrationDestinationSpaceProbeResult
            {
                DestinationRoot = normalizedDestination,
                DriveRoot = driveRoot,
                RequiredBytes = requiredBytes,
                AvailableBytes = available,
                CanCheck = true,
                HasEnoughSpace = enough,
                Summary = enough
                    ? "Destination has enough free space for this migration plan."
                    : "Destination has not enough free space for this migration plan."
            };
        }
        catch (Exception ex)
        {
            return new MigrationDestinationSpaceProbeResult
            {
                DestinationRoot = normalizedDestination,
                DriveRoot = driveRoot,
                RequiredBytes = requiredBytes,
                CanCheck = false,
                HasEnoughSpace = false,
                Summary = "Destination free space could not be checked.",
                Error = ex.Message
            };
        }
    }
}
