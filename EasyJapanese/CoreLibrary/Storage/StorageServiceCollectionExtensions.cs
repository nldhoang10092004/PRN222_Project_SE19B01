using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLibrary.Storage
{
    public static class StorageServiceCollectionExtensions
    {
        /// <summary>
        /// Register <see cref="IStorageService"/> bound to a Cloudflare R2
        /// bucket. Reads the "R2" section of <see cref="IConfiguration"/>.
        /// </summary>
        public static IServiceCollection AddCloudflareR2(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<R2Options>(configuration.GetSection(R2Options.SectionName));

            services.AddSingleton<IAmazonS3>(sp =>
            {
                var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<R2Options>>().Value;
                var creds = new BasicAWSCredentials(opts.AccessKey, opts.SecretKey);

                // R2 endpoint format: https://<accountid>.r2.cloudflarestorage.com
                var config = new AmazonS3Config
                {
                    ServiceURL = $"https://{opts.AccountId}.r2.cloudflarestorage.com",
                    AuthenticationRegion = "auto",           // R2 bắt buộc region = "auto"
                    ForcePathStyle = true,                   // R2 dùng path-style, không virtual-host
                    UseHttp = false
                };

                return new AmazonS3Client(creds, config);
            });

            services.AddSingleton<IStorageService, R2StorageService>();
            return services;
        }
    }
}
