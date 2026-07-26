using System;

namespace CoreLibrary.Data.Entities
{
    public class CommunityLike
    {
        public int LikeId { get; set; }
        public int PostId { get; set; }
        public int AccountId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual CommunityPost Post { get; set; } = null!;
    }
}
