using CoreWeb.Models.ChatBot;

namespace CoreWeb.Service.ChatBot
{
    public interface IJishoDictionaryService
    {
        Task<List<JishoWordResult>> SearchAsync(string keyword, CancellationToken ct = default);
    }
}
