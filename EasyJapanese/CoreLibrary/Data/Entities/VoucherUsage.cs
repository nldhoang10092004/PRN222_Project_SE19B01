using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class VoucherUsage
{
    public int UsageId { get; set; }

    public int VoucherId { get; set; }

    public int StudentId { get; set; }

    public int? TransactionId { get; set; }

    public decimal DiscountApplied { get; set; }

    public DateTime AppliedAt { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Transaction? Transaction { get; set; }

    public virtual Voucher Voucher { get; set; } = null!;
}
