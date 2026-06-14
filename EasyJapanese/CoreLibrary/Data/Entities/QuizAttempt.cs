using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class QuizAttempt
{
    public int AttemptId { get; set; }

    public int StudentId { get; set; }

    public int QuizId { get; set; }

    public int Score { get; set; }

    public int TotalPoints { get; set; }

    public bool IsPassed { get; set; }

    public string? SelectedAnswers { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
