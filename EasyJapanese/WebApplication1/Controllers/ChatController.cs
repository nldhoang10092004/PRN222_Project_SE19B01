using CoreLibrary.Authentication;
using CoreLibrary.Data;
using CoreWeb.Models.ChatBot;
using CoreWeb.Service.ChatBot;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Areas.Admin.Controllers;

namespace WebApplication1.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatBotService _chatBot;
        private readonly IAuthenticationService _auth;
        private readonly AppDbContext _db;

        public ChatController(IChatBotService chatBot, IAuthenticationService auth, AppDbContext db)
        {
            _chatBot = chatBot;
            _auth = auth;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest(new { error = "Tin nhắn không được để trống." });

            var result = await _chatBot.AskAsync(request, ct);
            return Json(result);
        }

        private async Task<(string Email, string Name)> GetUserOrGuestIdentityAsync()
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser != null)
            {
                return (currentUser.Email, currentUser.FullName ?? currentUser.Email);
            }

            var guestEmail = HttpContext.Session.GetString("GuestSupportEmail");
            if (string.IsNullOrEmpty(guestEmail))
            {
                guestEmail = $"guest_{Guid.NewGuid().ToString().Substring(0, 8)}@hijapan.vn";
                HttpContext.Session.SetString("GuestSupportEmail", guestEmail);
            }
            var guestName = HttpContext.Session.GetString("GuestSupportName") ?? "Khách hàng";

            return (guestEmail, guestName);
        }

        [HttpGet]
        public async Task<IActionResult> GetSupportMessages()
        {
            var identity = await GetUserOrGuestIdentityAsync();
            var ticket = TicketsController.GetTicketForUser(_db, identity.Email);
            if (ticket == null)
            {
                return Json(new
                {
                    ticketId = 0,
                    userEmail = identity.Email,
                    userFullName = identity.Name,
                    status = "None",
                    messages = new List<object>()
                });
            }

            return Json(new
            {
                ticketId = ticket.TicketId,
                userEmail = ticket.UserEmail,
                userFullName = ticket.UserFullName,
                status = ticket.Status,
                messages = ticket.Messages.OrderBy(m => m.SentAt).Select(m => new
                {
                    sender = m.Sender,
                    messageText = m.MessageText,
                    sentAt = m.SentAt.ToString("HH:mm dd/MM/yyyy")
                })
            });
        }

        public class SupportMessageDto
        {
            public string Message { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SendSupportMessage([FromBody] SupportMessageDto request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest(new { error = "Tin nhắn không được để trống." });

            var identity = await GetUserOrGuestIdentityAsync();
            var ticket = TicketsController.GetOrCreateTicketForUser(_db, identity.Email, identity.Name);
            TicketsController.AddUserMessage(_db, ticket.TicketId, request.Message);

            return Json(new
            {
                success = true,
                ticketId = ticket.TicketId,
                userEmail = ticket.UserEmail,
                userFullName = ticket.UserFullName,
                messages = ticket.Messages.OrderBy(m => m.SentAt).Select(m => new
                {
                    sender = m.Sender,
                    messageText = m.MessageText,
                    sentAt = m.SentAt.ToString("HH:mm dd/MM/yyyy")
                })
            });
        }
    }
}