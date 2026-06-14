using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class PlacementTest
{
    public int TestId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int Duration { get; set; }

    public int PassScore { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Admin CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<StudentPlacementResult> StudentPlacementResults { get; set; } = new List<StudentPlacementResult>();
}
