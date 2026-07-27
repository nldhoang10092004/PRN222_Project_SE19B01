using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class KanjiEntry
{
    public int KanjiId { get; set; }

    public int LevelId { get; set; }

    public string Character { get; set; } = null!;

    public string? Meaning { get; set; }

    public string? OnYomi { get; set; }

    public string? KunYomi { get; set; }

    public int? StrokeCount { get; set; }

    public string? StrokeOrderUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<KanjiExample> KanjiExamples { get; set; } = new List<KanjiExample>();

    public virtual JlptLevel Level { get; set; } = null!;
}
