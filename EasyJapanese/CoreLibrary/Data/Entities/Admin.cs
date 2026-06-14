using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Admin
{
    public int AdminId { get; set; }

    public string FullName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account AdminNavigation { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<PlacementTest> PlacementTests { get; set; } = new List<PlacementTest>();

    public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
}
