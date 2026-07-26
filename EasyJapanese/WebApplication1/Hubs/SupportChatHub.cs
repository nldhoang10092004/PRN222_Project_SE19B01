using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Areas.Admin.Controllers;

namespace WebApplication1.Hubs
{
    public class SupportChatHub : Hub
    {
        public async Task JoinTicketGroup(int ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket_{ticketId}");
        }

        public async Task LeaveTicketGroup(int ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket_{ticketId}");
        }

        public async Task JoinAdminChannel()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin_support_channel");
        }

        public async Task SendUserMessage(int ticketId, string messageText, string userEmail, string userName)
        {
            if (string.IsNullOrWhiteSpace(messageText)) return;

            var ticket = TicketsController.GetOrCreateTicketForUser(
                string.IsNullOrEmpty(userEmail) ? "khachhang@hijapan.vn" : userEmail,
                string.IsNullOrEmpty(userName) ? "Khách hàng" : userName
            );

            TicketsController.AddUserMessage(ticket.TicketId, messageText);

            // Join caller connection to the ticket group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket_{ticket.TicketId}");

            var sentAt = DateTime.UtcNow.ToString("HH:mm dd/MM/yyyy");

            // Confirm ticket ID to caller
            await Clients.Caller.SendAsync("TicketCreated", new
            {
                ticketId = ticket.TicketId,
                userEmail = ticket.UserEmail,
                userFullName = ticket.UserFullName
            });

            await Clients.Group($"ticket_{ticket.TicketId}").SendAsync("ReceiveMessage", new
            {
                ticketId = ticket.TicketId,
                sender = "User",
                messageText = messageText,
                sentAt = sentAt
            });

            await Clients.Group("admin_support_channel").SendAsync("NewTicketOrMessage", new
            {
                ticketId = ticket.TicketId,
                userFullName = ticket.UserFullName,
                userEmail = ticket.UserEmail,
                subject = ticket.Subject,
                status = ticket.Status,
                sender = "User",
                messageText = messageText,
                sentAt = sentAt
            });
        }

        public async Task SendAdminReply(int ticketId, string messageText, string newStatus)
        {
            var ticket = TicketsController._tickets.FirstOrDefault(t => t.TicketId == ticketId);
            if (ticket == null) return;

            if (!string.IsNullOrWhiteSpace(messageText))
            {
                ticket.ChatHistory.Add(new TicketsController.ChatMessage
                {
                    Sender = "Admin",
                    MessageText = messageText,
                    SentAt = DateTime.UtcNow
                });

                if (ticket.Status == "Open")
                {
                    ticket.Status = "InProgress";
                }
            }

            if (!string.IsNullOrEmpty(newStatus))
            {
                ticket.Status = newStatus;
            }

            var sentAt = DateTime.UtcNow.ToString("HH:mm dd/MM/yyyy");

            await Clients.Group($"ticket_{ticketId}").SendAsync("ReceiveMessage", new
            {
                ticketId = ticketId,
                sender = "Admin",
                messageText = messageText,
                sentAt = sentAt,
                status = ticket.Status
            });

            await Clients.Group("admin_support_channel").SendAsync("NewTicketOrMessage", new
            {
                ticketId = ticket.TicketId,
                userFullName = ticket.UserFullName,
                userEmail = ticket.UserEmail,
                subject = ticket.Subject,
                status = ticket.Status,
                sender = "Admin",
                messageText = messageText,
                sentAt = sentAt
            });
        }
    }
}
