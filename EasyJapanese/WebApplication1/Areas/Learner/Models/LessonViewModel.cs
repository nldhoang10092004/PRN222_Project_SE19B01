using System.Collections.Generic;

namespace CoreWeb.Areas.Learner.Models
{
    public class LessonViewModel
    {
        // Thông tin Lesson
        public int LessonId { get; set; }
        public int CourseId { get; set; }

        public string LessonTitle { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;

        public string? Content { get; set; }
        public string? VideoUrl { get; set; }

        // Các exercise được tách theo ExerciseType
        public List<LessonExerciseItemViewModel> VocabularyItems { get; set; } = new();
        public List<LessonExerciseItemViewModel> KanjiItems { get; set; } = new();
        public List<LessonExerciseItemViewModel> GrammarItems { get; set; } = new();
        public List<LessonExerciseItemViewModel> ReadingItems { get; set; } = new();
        public List<LessonExerciseItemViewModel> ListeningItems { get; set; } = new();
    }

    public class LessonExerciseItemViewModel
    {
        public int ExerciseId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }

        public string? AudioUrl { get; set; }
        public string? StrokeOrderUrl { get; set; }

        public int SortOrder { get; set; }
        public List<ExerciseQuestionViewModel> Questions { get; set; } = new();
    }

    public class ExerciseQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        public List<ExerciseAnswerOptionViewModel> Options { get; set; } = new();
    }

    public class ExerciseAnswerOptionViewModel
    {
        public int OptionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class ExerciseAnswerRequest
    {
        public int ExerciseId { get; set; }

        public List<ExerciseAnswerItem> Answers { get; set; } = new();
    }

    public class ExerciseAnswerItem
    {
        public int QuestionId { get; set; }

        public int OptionId { get; set; }
    }
}