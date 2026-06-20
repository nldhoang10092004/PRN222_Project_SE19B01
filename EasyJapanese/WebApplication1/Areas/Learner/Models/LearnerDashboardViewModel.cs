namespace CoreWeb.Areas.Learner.Models;

public class LearnerDashboardViewModel
{
    public string FullName { get; set; } = string.Empty;

    // Hai cờ trạng thái quyết định 4 nhánh hiển thị
    public bool HasActiveMembership { get; set; }
    public bool HasCompletedPlacementTest { get; set; }

    public int StreakDays { get; set; }

    public ContinueLessonViewModel? ContinueLesson { get; set; }
    public LearnerStatsViewModel Stats { get; set; } = new();
    public JlptGoalViewModel? JlptGoal { get; set; }
    public List<FlashcardReviewItemViewModel> FlashcardsDue { get; set; } = new();

    // Dùng khi chưa có membership — hiển thị card chọn gói
    public List<PlanPreviewViewModel> AvailablePlans { get; set; } = new();
}

public class ContinueLessonViewModel
{
    public int LessonId { get; set; }
    public int CourseId { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public int WatchedSeconds { get; set; }
    public int PercentComplete { get; set; }
}

public class LearnerStatsViewModel
{
    public int LessonsCompleted { get; set; }
    public int FlashcardsLearned { get; set; }
    public int CoursesEnrolled { get; set; }
}

public class JlptGoalViewModel
{
    public string RecommendedLevelName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class FlashcardReviewItemViewModel
{
    public int FlashcardId { get; set; }
    public string FrontText { get; set; } = string.Empty;
    public string BackText { get; set; } = string.Empty;
    public string? CourseTitle { get; set; }
}

public class PlanPreviewViewModel
{
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
}
