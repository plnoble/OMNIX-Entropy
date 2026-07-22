namespace Css.AcceptanceFixtures;

public enum AcceptanceFixtureCommandKind
{
    Provision,
    Uninstall,
    Lock,
    Reset,
    Status
}

public sealed record AcceptanceFixtureCommand(
    AcceptanceFixtureCommandKind Kind,
    string SessionId,
    string? Attestation,
    AcceptanceFixtureRole? Role,
    TimeSpan? Duration)
{
    public bool IsMutating => Kind != AcceptanceFixtureCommandKind.Status;

    public static AcceptanceFixtureCommand Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length < 1)
            throw new ArgumentException("A fixture command is required.", nameof(args));
        var kind = args[0].ToLowerInvariant() switch
        {
            "provision" => AcceptanceFixtureCommandKind.Provision,
            "uninstall" => AcceptanceFixtureCommandKind.Uninstall,
            "lock" => AcceptanceFixtureCommandKind.Lock,
            "reset" => AcceptanceFixtureCommandKind.Reset,
            "status" => AcceptanceFixtureCommandKind.Status,
            _ => throw new ArgumentException("Fixture command is not supported.", nameof(args))
        };

        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 1; index < args.Length; index += 2)
        {
            if (index + 1 >= args.Length ||
                !args[index].StartsWith("--", StringComparison.Ordinal) ||
                args[index].Length <= 2 ||
                !options.TryAdd(args[index][2..], args[index + 1]))
            {
                throw new ArgumentException("Fixture options are malformed or duplicated.", nameof(args));
            }
        }

        var allowed = kind switch
        {
            AcceptanceFixtureCommandKind.Uninstall =>
                new[] { "session-id", "role", "attestation" },
            AcceptanceFixtureCommandKind.Lock =>
                new[] { "session-id", "duration-seconds", "attestation" },
            AcceptanceFixtureCommandKind.Status =>
                new[] { "session-id" },
            _ => new[] { "session-id", "attestation" }
        };
        if (options.Keys.Any(key => !allowed.Contains(key, StringComparer.OrdinalIgnoreCase)))
            throw new ArgumentException("Fixture command contains an unknown option.", nameof(args));

        if (!options.TryGetValue("session-id", out var sessionId) ||
            !Guid.TryParseExact(sessionId, "D", out var parsed) ||
            !string.Equals(parsed.ToString("D"), sessionId, StringComparison.Ordinal))
        {
            throw new ArgumentException("A canonical lowercase session id is required.", nameof(args));
        }

        string? attestation = null;
        if (kind != AcceptanceFixtureCommandKind.Status)
        {
            if (!options.TryGetValue("attestation", out attestation) ||
                !string.Equals(
                    attestation,
                    AcceptanceFixtureAuthority.RequiredAttestation,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "The exact disposable environment attestation is required.",
                    nameof(args));
            }
        }

        AcceptanceFixtureRole? role = null;
        if (kind == AcceptanceFixtureCommandKind.Uninstall)
        {
            if (!options.TryGetValue("role", out var roleText) ||
                !Enum.TryParse<AcceptanceFixtureRole>(
                    roleText,
                    ignoreCase: true,
                    out var parsedRole))
            {
                throw new ArgumentException("A supported fixture role is required.", nameof(args));
            }
            role = parsedRole;
        }

        TimeSpan? duration = null;
        if (kind == AcceptanceFixtureCommandKind.Lock)
        {
            if (!options.TryGetValue("duration-seconds", out var secondsText) ||
                !int.TryParse(secondsText, out var seconds) ||
                seconds is < 30 or > 600)
            {
                throw new ArgumentException(
                    "Lock duration must be from 30 to 600 seconds.",
                    nameof(args));
            }
            duration = TimeSpan.FromSeconds(seconds);
        }

        return new AcceptanceFixtureCommand(kind, sessionId, attestation, role, duration);
    }
}
