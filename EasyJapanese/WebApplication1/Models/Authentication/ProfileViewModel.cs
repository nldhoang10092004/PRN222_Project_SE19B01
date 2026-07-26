using System;

namespace WebApplication1.Models.Profile
{
    public class ProfileViewModel
    {
        public string Role { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? AvatarUrl { get; set; }

        // Student + Mentor
        public string? PhoneNumber { get; set; }

        // Student only
        public DateOnly? DateOfBirth { get; set; }
        public string? JlptLevel { get; set; }

        // Mentor only
        public string? Bio { get; set; }
        public string? Expertise { get; set; }
    }
}