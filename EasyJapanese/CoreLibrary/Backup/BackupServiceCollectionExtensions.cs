using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLibrary.Backup
{
    public static class BackupServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseBackup(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<BackupOptions>(configuration.GetSection(BackupOptions.SectionName));
            services.AddSingleton<IBackupService, BackupService>();
            return services;
        }
    }
}
