using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Quiz
{
    public int QuizId { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public int? Duration { get; set; }

    public int PassScore { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}
