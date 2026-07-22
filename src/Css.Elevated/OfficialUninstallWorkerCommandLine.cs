using System.Diagnostics;
using System.Globalization;
using System.Security.Principal;
using Css.Ipc.Uninstall;

internal sealed record OfficialUninstallWorkerCommandLineOptions
{
    public required string PipeName { get; init; }
    public required string SessionId { get; init; }
    public required OfficialUninstallPipePeerIdentity Client { get; init; }
    public required int TimeoutMilliseconds { get; init; }
    public int ResponseDelayMilliseconds { get; init; }
    public int ExitDelayMilliseconds { get; init; }
}

internal static class OfficialUninstallWorkerCommandLine
{
    private const int MinimumTimeoutMilliseconds = 100;
    private const int MaximumTimeoutMilliseconds = 300_000;
    private const int MaximumFakeDelayMilliseconds = 5_000;

    private static readonly string[] RequiredNames =
    [
        "--pipe-name", "--session-id", "--client-sid", "--client-pid",
        "--client-windows-session", "--timeout-ms"
    ];

    private static readonly string[] FakeOnlyNames =
    [
        "--fake-response-delay-ms", "--fake-exit-delay-ms"
    ];

    internal static OfficialUninstallWorkerCommandLineOptions Parse(
        string[] args,
        bool allowFakeOptions)
    {
        ArgumentNullException.ThrowIfNull(args);
        var allowed = allowFakeOptions
            ? RequiredNames.Concat(FakeOnlyNames).ToArray()
            : RequiredNames;
        if (args.Length < RequiredNames.Length * 2
            || args.Length > allowed.Length * 2
            || args.Length % 2 != 0)
        {
            throw new ArgumentException("The worker metadata is incomplete.", nameof(args));
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < args.Length; index += 2)
        {
            var name = args[index];
            if (!allowed.Contains(name, StringComparer.Ordinal)
                || !values.TryAdd(name, args[index + 1]))
            {
                throw new ArgumentException("The worker metadata is invalid.", nameof(args));
            }
        }
        if (RequiredNames.Any(name => !values.ContainsKey(name)))
            throw new ArgumentException("The worker metadata is incomplete.", nameof(args));

        var pipeName = Token(values["--pipe-name"], "pipe name");
        if (pipeName.IndexOfAny(['\\', '/']) >= 0)
            throw new ArgumentException("The pipe name is invalid.", nameof(args));
        var sessionId = Token(values["--session-id"], "session id");
        var sid = values["--client-sid"];
        _ = new SecurityIdentifier(sid);
        var clientPid = PositiveInt(values["--client-pid"], "client pid");
        var clientSession = NonNegativeInt(
            values["--client-windows-session"],
            "client Windows session");
        var timeout = PositiveInt(values["--timeout-ms"], "timeout");
        if (timeout is < MinimumTimeoutMilliseconds or > MaximumTimeoutMilliseconds)
            throw new ArgumentOutOfRangeException(nameof(args), "The worker timeout is invalid.");

        return new OfficialUninstallWorkerCommandLineOptions
        {
            PipeName = pipeName,
            SessionId = sessionId,
            Client = new OfficialUninstallPipePeerIdentity
            {
                UserSid = sid,
                ProcessId = clientPid,
                WindowsSessionId = clientSession
            },
            TimeoutMilliseconds = timeout,
            ResponseDelayMilliseconds = allowFakeOptions
                ? OptionalDelay(values, "--fake-response-delay-ms")
                : 0,
            ExitDelayMilliseconds = allowFakeOptions
                ? OptionalDelay(values, "--fake-exit-delay-ms")
                : 0
        };
    }

    internal static OfficialUninstallPipePeerIdentity CurrentProcessIdentity()
    {
        using var identity = WindowsIdentity.GetCurrent();
        using var process = Process.GetCurrentProcess();
        return new OfficialUninstallPipePeerIdentity
        {
            UserSid = identity.User?.Value
                ?? throw new InvalidOperationException("The worker SID is unavailable."),
            ProcessId = Environment.ProcessId,
            WindowsSessionId = process.SessionId
        };
    }

    private static int OptionalDelay(
        IReadOnlyDictionary<string, string> values,
        string name)
    {
        if (!values.TryGetValue(name, out var value))
            return 0;
        var delay = NonNegativeInt(value, name);
        if (delay > MaximumFakeDelayMilliseconds)
            throw new ArgumentOutOfRangeException(nameof(values), "The fake delay is invalid.");
        return delay;
    }

    private static string Token(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
            throw new ArgumentException($"The {name} is invalid.");
        return value;
    }

    private static int PositiveInt(string value, string name)
    {
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
            || parsed <= 0)
        {
            throw new ArgumentException($"The {name} is invalid.");
        }
        return parsed;
    }

    private static int NonNegativeInt(string value, string name)
    {
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
            || parsed < 0)
        {
            throw new ArgumentException($"The {name} is invalid.");
        }
        return parsed;
    }
}
