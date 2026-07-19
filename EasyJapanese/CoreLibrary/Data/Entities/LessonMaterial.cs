using System;

namespace CoreLibrary.Data.Entities;

public partial class LessonMaterial
{
    public int MaterialId { get; set; }

    public int LessonId { get; set; }

    public string Title { get; set; } = null!;

    public string Url { get; set; } = null!;

    public string? FileType { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;
}
