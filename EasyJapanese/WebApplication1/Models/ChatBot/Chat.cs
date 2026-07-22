namespace CoreWeb.Models.ChatBot
{
    public class GeminiOptions
    {
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "gemini-2.5-flash";
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/models";
    }

    public class JishoOptions
    {
        public string BaseUrl { get; set; } = "https://jisho.org/api/v1/search/words";
    }

    public class ChatMessageDto
    {
        public string Role { get; set; } = "user"; // "user" hoặc "model"
        public string Content { get; set; } = "";
    }

    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;

        public string Mode { get; set; } = "chat";

        public List<ChatMessageDto> History { get; set; } = new();
    }

    public class ChatResponseDto
    {
        public string Reply { get; set; } = "";
        public JishoWordResult? DictionaryCard { get; set; }
    }


    public class JishoWordResult
    {
        public string Word { get; set; } = "";
        public string Reading { get; set; } = "";
        public List<string> Meanings { get; set; } = new();
        public List<string> PartsOfSpeech { get; set; } = new();
    }
}
