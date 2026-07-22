using CoreLibrary.Authentication;
using CoreWeb.Models.ChatBot;
using CoreWeb.Service.ChatBot;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatBotService _chatBot;
        private readonly IAuthenticationService _auth;

        public ChatController(IChatBotService chatBot, IAuthenticationService auth)
        {
            _chatBot = chatBot;
            _auth = auth;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request, CancellationToken ct)
        {
            //var user = await _auth.GetCurrentUserAsync(HttpContext);
            //if (user == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest(new { error = "Tin nhắn không được để trống." });

            var result = await _chatBot.AskAsync(request, ct);
            return Json(result);
        }
    }
}