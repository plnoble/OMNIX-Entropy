using System.Security.Cryptography;
using System.Text;

namespace Css.Rules.Winapp2;

public sealed class Winapp2RuleCatalogLoader
{
    public const int MaxPackBytes = 8 * 1024 * 1024;
    public const int MaxLineLength = 64 * 1024;
    public const int MaxRuleCount = 10_000;
    public const int MaxTargetsPerRule = 512;

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    public Winapp2RuleCatalog Load(string path, Winapp2RulePackDescriptor descriptor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = File.OpenRead(path);
        return Load(stream, descriptor);
    }

    public Winapp2RuleCatalog Load(Stream stream, Winapp2RulePackDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(descriptor);
        if (!stream.CanRead)
            throw new ArgumentException("The rule-pack stream must be readable.", nameof(stream));

        Winapp2RulePackDescriptorPolicy.Validate(descriptor);
        var content = ReadBounded(stream);
        var actualHash = Convert.ToHexString(SHA256.HashData(content));
        if (!actualHash.Equals(descriptor.ExpectedSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The rule-pack SHA-256 does not match its pinned descriptor.");

        string text;
        try
        {
            text = StrictUtf8.GetString(content);
        }
        catch (DecoderFallbackException ex)
        {
            throw new InvalidDataException("The rule pack is not valid UTF-8.", ex);
        }

        if (text.Length > 0 && text[0] == '\uFEFF')
            text = text[1..];
        if (text.IndexOf('\0') >= 0)
            throw new InvalidDataException("The rule pack contains a NUL character.");

        var parser = new Parser();
        parser.Parse(text);
        if (parser.Rules.Count == 0)
            throw new InvalidDataException("The rule pack must contain at least one rule section.");
        return new Winapp2RuleCatalog
        {
            Descriptor = descriptor,
            ContentSha256 = actualHash,
            Rules = parser.Rules,
            Diagnostics = parser.Diagnostics
        };
    }

    private static byte[] ReadBounded(Stream stream)
    {
        using var content = new MemoryStream();
        var buffer = new byte[81920];
        while (true)
        {
            var read = stream.Read(buffer, 0, buffer.Length);
            if (read == 0)
                return content.ToArray();
            if (content.Length + read > MaxPackBytes)
                throw new InvalidDataException($"The rule pack exceeds the {MaxPackBytes}-byte size limit.");
            content.Write(buffer, 0, read);
        }
    }

    private sealed class Parser
    {
        private readonly List<Winapp2RuleDefinition> _rules = [];
        private readonly List<Winapp2RuleDiagnostic> _diagnostics = [];
        private readonly HashSet<string> _names = new(StringComparer.OrdinalIgnoreCase);
        private RuleBuilder? _current;

        public IReadOnlyList<Winapp2RuleDefinition> Rules => _rules;
        public IReadOnlyList<Winapp2RuleDiagnostic> Diagnostics => _diagnostics;

        public void Parse(string text)
        {
            using var reader = new StringReader(text);
            var lineNumber = 0;
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                lineNumber++;
                if (line.Length > MaxLineLength)
                    throw Error(lineNumber, $"line exceeds the {MaxLineLength}-character limit");

                var trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
                {
                    _current?.RawLines.Add(line);
                    continue;
                }

                if (trimmed.StartsWith('['))
                {
                    StartRule(trimmed, line, lineNumber);
                    continue;
                }

                if (_current is null)
                    throw Error(lineNumber, "content appears before the first rule section");

                _current.RawLines.Add(line);
                var separator = trimmed.IndexOf('=');
                if (separator <= 0)
                    throw Error(lineNumber, "line is not a key=value assignment");

                var key = trimmed[..separator].Trim();
                var value = trimmed[(separator + 1)..].Trim();
                if (key.Length == 0)
                    throw Error(lineNumber, "assignment key is empty");

                Apply(key, value, lineNumber);
            }

            FinishRule();
        }

        private void StartRule(string trimmed, string rawLine, int lineNumber)
        {
            if (!trimmed.EndsWith(']') || trimmed.Count(character => character == '[') != 1 || trimmed.Count(character => character == ']') != 1)
                throw Error(lineNumber, "section header is malformed");

            var name = trimmed[1..^1].Trim();
            if (name.Length == 0)
                throw Error(lineNumber, "section name is empty");

            FinishRule();
            if (!_names.Add(name))
                throw Error(lineNumber, $"duplicate section '{name}'");
            if (_rules.Count >= MaxRuleCount)
                throw Error(lineNumber, $"rule count exceeds the {MaxRuleCount}-rule limit");

            _current = new RuleBuilder(name, lineNumber);
            _current.RawLines.Add(rawLine);
        }

        private void Apply(string key, string value, int lineNumber)
        {
            var current = _current!;
            if (key.Equals("LangSecRef", StringComparison.OrdinalIgnoreCase))
                current.LanguageSection = Singular(current.LanguageSection, key, value, lineNumber);
            else if (key.Equals("Section", StringComparison.OrdinalIgnoreCase))
                current.Section = Singular(current.Section, key, value, lineNumber);
            else if (key.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                current.Warning = Singular(current.Warning, key, value, lineNumber);
            else if (key.Equals("Default", StringComparison.OrdinalIgnoreCase))
                current.DefaultSelected = ParseDefault(value, lineNumber);
            else if (IsKey(key, "DetectFile"))
                AddTarget(current.DetectFilePaths, value, lineNumber);
            else if (IsKey(key, "DetectOS"))
                AddTarget(current.OperatingSystemConstraints, value, lineNumber);
            else if (IsKey(key, "SpecialDetect"))
                AddTarget(current.SpecialDetections, value, lineNumber);
            else if (IsKey(key, "Detect"))
                AddTarget(current.DetectPaths, value, lineNumber);
            else if (IsKey(key, "FileKey"))
                AddTarget(current.FileTargets, value, lineNumber);
            else if (IsKey(key, "RegKey"))
                AddTarget(current.RegistryTargets, value, lineNumber);
            else if (IsKey(key, "ExcludeKey"))
                AddTarget(current.ExclusionTargets, value, lineNumber);
            else
                _diagnostics.Add(new Winapp2RuleDiagnostic
                {
                    LineNumber = lineNumber,
                    RuleName = current.Name,
                    Key = key,
                    Message = "Unknown key was preserved only in raw source."
                });
        }

        private string Singular(string? existing, string key, string value, int lineNumber)
        {
            if (existing is not null)
            {
                _diagnostics.Add(new Winapp2RuleDiagnostic
                {
                    LineNumber = lineNumber,
                    RuleName = _current!.Name,
                    Key = key,
                    Message = "Duplicate singleton key; the last value was retained."
                });
            }

            return value;
        }

        private void AddTarget(List<string> targets, string value, int lineNumber)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw Error(lineNumber, "target value is empty");
            var current = _current!;
            current.TargetCount++;
            if (current.TargetCount > MaxTargetsPerRule)
                throw Error(lineNumber, $"rule target limit of {MaxTargetsPerRule} was exceeded");
            targets.Add(value);
        }

        private bool? ParseDefault(string value, int lineNumber)
        {
            if (value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1")
                return true;
            if (value.Equals("false", StringComparison.OrdinalIgnoreCase) || value == "0")
                return false;
            throw Error(lineNumber, "Default must be True, False, 1, or 0");
        }

        private void FinishRule()
        {
            if (_current is null)
                return;

            _rules.Add(_current.Build());
            _current = null;
        }

        private static bool IsKey(string key, string prefix)
        {
            if (key.Equals(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
            if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;
            var suffix = key[prefix.Length..];
            return suffix.Length > 0 && suffix.All(char.IsAsciiDigit);
        }

        private static InvalidDataException Error(int lineNumber, string message) =>
            new($"Invalid Winapp2 rule-pack line {lineNumber}: {message}.");
    }

    private sealed class RuleBuilder(string name, int startLine)
    {
        public string Name { get; } = name;
        public int StartLine { get; } = startLine;
        public string? LanguageSection { get; set; }
        public string? Section { get; set; }
        public List<string> DetectPaths { get; } = [];
        public List<string> DetectFilePaths { get; } = [];
        public List<string> SpecialDetections { get; } = [];
        public List<string> OperatingSystemConstraints { get; } = [];
        public List<string> FileTargets { get; } = [];
        public List<string> RegistryTargets { get; } = [];
        public List<string> ExclusionTargets { get; } = [];
        public string? Warning { get; set; }
        public bool? DefaultSelected { get; set; }
        public int TargetCount { get; set; }
        public List<string> RawLines { get; } = [];

        public Winapp2RuleDefinition Build() =>
            new()
            {
                Name = Name,
                LanguageSection = LanguageSection,
                Section = Section,
                DetectPaths = DetectPaths.ToArray(),
                DetectFilePaths = DetectFilePaths.ToArray(),
                SpecialDetections = SpecialDetections.ToArray(),
                OperatingSystemConstraints = OperatingSystemConstraints.ToArray(),
                FileTargets = FileTargets.ToArray(),
                RegistryTargets = RegistryTargets.ToArray(),
                ExclusionTargets = ExclusionTargets.ToArray(),
                Warning = Warning,
                DefaultSelected = DefaultSelected,
                RawSource = string.Join(Environment.NewLine, RawLines)
            };
    }
}
