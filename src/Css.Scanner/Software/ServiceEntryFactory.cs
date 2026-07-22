namespace Css.Scanner.Software;

public static class ServiceEntryFactory
{
    public static ServiceEntry? FromRegistryValues(
        string name,
        string? displayName,
        string? imagePath,
        object? startValue = null)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(imagePath))
            return null;

        return new ServiceEntry(
            name.Trim(),
            string.IsNullOrWhiteSpace(displayName) ? name.Trim() : displayName.Trim(),
            imagePath.Trim(),
            ParseStartMode(startValue));
    }

    private static string? ParseStartMode(object? value)
    {
        if (value is null)
            return null;

        try
        {
            return Convert.ToInt32(value) switch
            {
                0 => "Boot",
                1 => "System",
                2 => "Automatic",
                3 => "Manual",
                4 => "Disabled",
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }
}
