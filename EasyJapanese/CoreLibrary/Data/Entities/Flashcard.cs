using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Flashcard
{
    public int FlashcardId { get; set; }

    public int StudentId { get; set; }

    public int? CourseId { get; set; }

    public string FrontText { get; set; } = null!;

    public string BackText { get; set; } = null!;

    public decimal Efactor { get; set; }

    public DateTime? NextReviewAt { get; set; }

    public int ReviewCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Student Student { get; set; } = null!;
}
