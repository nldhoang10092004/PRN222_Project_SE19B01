using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class CommunityLike
{
    public int LikeId { get; set; }

    public int PostId { get; set; }

    public int AccountId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CommunityPost Post { get; set; } = null!;
}
