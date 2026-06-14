using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class LessonProgress
{
    public int ProgressId { get; set; }

    public int StudentId { get; set; }

    public int LessonId { get; set; }

    public int WatchedSeconds { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime LastAccessedAt { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
