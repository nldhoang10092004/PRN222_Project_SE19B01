namespace CoreLibrary.Storage
{
    /// <summary>
    /// Strongly-typed configuration for Cloudflare R2 access.
    /// Bound from the "R2" section of appsettings.json.
    /// </summary>
    public class R2Options
    {
        public const string SectionName = "R2";

        /// <summary>Cloudflare account ID (visible in the R2 dashboard URL).</summary>
        public string AccountId { get; set; } = string.Empty;

        /// <summary>R2 API token access key.</summary>
        public string AccessKey { get; set; } = string.Empty;

        /// <summary>R2 API token secret key.</summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>Bucket name (e.g. "easyjapanese-assets").</summary>
        public string BucketName { get; set; } = "easyjapanese-assets";

        /// <summary>
        /// Public base URL for serving assets. Either the free
        /// pub-xxx.r2.dev subdomain or a custom domain like
        /// "https://cdn.easyjapanese.app".
        /// Must NOT have a trailing slash.
        /// </summary>
        public string PublicBaseUrl { get; set; } = string.Empty;
    }
}
