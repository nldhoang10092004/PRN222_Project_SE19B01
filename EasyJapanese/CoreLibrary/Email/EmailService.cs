using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace CoreLibrary.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendOtpAsync(string toEmail, string otp, string purpose, CancellationToken cancellationToken = default)
        {
            var subject = $"[{purpose}] Mã xác nhận của bạn";
            var body = BuildOtpHtml(otp, purpose);
            await SendAsync(toEmail, subject, body, cancellationToken);
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_options.Host, _options.Port, _options.UseSsl, cancellationToken);
            await client.AuthenticateAsync(_options.User, _options.AppPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);

            _logger.LogInformation("Email sent to {Email} subject={Subject}", toEmail, subject);
        }

        private static string BuildOtpHtml(string otp, string purpose)
        {
            return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Inter, Arial, sans-serif; background:#FAFAFA; padding:24px;'>
  <div style='max-width:480px; margin:0 auto; background:#FFFFFF; border-radius:12px; padding:32px;'>
    <h2 style='color:#DC2626; margin:0 0 16px 0;'>Hi Japan!</h2>
    <p style='color:#18181B; font-size:16px;'>Mã xác nhận cho <strong>{purpose}</strong> của bạn là:</p>
    <div style='font-size:32px; font-weight:800; letter-spacing:8px; color:#18181B; background:#FEE2E2; padding:16px 24px; border-radius:8px; text-align:center; margin:24px 0;'>{otp}</div>
    <p style='color:#52525B; font-size:14px;'>Mã có hiệu lực trong <strong>10 phút</strong>. Nếu bạn không yêu cầu mã này, hãy bỏ qua email.</p>
    <hr style='border:none; border-top:1px solid #E4E4E7; margin:24px 0;'>
    <p style='color:#71717A; font-size:12px;'>© Hi Japan! — Nền tảng học tiếng Nhật</p>
  </div>
</body>
</html>";
        }
    }
}
