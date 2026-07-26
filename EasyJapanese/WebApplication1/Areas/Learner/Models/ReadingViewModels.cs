namespace WebApplication1.Areas.Learner.Models
{
    public class ReadingListViewModel
    {
        public List<ReadingExerciseVm> Exercises { get; set; } = new();
    }

    public class ReadingExerciseVm
    {
        public int ExerciseId { get; set; }
        public string Title { get; set; } = "";
        public int QuestionCount { get; set; }
    }

    public class ReadingDetailViewModel
    {
        public int ExerciseId { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public List<ReadingQuestionVm> Questions { get; set; } = new();
    }

    public class ReadingQuestionVm
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = "";
        public int SortOrder { get; set; }
        public List<ReadingOptionVm> AnswerOptions { get; set; } = new();
    }

    public class ReadingOptionVm
    {
        public int OptionId { get; set; }
        public string AnswerText { get; set; } = "";
    }

    public class ReadingResultViewModel
    {
        public int ExerciseId { get; set; }
        public string Title { get; set; } = "";
        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public decimal ScorePercent { get; set; }
        public List<QuestionResultVm> Results { get; set; } = new();
    }

    public class QuestionResultVm
    {
        public string QuestionText { get; set; } = "";
        public string SelectedAnswer { get; set; } = "";
        public string CorrectAnswer { get; set; } = "";
        public bool IsCorrect { get; set; }
    }
}
