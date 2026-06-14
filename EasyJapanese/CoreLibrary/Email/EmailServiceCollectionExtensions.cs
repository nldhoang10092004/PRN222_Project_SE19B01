using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLibrary.Email
{
    public static class EmailServiceCollectionExtensions
    {
        public static IServiceCollection AddGmailEmail(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
            services.AddSingleton<IEmailService, EmailService>();
            return services;
        }
    }
}
