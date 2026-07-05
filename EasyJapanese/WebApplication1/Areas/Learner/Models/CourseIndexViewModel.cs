using System.Collections.Generic;

namespace CoreWeb.Areas.Learner.Models
{
    public class CourseIndexViewModel
    {
        // Navbar
        public string CurrentUserName { get; set; } = string.Empty;
        public string CurrentUserInitial { get; set; } = string.Empty;
        public bool HasMembership { get; set; }
        public bool HasCompletedPlacement { get; set; }

        // DB: StudentPlacementResults → JlptLevels.LevelName
        // null = chưa làm test
        public string? RecommendedLevelName { get; set; }

        // Danh sách course nhóm theo level (đã filter theo logic)
        public List<CourseLevelGroupViewModel> GroupedCourses { get; set; } = new();
    }

    public class CourseLevelGroupViewModel
    {
        // DB: JlptLevels.LevelName
        public string LevelName { get; set; } = string.Empty;

        // DB: JlptLevels.SortOrder — dùng để sort group
        public int SortOrder { get; set; }

        public List<CourseCardViewModel> Courses { get; set; } = new();
    }

    public class CourseCardViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsFree { get; set; }
        public bool IsEnrolled { get; set; }

        // true  = có thể truy cập (free, hoặc có membership)
        // false = cần membership
        public bool IsAccessible { get; set; }

        public string? MentorName { get; set; }
        public string? MentorExpertise { get; set; }
        public string? MentorAvatarUrl { get; set; }
    }
}