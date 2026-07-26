using CoreLibrary.Membership;

namespace BackupWorkerService;

public class MembershipExpiryWorker(IServiceScopeFactory scopeFactory, ILogger<MembershipExpiryWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IMembershipExpiryService>();

                await service.ExpireOverdueMembershipsAsync(stoppingToken);
                await service.SendExpiryRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Xử lý membership expiry thất bại");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
