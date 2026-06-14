using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Lesson
{
    public int LessonId { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string LessonType { get; set; } = null!;

    public string? VideoUrl { get; set; }

    public string? Content { get; set; }

    public int? Duration { get; set; }

    public int SortOrder { get; set; }

    public bool IsPreview { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
}
