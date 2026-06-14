using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Student
{
    public int StudentId { get; set; }

    public string FullName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CourseReview> CourseReviews { get; set; } = new List<CourseReview>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();

    public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

    public virtual ICollection<StudentMembership> StudentMemberships { get; set; } = new List<StudentMembership>();

    public virtual Account StudentNavigation { get; set; } = null!;

    public virtual ICollection<StudentPlacementResult> StudentPlacementResults { get; set; } = new List<StudentPlacementResult>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
}
