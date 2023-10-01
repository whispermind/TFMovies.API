using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Services.Implementations;

public class EmailBackgroundService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailBackgroundService(IBackgroundTaskQueue taskQueue, IServiceScopeFactory scopeFactory)
    {
        _taskQueue = taskQueue;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _taskQueue.DequeueAsync(stoppingToken);

            using (var scope = _scopeFactory.CreateScope())
            {
                await workItem(stoppingToken);
            }
        }
    }
}

