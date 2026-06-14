using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class ReviewResponse
{
    public int ResponseId { get; set; }

    public int ReviewId { get; set; }

    public int ResponderId { get; set; }

    public string Response { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Account Responder { get; set; } = null!;

    public virtual CourseReview Review { get; set; } = null!;
}
