namespace CoreLibrary.Membership
{
    public class MembershipExpiryOptions
    {
        public const string SectionName = "MembershipExpiry";

        /// <summary>Số ngày trước khi hết hạn thì gửi email nhắc gia hạn.</summary>
        public int RemindBeforeDays { get; set; } = 3;
    }
}
