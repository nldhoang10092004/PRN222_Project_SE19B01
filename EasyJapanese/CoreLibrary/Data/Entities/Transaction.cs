using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int StudentId { get; set; }

    public int PlanId { get; set; }

    public int? VoucherId { get; set; }

    public decimal OriginalAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string? PaymentRef { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;

    public virtual ICollection<StudentMembership> StudentMemberships { get; set; } = new List<StudentMembership>();

    public virtual Voucher? Voucher { get; set; }

    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
}
