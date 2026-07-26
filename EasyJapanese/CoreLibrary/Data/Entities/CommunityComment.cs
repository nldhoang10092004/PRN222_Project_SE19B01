using System;

namespace CoreLibrary.Data.Entities
{
    public class CommunityComment
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual CommunityPost Post { get; set; } = null!;
    }
}
