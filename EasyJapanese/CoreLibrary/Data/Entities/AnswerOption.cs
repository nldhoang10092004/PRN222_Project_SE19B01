using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class AnswerOption
{
    public int OptionId { get; set; }

    public int QuestionId { get; set; }

    public string AnswerText { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public virtual Question Question { get; set; } = null!;
}
