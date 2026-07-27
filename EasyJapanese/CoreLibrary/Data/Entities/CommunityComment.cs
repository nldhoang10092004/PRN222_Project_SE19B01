using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class CommunityComment
{
    public int CommentId { get; set; }

    public int PostId { get; set; }

    public int AuthorId { get; set; }

    public string? AuthorName { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual CommunityPost Post { get; set; } = null!;
}
