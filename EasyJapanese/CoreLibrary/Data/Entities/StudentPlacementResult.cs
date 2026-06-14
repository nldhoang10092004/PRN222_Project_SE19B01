using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class StudentPlacementResult
{
    public int ResultId { get; set; }

    public int StudentId { get; set; }

    public int TestId { get; set; }

    public int Score { get; set; }

    public int TotalPoints { get; set; }

    public int? RecommendedLevelId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual JlptLevel? RecommendedLevel { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual PlacementTest Test { get; set; } = null!;
}
