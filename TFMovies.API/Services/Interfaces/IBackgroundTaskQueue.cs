using TFMovies.API.Models.Requests;

namespace TFMovies.API.Services.Interfaces;

public interface IBackgroundTaskQueue
{
    public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);
    public Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}
