using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Voucher
{
    public int VoucherId { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal? MaxDiscountCap { get; set; }

    public decimal MinOrderValue { get; set; }

    public int? ApplicablePlanId { get; set; }

    public int? MaxUsesTotal { get; set; }

    public int MaxUsesPerUser { get; set; }

    public int UsedCount { get; set; }

    public DateTime? StartsAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual SubscriptionPlan? ApplicablePlan { get; set; }

    public virtual Admin CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
}
