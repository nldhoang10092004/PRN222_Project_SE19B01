using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class StudentMembership
{
    public int MembershipId { get; set; }

    public int StudentId { get; set; }

    public int PlanId { get; set; }

    public int TransactionId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
