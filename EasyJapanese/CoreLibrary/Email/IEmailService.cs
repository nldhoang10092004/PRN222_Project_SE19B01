using System.Threading;
using System.Threading.Tasks;

namespace CoreLibrary.Email
{
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string otp, string purpose, CancellationToken cancellationToken = default);

        Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}
