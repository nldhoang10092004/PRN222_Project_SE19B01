using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Exercise
{
    public int ExerciseId { get; set; }

    public int CourseId { get; set; }

    public string ExerciseType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string? AudioUrl { get; set; }

    public string? StrokeOrderUrl { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
