using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities
{
    public class CommunityPost
    {
        public int PostId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorRole { get; set; } = "Student";
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "Kinh nghiệm học";
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int LikeCount { get; set; } = 0;
        public int ViewCount { get; set; } = 0;
        public bool IsApproved { get; set; } = true;
        public bool IsPinned { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CommunityComment> Comments { get; set; } = new List<CommunityComment>();
        public virtual ICollection<CommunityLike> Likes { get; set; } = new List<CommunityLike>();
    }
}
