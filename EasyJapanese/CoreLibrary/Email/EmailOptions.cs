namespace CoreLibrary.Email
{
    public class EmailOptions
    {
        public const string SectionName = "Email";

        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string User { get; set; } = string.Empty;
        public string AppPassword { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "Hi Japan!";
        public bool UseSsl { get; set; } = true;
    }
}
