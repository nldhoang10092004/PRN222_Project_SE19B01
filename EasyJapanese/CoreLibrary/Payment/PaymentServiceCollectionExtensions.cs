using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PayOS;

namespace CoreLibrary.Payment
{
    public static class PaymentServiceCollectionExtensions
    {
        /// <summary>
        /// Register <see cref="IPaymentService"/> and the underlying
        /// <see cref="PayOSClient"/> as singletons, binding credentials from the
        /// "PayOS" section of <see cref="IConfiguration"/>.
        /// </summary>
        public static IServiceCollection AddPayOS(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<PayOSOptions>(configuration.GetSection(PayOSOptions.SectionName));

            services.AddSingleton(sp =>
            {
                var ours = sp.GetRequiredService<IOptions<PayOSOptions>>().Value;
                var sdk = new global::PayOS.PayOSOptions
                {
                    ClientId = ours.ClientId,
                    ApiKey = ours.ApiKey,
                    ChecksumKey = ours.ChecksumKey
                };
                if (!string.IsNullOrWhiteSpace(ours.PartnerCode)) sdk.PartnerCode = ours.PartnerCode;
                if (!string.IsNullOrWhiteSpace(ours.BaseUrl)) sdk.BaseUrl = ours.BaseUrl;
                return new PayOSClient(sdk);
            });

            services.AddSingleton<IPaymentService, PaymentService>();
            return services;
        }
    }
}
