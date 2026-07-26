using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLibrary.Reconciliation
{
    public static class ReconciliationServiceCollectionExtensions
    {
        public static IServiceCollection AddTransactionReconciliation(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<ReconciliationOptions>(configuration.GetSection(ReconciliationOptions.SectionName));
            services.AddScoped<ITransactionReconciliationService, TransactionReconciliationService>();
            return services;
        }
    }
}
