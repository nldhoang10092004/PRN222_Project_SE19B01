using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities;

public partial class SupportMessage
{
    public int MessageId { get; set; }

    public int TicketId { get; set; }

    public string Sender { get; set; } = null!;

    public string MessageText { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public virtual SupportTicket Ticket { get; set; } = null!;
}
