using CoreLibrary.Reconciliation;

namespace BackupWorkerService;

public class ReconciliationWorker(IServiceScopeFactory scopeFactory, ILogger<ReconciliationWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ITransactionReconciliationService>();

                await service.ReconcilePendingTransactionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Đồng bộ transaction thất bại");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
