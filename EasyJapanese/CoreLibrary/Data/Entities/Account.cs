using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class Account
{
    public int AccountId { get; set; }

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string Role { get; set; } = null!;

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }

    public bool IsLocked { get; set; }

    public bool IsEmailVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual Mentor? Mentor { get; set; }

    public virtual ICollection<ReviewResponse> ReviewResponses { get; set; } = new List<ReviewResponse>();

    public virtual Student? Student { get; set; }
}
