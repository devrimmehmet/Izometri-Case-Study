namespace NotificationService.Infrastructure.Persistence;

public sealed class DatabaseMigrationState
{
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Ready => _ready.Task;

    public void MarkReady() => _ready.TrySetResult();

    public void MarkFailed(Exception exception) => _ready.TrySetException(exception);
}
