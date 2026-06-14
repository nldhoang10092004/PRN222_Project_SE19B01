using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public int? AssignedBy { get; set; }

    public DateTime EnrolledAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Admin? AssignedByNavigation { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
