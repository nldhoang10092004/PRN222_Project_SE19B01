using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLibrary.Data
{
    public static class AppDbContextExtensions
    {
        public const string DefaultConnectionName = "DefaultConnection";

        public static IServiceCollection AddAppDbContext(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionName = DefaultConnectionName)
        {
            var connectionString = configuration.GetConnectionString(connectionName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{connectionName}' is not configured. " +
                    $"Add it to appsettings.json under 'ConnectionStrings'.");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }
    }
}
