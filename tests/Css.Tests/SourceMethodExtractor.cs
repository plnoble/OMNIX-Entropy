namespace Css.Tests;

internal static class SourceMethodExtractor
{
    public static string Extract(string source, string declaration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(declaration);
        if (!declaration.Contains('('))
            throw new ArgumentException("A full method declaration prefix is required.", nameof(declaration));

        var start = source.IndexOf(declaration, StringComparison.Ordinal);
        if (start < 0)
            throw new InvalidOperationException("Method declaration was not found: " + declaration);
        var openingBrace = source.IndexOf('{', start + declaration.Length);
        if (openingBrace < 0)
            throw new InvalidOperationException("Method body was not found: " + declaration);

        var depth = 0;
        var state = LexicalState.Code;
        for (var index = openingBrace; index < source.Length; index++)
        {
            var current = source[index];
            var next = index + 1 < source.Length ? source[index + 1] : '\0';
            switch (state)
            {
                case LexicalState.LineComment:
                    if (current is '\r' or '\n')
                        state = LexicalState.Code;
                    continue;
                case LexicalState.BlockComment:
                    if (current == '*' && next == '/')
                    {
                        state = LexicalState.Code;
                        index++;
                    }
                    continue;
                case LexicalState.String:
                    if (current == '\\')
                    {
                        index++;
                    }
                    else if (current == '"')
                    {
                        state = LexicalState.Code;
                    }
                    continue;
                case LexicalState.VerbatimString:
                    if (current == '"' && next == '"')
                    {
                        index++;
                    }
                    else if (current == '"')
                    {
                        state = LexicalState.Code;
                    }
                    continue;
                case LexicalState.Character:
                    if (current == '\\')
                    {
                        index++;
                    }
                    else if (current == '\'')
                    {
                        state = LexicalState.Code;
                    }
                    continue;
            }

            if (current == '/' && next == '/')
            {
                state = LexicalState.LineComment;
                index++;
                continue;
            }
            if (current == '/' && next == '*')
            {
                state = LexicalState.BlockComment;
                index++;
                continue;
            }
            if (current == '"')
            {
                var isVerbatim = index > 0 && source[index - 1] == '@';
                state = isVerbatim ? LexicalState.VerbatimString : LexicalState.String;
                continue;
            }
            if (current == '\'')
            {
                state = LexicalState.Character;
                continue;
            }
            if (current == '{')
            {
                depth++;
            }
            else if (current == '}' && --depth == 0)
            {
                return source[start..(index + 1)];
            }
        }

        throw new InvalidOperationException("Method body is unbalanced: " + declaration);
    }

    private enum LexicalState
    {
        Code,
        LineComment,
        BlockComment,
        String,
        VerbatimString,
        Character
    }
}
