using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class SupportTicket
{
    public int TicketId { get; set; }

    public string UserEmail { get; set; } = null!;

    public string? UserFullName { get; set; }

    public string Category { get; set; } = null!;

    public string? Subject { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<SupportMessage> SupportMessages { get; set; } = new List<SupportMessage>();
}
