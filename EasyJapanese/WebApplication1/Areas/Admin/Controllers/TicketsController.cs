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

        private static List<SupportTicket> _tickets = new List<SupportTicket>
        {
            new SupportTicket
            {
                TicketId = 1001,
                UserFullName = "Nguyễn Văn An",
                UserEmail = "an.nguyen@gmail.com",
                Category = "Thanh toán",
                Subject = "Không tự động nâng cấp VIP sau khi quét mã thanh toán",
                Status = "Open",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                ChatHistory = new List<ChatMessage>
                {
                    new ChatMessage { Sender = "User", MessageText = "Chào Ad, mình vừa thực hiện thanh toán chuyển khoản gói 3 tháng qua cổng VNPAY lúc 9h sáng nay.", SentAt = DateTime.UtcNow.AddHours(-2) },
                    new ChatMessage { Sender = "User", MessageText = "Tiền trong tài khoản ngân hàng của mình đã bị trừ 199.000đ rồi, mã giao dịch là VP120349. Nhưng tài khoản trên web vẫn báo là Basic. Mong Ad kích hoạt hộ nhé.", SentAt = DateTime.UtcNow.AddHours(-1).AddMinutes(-50) }
                }
            },
            new SupportTicket
            {
                TicketId = 1002,
                UserFullName = "Trần Thị Bình",
                UserEmail = "binh.tran@yahoo.com",
                Category = "Tài khoản",
                Subject = "Lỗi không đăng nhập được bằng Google",
                Status = "InProgress",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ChatHistory = new List<ChatMessage>
                {
                    new ChatMessage { Sender = "User", MessageText = "Chào admin, em dùng tài khoản Google để đăng nhập nhưng cứ bị báo lỗi Auth Timeout hoài.", SentAt = DateTime.UtcNow.AddDays(-1) },
                    new ChatMessage { Sender = "Admin", MessageText = "Chào em, em thử đăng nhập bằng trình duyệt ẩn danh hoặc xóa cookie xem có được không nhé.", SentAt = DateTime.UtcNow.AddHours(-5) },
                    new ChatMessage { Sender = "User", MessageText = "Em đã xóa cookie và thử lại rồi nhưng vẫn báo lỗi như cũ ạ, hình như do server kết nối Google có vấn đề.", SentAt = DateTime.UtcNow.AddHours(-4) }
                }
            },
            new SupportTicket
            {
                TicketId = 1003,
                UserFullName = "Phạm Hồng Đăng",
                UserEmail = "dangpham@outlook.com",
                Category = "Nội dung bài học",
                Subject = "Sai đáp án bài tập trắc nghiệm Kanji N3",
                Status = "Resolved",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ChatHistory = new List<ChatMessage>
                {
                    new ChatMessage { Sender = "User", MessageText = "Ở bài trắc nghiệm N3 phần Kanji ôn tập bài 4, đáp án câu 5 đang bị sai. Mong thầy cô xem lại.", SentAt = DateTime.UtcNow.AddDays(-3) },
                    new ChatMessage { Sender = "Admin", MessageText = "Cảm ơn bạn đã đóng góp. Đội ngũ học thuật đã chỉnh sửa lại đáp án chính xác của câu hỏi này.", SentAt = DateTime.UtcNow.AddDays(-2) },
                    new ChatMessage { Sender = "User", MessageText = "Dạ vâng em cảm ơn admin nhiều ạ, chúc web ngày càng phát triển.", SentAt = DateTime.UtcNow.AddDays(-2).AddMinutes(30) }
                }
            }
        };

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
