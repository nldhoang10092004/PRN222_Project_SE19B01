using System;

namespace CoreLibrary.Data.Entities;

public partial class StudentExerciseResult
{
    public int ResultId { get; set; }

    public int StudentId { get; set; }

    public int ExerciseId { get; set; }

    public int Score { get; set; }

    public int TotalQuestions { get; set; }

    public int CorrectAnswers { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public virtual Student Student { get; set; } = null!;

    public virtual Exercise Exercise { get; set; } = null!;
}
