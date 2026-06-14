using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class SubscriptionPlan
{
    public int PlanId { get; set; }

    public string PlanName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int DurationDays { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<StudentMembership> StudentMemberships { get; set; } = new List<StudentMembership>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
}
