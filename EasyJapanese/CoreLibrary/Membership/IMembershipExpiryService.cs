namespace CoreLibrary.Membership
{
    public interface IMembershipExpiryService
    {
        /// <summary>Set IsActive = false cho các StudentMembership đã qua EndDate.</summary>
        /// <returns>Số membership vừa bị hết hạn.</returns>
        Task<int> ExpireOverdueMembershipsAsync(CancellationToken cancellationToken = default);

        /// <summary>Gửi email nhắc gia hạn cho membership sắp hết hạn trong RemindBeforeDays ngày.</summary>
        /// <returns>Số email đã gửi.</returns>
        Task<int> SendExpiryRemindersAsync(CancellationToken cancellationToken = default);
    }
}
