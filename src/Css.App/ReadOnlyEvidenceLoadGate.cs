namespace Css.App;

public sealed class ReadOnlyEvidenceLoadGate
{
    private readonly object _sync = new();
    private Task? _inFlight;
    private bool _hasCompletedLoad;

    public bool HasCompletedLoad
    {
        get
        {
            lock (_sync)
                return _hasCompletedLoad;
        }
    }

    public Task EnsureLoadedAsync(Func<Task<bool>> loader) =>
        StartAsync(loader, forceRefresh: false);

    public Task RefreshAsync(Func<Task<bool>> loader) =>
        StartAsync(loader, forceRefresh: true);

    public void MarkLoaded()
    {
        lock (_sync)
            _hasCompletedLoad = true;
    }

    private Task StartAsync(Func<Task<bool>> loader, bool forceRefresh)
    {
        ArgumentNullException.ThrowIfNull(loader);
        TaskCompletionSource<bool> completion;
        lock (_sync)
        {
            if (!forceRefresh && _hasCompletedLoad)
                return Task.CompletedTask;
            if (_inFlight is not null)
                return _inFlight;

            completion = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _inFlight = completion.Task;
        }

        _ = CompleteLoadAsync(loader, completion);
        return completion.Task;
    }

    private async Task CompleteLoadAsync(
        Func<Task<bool>> loader,
        TaskCompletionSource<bool> completion)
    {
        try
        {
            var completed = await loader();
            lock (_sync)
            {
                if (completed)
                    _hasCompletedLoad = true;
            }
            completion.TrySetResult(completed);
        }
        catch (Exception exception)
        {
            completion.TrySetException(exception);
        }
        finally
        {
            lock (_sync)
            {
                if (ReferenceEquals(_inFlight, completion.Task))
                    _inFlight = null;
            }
        }
    }
}
