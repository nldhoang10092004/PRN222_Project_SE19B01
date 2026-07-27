using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class CommunityPost
{
    public int PostId { get; set; }

    public int AuthorId { get; set; }

    public string? AuthorName { get; set; }

    public string AuthorRole { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public int LikeCount { get; set; }

    public int ViewCount { get; set; }

    public bool IsApproved { get; set; }

    public bool IsPinned { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CommunityComment> CommunityComments { get; set; } = new List<CommunityComment>();

    public virtual ICollection<CommunityLike> CommunityLikes { get; set; } = new List<CommunityLike>();
}
