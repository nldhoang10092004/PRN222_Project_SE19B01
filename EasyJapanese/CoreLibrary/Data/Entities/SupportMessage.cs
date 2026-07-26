using System;

namespace CoreLibrary.Data.Entities
{
    public class SupportMessage
    {
        public int MessageId { get; set; }
        public int TicketId { get; set; }
        public string Sender { get; set; } = "User"; // User, Admin
        public string MessageText { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public virtual SupportTicket Ticket { get; set; } = null!;
    }
}
