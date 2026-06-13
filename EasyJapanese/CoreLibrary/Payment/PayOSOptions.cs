namespace CoreLibrary.Payment
{
    /// <summary>
    /// Strongly-typed configuration for the PayOS client. Bound from the
    /// "PayOS" section of appsettings.json.
    /// </summary>
    public class PayOSOptions
    {
        public const string SectionName = "PayOS";

        public string ClientId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ChecksumKey { get; set; } = string.Empty;

        /// <summary>
        /// Optional partner integration code sent in the x-partner-code header.
        /// </summary>
        public string? PartnerCode { get; set; }

        /// <summary>
        /// Optional override for the PayOS API base URL. Leave null to use the SDK default.
        /// </summary>
        public string? BaseUrl { get; set; }
    }
}
