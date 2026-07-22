namespace Css.App;

public sealed class SoftwareInventoryLoadGate
{
    private readonly ReadOnlyEvidenceLoadGate _inner = new();

    public bool HasCompletedLoad => _inner.HasCompletedLoad;

    public Task EnsureLoadedAsync(Func<Task<bool>> loader) =>
        _inner.EnsureLoadedAsync(loader);

    public Task RefreshAsync(Func<Task<bool>> loader) =>
        _inner.RefreshAsync(loader);

    public void MarkLoaded() => _inner.MarkLoaded();
}
