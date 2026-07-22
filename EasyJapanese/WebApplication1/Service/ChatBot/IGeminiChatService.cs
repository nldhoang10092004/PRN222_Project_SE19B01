using CoreWeb.Models.ChatBot;

namespace CoreWeb.Service.ChatBot
{
    public interface IGeminiChatService
    {
        Task<string> GenerateReplyAsync(string userMessage, List<ChatMessageDto> history,string? dictionaryContext, CancellationToken ct = default);
    }
}
