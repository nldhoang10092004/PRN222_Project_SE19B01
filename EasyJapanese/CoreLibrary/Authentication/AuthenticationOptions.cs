namespace CoreLibrary.Authentication
{
    public class AuthenticationOptions
    {
        public const string SectionName = "Authentication";

        public int OtpLength { get; set; } = 6;
        public int OtpExpiryMinutes { get; set; } = 10;
        public int MaxFailedAttempts { get; set; } = 5;
    }
}
