using CoreLibrary.Data;
using CoreLibrary.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreLibrary.Membership
{
    public class MembershipExpiryService : IMembershipExpiryService
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;
        private readonly MembershipExpiryOptions _options;
        private readonly ILogger<MembershipExpiryService> _logger;

        public MembershipExpiryService(
            AppDbContext db,
            IEmailService email,
            IOptions<MembershipExpiryOptions> options,
            ILogger<MembershipExpiryService> logger)
        {
            _db = db;
            _email = email;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<int> ExpireOverdueMembershipsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var overdue = await _db.StudentMemberships
                .Where(m => m.IsActive && m.EndDate < now)
                .ToListAsync(cancellationToken);

            foreach (var membership in overdue)
            {
                membership.IsActive = false;
            }

            if (overdue.Count > 0)
            {
                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Đã hết hạn {Count} membership", overdue.Count);
            }

            return overdue.Count;
        }

        public async Task<int> SendExpiryRemindersAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var deadline = now.AddDays(_options.RemindBeforeDays);

            var expiringSoon = await _db.StudentMemberships
                .Include(m => m.Plan)
                .Include(m => m.Student).ThenInclude(s => s.StudentNavigation)
                .Where(m => m.IsActive && m.EndDate >= now && m.EndDate <= deadline)
                .ToListAsync(cancellationToken);

            var sentCount = 0;
            foreach (var membership in expiringSoon)
            {
                var email = membership.Student.StudentNavigation.Email;
                var daysLeft = (membership.EndDate - now).Days;
                var subject = "Gói thành viên của bạn sắp hết hạn";
                var body = $@"
<p>Chào {membership.Student.FullName},</p>
<p>Gói <strong>{membership.Plan.PlanName}</strong> của bạn sẽ hết hạn sau <strong>{daysLeft} ngày</strong> (vào {membership.EndDate:dd/MM/yyyy}).</p>
<p>Gia hạn ngay để tiếp tục học không bị gián đoạn.</p>";

                try
                {
                    await _email.SendAsync(email, subject, body, cancellationToken);
                    sentCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gửi email nhắc gia hạn thất bại cho StudentId={StudentId}", membership.StudentId);
                }
            }

            if (sentCount > 0)
                _logger.LogInformation("Đã gửi {Count} email nhắc gia hạn membership", sentCount);

            return sentCount;
        }
    }
}
