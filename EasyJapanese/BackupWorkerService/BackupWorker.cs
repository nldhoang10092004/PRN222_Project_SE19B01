using CoreLibrary.Backup;

namespace BackupWorkerService;

public class BackupWorker(IBackupService backupService, ILogger<BackupWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await backupService.BackupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Backup database thất bại");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
