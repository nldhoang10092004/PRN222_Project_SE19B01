using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreLibrary.Const;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    [Route("admin/tickets")]
    public class TicketsController : Controller
    {
        private readonly AppDbContext _context;

        public TicketsController(AppDbContext context)
        {
            _context = context;
        }

        public static SupportTicket? GetTicketForUser(AppDbContext db, string userEmail)
        {
            if (string.IsNullOrEmpty(userEmail)) return null;
            return db.SupportTickets
                .Include(t => t.Messages)
                .FirstOrDefault(t => t.UserEmail.ToLower() == userEmail.ToLower() && t.Status != "Resolved");
        }

        public static SupportTicket GetOrCreateTicketForUser(AppDbContext db, string userEmail, string userFullName)
        {
            if (string.IsNullOrEmpty(userEmail)) userEmail = "khachhang@hijapan.vn";
            if (string.IsNullOrEmpty(userFullName)) userFullName = "Khách hàng";

            var ticket = db.SupportTickets
                .Include(t => t.Messages)
                .FirstOrDefault(t => t.UserEmail.ToLower() == userEmail.ToLower() && t.Status != "Resolved");

            if (ticket == null)
            {
                ticket = new SupportTicket
                {
                    UserFullName = userFullName,
                    UserEmail = userEmail,
                    Category = "Hỗ trợ trực tuyến",
                    Subject = "Yêu cầu hỗ trợ từ học viên",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                db.SupportTickets.Add(ticket);
                db.SaveChanges();
            }
            return ticket;
        }

        public static void AddUserMessage(AppDbContext db, int ticketId, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText)) return;

            var ticket = db.SupportTickets.Find(ticketId);
            if (ticket != null)
            {
                var msg = new SupportMessage
                {
                    TicketId = ticketId,
                    Sender = "User",
                    MessageText = messageText.Trim(),
                    SentAt = DateTime.UtcNow
                };
                db.SupportMessages.Add(msg);
                ticket.UpdatedAt = DateTime.UtcNow;

                if (ticket.Status == "Resolved")
                {
                    ticket.Status = "InProgress";
                }
                db.SaveChanges();
            }
        }

        public static void AddAdminReply(AppDbContext db, int ticketId, string messageText, string newStatus)
        {
            var ticket = db.SupportTickets.Find(ticketId);
            if (ticket != null)
            {
                if (!string.IsNullOrWhiteSpace(messageText))
                {
                    var msg = new SupportMessage
                    {
                        TicketId = ticketId,
                        Sender = "Admin",
                        MessageText = messageText.Trim(),
                        SentAt = DateTime.UtcNow
                    };
                    db.SupportMessages.Add(msg);

                    if (ticket.Status == "Open")
                    {
                        ticket.Status = "InProgress";
                    }
                }

                if (!string.IsNullOrEmpty(newStatus))
                {
                    ticket.Status = newStatus;
                }

                ticket.UpdatedAt = DateTime.UtcNow;
                db.SaveChanges();
            }
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? selectedTicketId, string? statusFilter)
        {
            ViewData["Title"] = "Ticket Support Chat";
            
            var query = _context.SupportTickets
                .Include(t => t.Messages)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(t => t.Status == statusFilter);
            }

            var tickets = await query.OrderByDescending(t => t.UpdatedAt).ToListAsync();
            ViewData["StatusFilter"] = statusFilter;

            SupportTicket? selectedTicket = null;
            if (selectedTicketId.HasValue)
            {
                selectedTicket = await _context.SupportTickets
                    .Include(t => t.Messages)
                    .FirstOrDefaultAsync(t => t.TicketId == selectedTicketId.Value);
            }

            if (selectedTicket == null && tickets.Any())
            {
                selectedTicket = tickets.First();
            }

            ViewBag.SelectedTicket = selectedTicket;
            return View(tickets);
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage(int ticketId, string messageText, string newStatus)
        {
            AddAdminReply(_context, ticketId, messageText, newStatus);
            return RedirectToAction(nameof(Index), new { selectedTicketId = ticketId, statusFilter = HttpContext.Request.Query["statusFilter"] });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int ticketId)
        {
            var ticket = await _context.SupportTickets
                .Include(t => t.Messages)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null) return NotFound();

            _context.SupportTickets.Remove(ticket);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa Ticket Support thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
