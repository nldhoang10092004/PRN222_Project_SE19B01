using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class JlptLevel
{
    public int LevelId { get; set; }

    public string LevelName { get; set; } = null!;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<StudentPlacementResult> StudentPlacementResults { get; set; } = new List<StudentPlacementResult>();
}
