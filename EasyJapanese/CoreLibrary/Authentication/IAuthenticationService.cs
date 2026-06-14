using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CoreLibrary.Authentication
{
    public class CurrentUser
    {
        public int AccountId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? FullName { get; set; }
    }

    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public enum AuthStatus
    {
        Success,
        EmailAlreadyExists,
        EmailNotVerified,
        InvalidCredentials,
        AccountLocked,
        InvalidOtp,
        OtpExpired,
        OtpSendFailed,
        NoPendingRegistration,
        WeakPassword,
        InvalidInput,
        SystemError
    }

    public class AuthResult
    {
        public AuthStatus Status { get; init; }
        public string? Message { get; init; }
        public CurrentUser? User { get; init; }

        public bool IsSuccess => Status == AuthStatus.Success;

        public static AuthResult Ok(CurrentUser? user = null, string? message = null)
            => new() { Status = AuthStatus.Success, User = user, Message = message };

        public static AuthResult Fail(AuthStatus status, string message)
            => new() { Status = status, Message = message };
    }

    public class PendingRegistration
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string OtpHash { get; set; } = string.Empty;
        public System.DateTime OtpExpiry { get; set; }
    }

    public interface IAuthenticationService
    {
        public const string SessionKeyCurrentUser = "_CurrentUser";
        public const string SessionKeyPendingRegistration = "_PendingRegistration";

        Task<AuthResult> StartRegistrationAsync(RegisterRequest request, HttpContext httpContext, CancellationToken cancellationToken = default);

        Task<AuthResult> VerifyRegistrationAsync(string email, string otp, HttpContext httpContext, CancellationToken cancellationToken = default);

        Task<AuthResult> LoginAsync(string email, string password, HttpContext httpContext, CancellationToken cancellationToken = default);

        Task LogoutAsync(HttpContext httpContext);

        Task<CurrentUser?> GetCurrentUserAsync(HttpContext httpContext);
    }
}
