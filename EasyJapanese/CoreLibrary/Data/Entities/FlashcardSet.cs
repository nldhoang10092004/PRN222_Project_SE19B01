using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class FlashcardSet
{
    public int FlashcardSetId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int? CourseId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();

    public virtual Account CreatedByNavigation { get; set; } = null!;
}