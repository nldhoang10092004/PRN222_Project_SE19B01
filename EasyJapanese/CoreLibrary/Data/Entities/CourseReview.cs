using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class CourseReview
{
    public int ReviewId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public byte Rating { get; set; }

    public string? Title { get; set; }

    public string? Comment { get; set; }

    public bool IsVisible { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<ReviewResponse> ReviewResponses { get; set; } = new List<ReviewResponse>();

    public virtual Student Student { get; set; } = null!;
}
