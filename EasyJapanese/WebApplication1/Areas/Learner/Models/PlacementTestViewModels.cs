namespace WebApplication1.Areas.Learner.Models
{
    public enum PlacementState { NotStarted, InProgress, Completed }

    public class PlacementTestViewModel
    {
        public int TestId { get; set; }
        public int Duration { get; set; }
        public PlacementState State { get; set; }
        public List<QuestionVm> Questions { get; set; } = new();
        public int CurrentIndex { get; set; }
        public int? SelectedOptionId { get; set; }

        public int Score { get; set; }
        public int TotalPoints { get; set; }
        public int CorrectCount { get; set; }
        public int RecommendedLevelId { get; set; }
        public string RecommendedLevelName { get; set; } = "";
    }

    public class QuestionVm
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = "";
        public int SortOrder { get; set; }
        public List<OptionVm> AnswerOptions { get; set; } = new();
    }

    public class OptionVm
    {
        public int OptionId { get; set; }
        public string AnswerText { get; set; } = "";
    }
}