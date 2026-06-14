using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CoreLibrary.Const;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Email;
using CoreLibrary.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreLibrary.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;
        private readonly AuthenticationOptions _options;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            AppDbContext db,
            IEmailService email,
            IOptions<AuthenticationOptions> options,
            ILogger<AuthenticationService> logger)
        {
            _db = db;
            _email = email;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<AuthResult> StartRegistrationAsync(
            RegisterRequest request,
            HttpContext httpContext,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return AuthResult.Fail(AuthStatus.InvalidInput, MessageConst.INVALID_INPUT);
            }

            if (request.Password.Length < 8 ||
                !request.Password.Any(char.IsDigit) ||
                !request.Password.Any(char.IsLetter))
            {
                return AuthResult.Fail(AuthStatus.WeakPassword, MessageConst.PASSWORD_TOO_WEAK);
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var existing = await _db.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Email == normalizedEmail, cancellationToken);
            if (existing != null)
            {
                return AuthResult.Fail(AuthStatus.EmailAlreadyExists, MessageConst.EMAIL_ALREADY_EXISTS);
            }

            var otp = GenerateOtp(_options.OtpLength);
            var pending = new PendingRegistration
            {
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                PhoneNumber = request.PhoneNumber.Trim(),
                PasswordHash = PasswordUtil.HashPassword(request.Password),
                OtpHash = PasswordUtil.HashPassword(otp),
                OtpExpiry = DateTime.UtcNow.AddMinutes(_options.OtpExpiryMinutes)
            };

            try
            {
                await _email.SendOtpAsync(normalizedEmail, otp, "Xác nhận đăng ký tài khoản Hi Japan!", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP to {Email}", normalizedEmail);
                return AuthResult.Fail(AuthStatus.OtpSendFailed, MessageConst.OTP_SEND_FAILED);
            }

            httpContext.Session.SetObject(IAuthenticationService.SessionKeyPendingRegistration, pending);
            return AuthResult.Ok(message: "Mã OTP đã được gửi tới email của bạn.");
        }

        public async Task<AuthResult> VerifyRegistrationAsync(
            string email,
            string otp,
            HttpContext httpContext,
            CancellationToken cancellationToken = default)
        {
            var pending = httpContext.Session.GetObject<PendingRegistration>(IAuthenticationService.SessionKeyPendingRegistration);
            if (pending == null || !string.Equals(pending.Email, email?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return AuthResult.Fail(AuthStatus.NoPendingRegistration, MessageConst.NO_PENDING_REGISTRATION);
            }

            if (pending.OtpExpiry < DateTime.UtcNow)
            {
                httpContext.Session.Remove(IAuthenticationService.SessionKeyPendingRegistration);
                return AuthResult.Fail(AuthStatus.OtpExpired, MessageConst.OTP_EXPIRED);
            }

            if (!PasswordUtil.VerifyPassword(otp, pending.OtpHash))
            {
                return AuthResult.Fail(AuthStatus.InvalidOtp, MessageConst.INVALID_OTP);
            }

            var now = DateTime.UtcNow;
            var account = new Account
            {
                Email = pending.Email,
                PasswordHash = pending.PasswordHash,
                Role = RoleConst.STUDENT,
                IsEmailVerified = true,
                IsLocked = false,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.Accounts.Add(account);
            await _db.SaveChangesAsync(cancellationToken);

            var student = new Student
            {
                StudentId = account.AccountId,
                FullName = pending.FullName,
                PhoneNumber = pending.PhoneNumber,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.Students.Add(student);
            await _db.SaveChangesAsync(cancellationToken);

            httpContext.Session.Remove(IAuthenticationService.SessionKeyPendingRegistration);

            return AuthResult.Ok(
                user: new CurrentUser
                {
                    AccountId = account.AccountId,
                    Email = account.Email,
                    Role = account.Role,
                    FullName = pending.FullName
                },
                message: MessageConst.REGISTRATION_SUCCESS);
        }

        public async Task<AuthResult> LoginAsync(
            string email,
            string password,
            HttpContext httpContext,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return AuthResult.Fail(AuthStatus.InvalidCredentials, MessageConst.LOGIN_FAILED);
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var account = await _db.Accounts
                .Include(a => a.Student)
                .Include(a => a.Mentor)
                .Include(a => a.Admin)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Email == normalizedEmail, cancellationToken);

            if (account == null)
            {
                return AuthResult.Fail(AuthStatus.InvalidCredentials, MessageConst.LOGIN_FAILED);
            }

            if (account.IsLocked)
            {
                return AuthResult.Fail(AuthStatus.AccountLocked, MessageConst.ACCOUNT_LOCKED);
            }

            if (!account.IsEmailVerified)
            {
                return AuthResult.Fail(AuthStatus.EmailNotVerified, MessageConst.EMAIL_NOT_VERIFIED);
            }

            if (string.IsNullOrEmpty(account.PasswordHash) ||
                !PasswordUtil.VerifyPassword(password, account.PasswordHash))
            {
                return AuthResult.Fail(AuthStatus.InvalidCredentials, MessageConst.LOGIN_FAILED);
            }

            var fullName = account.Student?.FullName
                ?? account.Mentor?.FullName
                ?? account.Admin?.FullName;

            var currentUser = new CurrentUser
            {
                AccountId = account.AccountId,
                Email = account.Email,
                Role = account.Role,
                FullName = fullName
            };
            httpContext.Session.SetObject(IAuthenticationService.SessionKeyCurrentUser, currentUser);

            return AuthResult.Ok(user: currentUser);
        }

        public Task LogoutAsync(HttpContext httpContext)
        {
            httpContext.Session.Remove(IAuthenticationService.SessionKeyCurrentUser);
            return Task.CompletedTask;
        }

        public Task<CurrentUser?> GetCurrentUserAsync(HttpContext httpContext)
        {
            return Task.FromResult(httpContext.Session.GetObject<CurrentUser>(IAuthenticationService.SessionKeyCurrentUser));
        }

        private static string GenerateOtp(int length)
        {
            var buffer = new byte[length];
            RandomNumberGenerator.Fill(buffer);
            // Bỏ qua byte 0..9 để tránh chia modulo bias; chuyển sang digit character
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = (char)('0' + (buffer[i] % 10));
            }
            return new string(chars);
        }
    }
}
