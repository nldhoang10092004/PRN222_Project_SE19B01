using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLibrary.Membership
{
    public static class MembershipServiceCollectionExtensions
    {
        public static IServiceCollection AddMembershipExpiry(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<MembershipExpiryOptions>(configuration.GetSection(MembershipExpiryOptions.SectionName));
            services.AddScoped<IMembershipExpiryService, MembershipExpiryService>();
            return services;
        }
    }
}
