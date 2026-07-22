using CoreWeb.Models.ChatBot;

namespace CoreWeb.Service.ChatBot
{
    public interface IChatBotService
    {
        Task<ChatResponseDto> AskAsync(ChatRequestDto request, CancellationToken ct = default);
    }
}
