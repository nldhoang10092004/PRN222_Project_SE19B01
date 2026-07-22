using CoreWeb.Models.ChatBot;
using System.Text.RegularExpressions;

namespace CoreWeb.Service.ChatBot
{
    public class ChatBotService : IChatBotService
    {
        private readonly IJishoDictionaryService _jisho;
        private readonly IGeminiChatService _gemini;

        private static readonly Regex AskMeaningRegex = new(
            @"(?:nghĩa\s+(?:của\s+)?[""']?(?<w1>.+?)[""']?\s+là\s+gì)" +
            @"|(?:[""']?(?<w2>.+?)[""']?\s+nghĩa\s+là\s+gì)" +
            @"|(?:tra\s+từ\s*[:\s]+(?<w3>.+))" +
            @"|(?:[""']?(?<w4>.+?)[""']?\s+(?:là\s+gì|có\s+nghĩa\s+là\s+gì))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static readonly Regex JapaneseCharsRegex = new(
            @"^[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}ー\s]+$",
            RegexOptions.Compiled
        );

        public ChatBotService(
            IJishoDictionaryService jisho,
            IGeminiChatService gemini)
        {
            _jisho = jisho;
            _gemini = gemini;
        }

        public async Task<ChatResponseDto> AskAsync(
            ChatRequestDto request,
            CancellationToken ct = default)
        {
            var message = (request.Message ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(message))
            {
                return new ChatResponseDto
                {
                    Reply = "Bạn chưa nhập nội dung."
                };
            }

            var mode = (request.Mode ?? "chat")
                .Trim()
                .ToLowerInvariant();

            if (mode == "dictionary")
            {
                return await SearchDictionaryAsync(message, ct);
            }

            return await ChatWithGeminiAsync(request, message, ct);
        }

        private async Task<ChatResponseDto> SearchDictionaryAsync(
            string message,
            CancellationToken ct)
        {
            var keyword = ExtractDictionaryQuery(message) ?? message;

            var results = await _jisho.SearchAsync(keyword, ct);
            var card = results.FirstOrDefault();

            if (card == null)
            {
                return new ChatResponseDto
                {
                    Reply = $"Không tìm thấy từ \"{keyword}\" trong từ điển.",
                    DictionaryCard = null
                };
            }

            return new ChatResponseDto
            {
                Reply = string.Empty,
                DictionaryCard = card
            };
        }

        private async Task<ChatResponseDto> ChatWithGeminiAsync(
            ChatRequestDto request,
            string message,
            CancellationToken ct)
        {
            var reply = await _gemini.GenerateReplyAsync(
                message,
                request.History ?? new List<ChatMessageDto>(),
                dictionaryContext: null,
                ct
            );

            return new ChatResponseDto
            {
                Reply = reply,
                DictionaryCard = null
            };
        }

        private static string? ExtractDictionaryQuery(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            var match = AskMeaningRegex.Match(message);

            if (match.Success)
            {
                var word =
                    match.Groups["w1"].Success
                        ? match.Groups["w1"].Value
                        : match.Groups["w2"].Success
                            ? match.Groups["w2"].Value
                            : match.Groups["w3"].Success
                                ? match.Groups["w3"].Value
                                : match.Groups["w4"].Value;

                word = word
                    .Trim()
                    .Trim('"', '\'', '?', '.', '。', '？');

                if (!string.IsNullOrWhiteSpace(word) &&
                    word.Length <= 40)
                {
                    return word;
                }
            }

            if (message.Length <= 20 &&
                JapaneseCharsRegex.IsMatch(message))
            {
                return message;
            }

            return null;
        }
    }
}