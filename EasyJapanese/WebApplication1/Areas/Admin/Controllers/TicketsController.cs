using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Const;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    [Route("admin/tickets")]
    public class TicketsController : Controller
    {
        public class ChatMessage
        {
            public string Sender { get; set; } // "User" or "Admin"
            public string MessageText { get; set; }
            public DateTime SentAt { get; set; }
        }

        public class SupportTicket
        {
            public int TicketId { get; set; }
            public string UserFullName { get; set; }
            public string UserEmail { get; set; }
            public string Category { get; set; }
            public string Subject { get; set; }
            public string Status { get; set; } // Open, InProgress, Resolved
            public DateTime CreatedAt { get; set; }
            public List<ChatMessage> ChatHistory { get; set; } = new List<ChatMessage>();
        }

        public static List<SupportTicket> _tickets = new List<SupportTicket>();

        public static SupportTicket GetTicketForUser(string userEmail)
        {
            if (string.IsNullOrEmpty(userEmail)) return null;
            lock (_tickets)
            {
                return _tickets.FirstOrDefault(t => t.UserEmail.Equals(userEmail, StringComparison.OrdinalIgnoreCase) && t.Status != "Resolved");
            }
        }

        public static SupportTicket GetOrCreateTicketForUser(string userEmail, string userFullName)
        {
            if (string.IsNullOrEmpty(userEmail)) userEmail = "khachhang@hijapan.vn";
            if (string.IsNullOrEmpty(userFullName)) userFullName = "Khách hàng";

            lock (_tickets)
            {
                var ticket = _tickets.FirstOrDefault(t => t.UserEmail.Equals(userEmail, StringComparison.OrdinalIgnoreCase) && t.Status != "Resolved");
                if (ticket == null)
                {
                    var nextId = _tickets.Any() ? _tickets.Max(t => t.TicketId) + 1 : 1001;
                    ticket = new SupportTicket
                    {
                        TicketId = nextId,
                        UserFullName = userFullName,
                        UserEmail = userEmail,
                        Category = "Hỗ trợ trực tuyến",
                        Subject = "Yêu cầu hỗ trợ từ học viên",
                        Status = "Open",
                        CreatedAt = DateTime.UtcNow,
                        ChatHistory = new List<ChatMessage>()
                    };
                    _tickets.Insert(0, ticket);
                }
                return ticket;
            }
        }

        public static void AddUserMessage(int ticketId, string messageText)
        {
            lock (_tickets)
            {
                var ticket = _tickets.FirstOrDefault(t => t.TicketId == ticketId);
                if (ticket != null && !string.IsNullOrWhiteSpace(messageText))
                {
                    ticket.ChatHistory.Add(new ChatMessage
                    {
                        Sender = "User",
                        MessageText = messageText,
                        SentAt = DateTime.UtcNow
                    });
                    if (ticket.Status == "Resolved")
                    {
                        ticket.Status = "InProgress";
                    }
                }
            }
        }

        [HttpGet("")]
        public IActionResult Index(int? selectedTicketId, string statusFilter)
        {
            ViewData["Title"] = "Ticket Support Chat";
            
            var query = _tickets.AsQueryable();
            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(t => t.Status == statusFilter);
            }

            var tickets = query.OrderByDescending(t => t.CreatedAt).ToList();
            ViewData["StatusFilter"] = statusFilter;

            SupportTicket selectedTicket = null;
            if (selectedTicketId.HasValue)
            {
                selectedTicket = _tickets.FirstOrDefault(t => t.TicketId == selectedTicketId.Value);
            }
            else if (tickets.Any())
            {
                selectedTicket = tickets.First();
            }

            ViewBag.SelectedTicket = selectedTicket;
            return View(tickets);
        }

        [HttpPost("send-message")]
        public IActionResult SendMessage(int ticketId, string messageText, string newStatus)
        {
            var ticket = _tickets.FirstOrDefault(t => t.TicketId == ticketId);
            if (ticket == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(messageText))
            {
                ticket.ChatHistory.Add(new ChatMessage
                {
                    Sender = "Admin",
                    MessageText = messageText,
                    SentAt = DateTime.UtcNow
                });

                // Auto advance status to InProgress if it was Open
                if (ticket.Status == "Open")
                {
                    ticket.Status = "InProgress";
                }
            }

            if (!string.IsNullOrEmpty(newStatus))
            {
                ticket.Status = newStatus;
            }

            return RedirectToAction(nameof(Index), new { selectedTicketId = ticketId, statusFilter = HttpContext.Request.Query["statusFilter"] });
        }

        [HttpPost("delete")]
        public IActionResult Delete(int ticketId)
        {
            var ticket = _tickets.FirstOrDefault(t => t.TicketId == ticketId);
            if (ticket == null) return NotFound();

            _tickets.Remove(ticket);
            TempData["SuccessMessage"] = "Đã xóa Ticket Support thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
