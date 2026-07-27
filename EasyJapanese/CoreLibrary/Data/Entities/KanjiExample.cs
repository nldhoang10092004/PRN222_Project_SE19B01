using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class KanjiExample
{
    public int ExampleId { get; set; }

    public int KanjiId { get; set; }

    public string Word { get; set; } = null!;

    public string? Reading { get; set; }

    public string? Meaning { get; set; }

    public int SortOrder { get; set; }

    public virtual KanjiEntry Kanji { get; set; } = null!;
}
