using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Question
{
    public int QuestionId { get; set; }

    public int? TestId { get; set; }

    public int? QuizId { get; set; }

    public int? ExerciseId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string QuestionType { get; set; } = null!;

    public int Points { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();

    public virtual Exercise? Exercise { get; set; }

    public virtual Quiz? Quiz { get; set; }

    public virtual PlacementTest? Test { get; set; }
}
