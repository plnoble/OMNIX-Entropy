using System;
using System.Linq;
using System.Xml.Linq;

namespace Css.Scanner.Software;

public static class ScheduledTaskXmlParser
{
    public static ScheduledTaskEntry? Parse(string taskName, string xml)
    {
        if (string.IsNullOrWhiteSpace(taskName) || string.IsNullOrWhiteSpace(xml))
            return null;

        try
        {
            var document = XDocument.Parse(xml);
            var command = document
                .Descendants()
                .Where(element => element.Name.LocalName.Equals("Exec", StringComparison.OrdinalIgnoreCase))
                .Select(exec => exec
                    .Elements()
                    .FirstOrDefault(element => element.Name.LocalName.Equals("Command", StringComparison.OrdinalIgnoreCase))
                    ?.Value)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

            if (string.IsNullOrWhiteSpace(command))
                return null;

            var settings = document
                .Descendants()
                .FirstOrDefault(element => element.Name.LocalName.Equals("Settings", StringComparison.OrdinalIgnoreCase));
            var enabledText = settings?
                .Elements()
                .FirstOrDefault(element => element.Name.LocalName.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
                ?.Value;
            bool? isEnabled = bool.TryParse(enabledText, out var enabled)
                ? enabled
                : null;

            return new ScheduledTaskEntry(taskName, NormalizeCommand(command), isEnabled);
        }
        catch
        {
            return null;
        }
    }

    private static string NormalizeCommand(string command)
    {
        var trimmed = command.Trim().Trim('"');
        var exeIndex = trimmed.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        return exeIndex >= 0 ? trimmed[..(exeIndex + 4)] : trimmed;
    }
}
