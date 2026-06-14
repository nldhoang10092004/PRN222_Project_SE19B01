using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Mentor
{
    public int MentorId { get; set; }

    public string FullName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Bio { get; set; }

    public string? Expertise { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Course> CourseCreatedByNavigations { get; set; } = new List<Course>();

    public virtual ICollection<Course> CourseMentors { get; set; } = new List<Course>();

    public virtual Account MentorNavigation { get; set; } = null!;
}
