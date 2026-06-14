using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Course
{
    public int CourseId { get; set; }

    public int LevelId { get; set; }

    public int? MentorId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsFree { get; set; }

    public bool IsPublished { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CourseReview> CourseReviews { get; set; } = new List<CourseReview>();

    public virtual Mentor CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();

    public virtual ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual JlptLevel Level { get; set; } = null!;

    public virtual Mentor? Mentor { get; set; }

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
