using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Css.Core.Apps;

public static class BeginnerTextSanitizer
{
    private const string LocalPathReplacement = "\u67d0\u4e2a\u672c\u673a\u4f4d\u7f6e";

    private static readonly Regex ContextualLocalPathPattern = new(
        @"(?<![A-Za-z0-9])[A-Za-z]:\\.+?(?=\s+(?:\u5360\u7528|\u5927\u5c0f|\u5305\u542b|\u7ea6\u4e3a|\u7ea6|\u5df2\u4f7f\u7528|\u9884\u8ba1|\u6765\u81ea|\u4f4d\u4e8e|\u98ce\u9669|\u53ef\u91ca\u653e)|[\uff0c\u3002\uff1b,;\r\n]|$)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex CompactLocalPathPattern = new(
        @"(?<![A-Za-z0-9])[A-Za-z]:\\[^\s\uff0c\u3002\uff1b,;\r\n]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string HideLocalPaths(
        string? text,
        IEnumerable<string>? knownPaths = null)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var sanitized = text;
        if (knownPaths is not null)
        {
            foreach (var path in knownPaths
                         .Where(IsLocalPath)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderByDescending(path => path.Length))
            {
                sanitized = sanitized.Replace(
                    path,
                    LocalPathReplacement,
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        sanitized = ContextualLocalPathPattern.Replace(sanitized, LocalPathReplacement);
        return CompactLocalPathPattern.Replace(sanitized, LocalPathReplacement);
    }

    private static bool IsLocalPath(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length >= 3 &&
        char.IsLetter(value[0]) &&
        value[1] == ':' &&
        value[2] == '\\';
}
