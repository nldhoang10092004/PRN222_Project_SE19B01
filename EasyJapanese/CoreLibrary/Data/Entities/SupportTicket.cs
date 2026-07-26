using System;
using System.Collections.Generic;

namespace CoreLibrary.Data.Entities
{
    public class SupportTicket
    {
        public int TicketId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string Category { get; set; } = "Hỗ trợ trực tuyến";
        public string Subject { get; set; } = "Yêu cầu hỗ trợ từ học viên";
        public string Status { get; set; } = "Open"; // Open, InProgress, Resolved
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
    }
}
