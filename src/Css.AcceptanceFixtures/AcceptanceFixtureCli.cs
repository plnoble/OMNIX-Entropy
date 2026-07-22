using System.Text.Json;

namespace Css.AcceptanceFixtures;

public static class AcceptanceFixtureCli
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var command = AcceptanceFixtureCommand.Parse(args);
            var layout = AcceptanceFixtureLayout.CreateForWindows(command.SessionId);
            var files = new SystemAcceptanceFixtureFileSystem();
            var registry = new CurrentUserAcceptanceFixtureRegistry();
            var fixtureOperator = new AcceptanceFixtureOperator(files, registry);
            object result = command.Kind switch
            {
                AcceptanceFixtureCommandKind.Provision => fixtureOperator.Provision(
                    layout,
                    AcceptanceFixturePayload.Discover(AppContext.BaseDirectory),
                    command.Attestation!),
                AcceptanceFixtureCommandKind.Uninstall => fixtureOperator.Uninstall(
                    layout,
                    command.Role!.Value,
                    command.Attestation!),
                AcceptanceFixtureCommandKind.Reset => fixtureOperator.Reset(
                    layout,
                    command.Attestation!),
                AcceptanceFixtureCommandKind.Status => fixtureOperator.GetStatus(layout),
                AcceptanceFixtureCommandKind.Lock => await HoldFailureLockAsync(
                    layout,
                    command.Attestation!,
                    command.Duration!.Value),
                _ => throw new InvalidOperationException("Fixture command is unsupported.")
            };
            Console.WriteLine(JsonSerializer.Serialize(
                new { Success = true, Result = result },
                JsonOptions));
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(JsonSerializer.Serialize(
                new
                {
                    Success = false,
                    ErrorType = exception.GetType().Name,
                    Error = exception.Message
                },
                JsonOptions));
            return 1;
        }
    }

    private static async Task<object> HoldFailureLockAsync(
        AcceptanceFixtureLayout layout,
        string attestation,
        TimeSpan duration)
    {
        var path = AcceptanceFixtureAuthority.ValidateFailureLockTarget(
            layout,
            layout.FailureLockFile,
            attestation);
        using var lockStream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None);
        await Task.Delay(duration);
        return new
        {
            layout.SessionId,
            Locked = true,
            DurationSeconds = (int)duration.TotalSeconds
        };
    }
}
