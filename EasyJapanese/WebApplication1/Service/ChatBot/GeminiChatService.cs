using CoreWeb.Models.ChatBot;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreWeb.Service.ChatBot
{
    public class GeminiChatService : IGeminiChatService
    {
        private readonly HttpClient _http;
        private readonly GeminiOptions _options;
        private readonly ILogger<GeminiChatService> _logger;

        private const string SystemPrompt =
            "Bạn là trợ lý học tiếng Nhật của nền tảng \"Hi Japan!\". " +
            "Luôn trả lời bằng tiếng Việt, ngắn gọn, thân thiện, đúng trọng tâm. " +
            "Nếu người dùng hỏi nghĩa một từ và có dữ liệu từ điển được cung cấp, " +
            "hãy diễn giải lại dữ liệu đó một cách dễ hiểu thay vì chỉ liệt kê nguyên văn.";

        public GeminiChatService(
            HttpClient http,
            IOptions<GeminiOptions> options,
            ILogger<GeminiChatService> logger)
        {
            _http = http;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> GenerateReplyAsync(string userMessage, List<ChatMessageDto>? history, string? dictionaryContext, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                return "Chatbot chưa được cấu hình API key.";
            }

            if (string.IsNullOrWhiteSpace(_options.BaseUrl))
            {
                return "Chatbot chưa được cấu hình Gemini BaseUrl.";
            }

            if (string.IsNullOrWhiteSpace(_options.Model))
            {
                return "Chatbot chưa được cấu hình Gemini Model.";
            }

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "Bạn chưa nhập nội dung cần hỏi.";
            }

            var contents = new List<GeminiContent>();

            foreach (var item in (history ?? new List<ChatMessageDto>())
                         .Where(x => !string.IsNullOrWhiteSpace(x.Content))
                         .TakeLast(10))
            {
                contents.Add(new GeminiContent
                {
                    Role = item.Role?.Equals(
                        "model",
                        StringComparison.OrdinalIgnoreCase) == true
                            ? "model"
                            : "user",

                    Parts = new List<GeminiPart>
                    {
                        new()
                        {
                            Text = item.Content
                        }
                    }
                });
            }

            var finalUserText =
                string.IsNullOrWhiteSpace(dictionaryContext)
                    ? userMessage.Trim()
                    : $"""
                       {userMessage.Trim()}

                       [Dữ liệu từ điển tham khảo]
                       {dictionaryContext}
                       """;

            contents.Add(new GeminiContent
            {
                Role = "user",
                Parts = new List<GeminiPart>
                {
                    new()
                    {
                        Text = finalUserText
                    }
                }
            });

            var requestBody = new GeminiRequest
            {
                SystemInstruction = new GeminiContent
                {
                    Parts = new List<GeminiPart>
                    {
                        new()
                        {
                            Text = SystemPrompt
                        }
                    }
                },

                Contents = contents,

                GenerationConfig = new GeminiGenerationConfig
                {
                    Temperature = 0.6,
                    MaxOutputTokens = 1024
                }
            };

            var url =$"{_options.BaseUrl.TrimEnd('/')}/" + $"{_options.Model.Trim()}:generateContent";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            Console.WriteLine(url);
            request.Headers.TryAddWithoutValidation(
                "x-goog-api-key",
                _options.ApiKey.Trim()
            );

            request.Content = JsonContent.Create(
                requestBody,
                options: new JsonSerializerOptions
                {
                    DefaultIgnoreCondition =
                        JsonIgnoreCondition.WhenWritingNull
                }
            );

            try
            {
                using var response = await _http.SendAsync(
                    request,
                    ct
                );

                var responseJson =
                    await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Gemini request failed. Status: {Status}. URL: {Url}. Response: {Response}",
                        (int)response.StatusCode,
                        url,
                        responseJson
                    );

                    return GetFriendlyErrorMessage(
                        (int)response.StatusCode,
                        responseJson
                    );
                }

                var parsed =
                    JsonSerializer.Deserialize<GeminiResponse>(
                        responseJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    );

                var reply = parsed?
                    .Candidates?
                    .FirstOrDefault()?
                    .Content?
                    .Parts?
                    .FirstOrDefault(part =>
                        !string.IsNullOrWhiteSpace(part.Text))?
                    .Text;

                if (string.IsNullOrWhiteSpace(reply))
                {
                    _logger.LogWarning(
                        "Gemini response has no answer. Response: {Response}",
                        responseJson
                    );

                    return "Xin lỗi, Gemini không trả về nội dung phù hợp.";
                }

                return reply.Trim();
            }
            catch (OperationCanceledException)
                when (!ct.IsCancellationRequested)
            {
                _logger.LogError(
                    "Gemini request timed out."
                );

                return "Kết nối tới AI quá thời gian. Vui lòng thử lại.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to connect to Gemini API."
                );

                return "Không thể kết nối tới Gemini API. Hãy kiểm tra Internet, URL và chứng chỉ HTTPS.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected Gemini error."
                );

                return "Có lỗi xảy ra khi xử lý câu trả lời từ AI.";
            }
        }

        private static string GetFriendlyErrorMessage(
            int statusCode,
            string responseBody)
        {
            return statusCode switch
            {
                400 =>
                    $"Gemini trả về lỗi 400. Request không hợp lệ hoặc tài khoản chưa đáp ứng điều kiện. Chi tiết: {responseBody}",

                401 =>
                    $"Gemini trả về lỗi 401. API key không hợp lệ. Chi tiết: {responseBody}",

                403 =>
                    $"Gemini trả về lỗi 403. API key hoặc project không có quyền truy cập. Chi tiết: {responseBody}",

                404 =>
                    $"Gemini trả về lỗi 404. Không tìm thấy model hoặc endpoint. Chi tiết: {responseBody}",

                429 =>
                    $"Gemini trả về lỗi 429. Đã vượt giới hạn request hoặc quota. Chi tiết: {responseBody}",

                >= 500 =>
                    $"Máy chủ Gemini đang gặp sự cố. Chi tiết: {responseBody}",

                _ =>
                    $"Gemini trả về lỗi {statusCode}. Chi tiết: {responseBody}"
            };
        }

        private class GeminiRequest
        {
            [JsonPropertyName("contents")]
            public List<GeminiContent> Contents { get; set; } = new();

            [JsonPropertyName("systemInstruction")]
            public GeminiContent? SystemInstruction { get; set; }

            [JsonPropertyName("generationConfig")]
            public GeminiGenerationConfig? GenerationConfig { get; set; }
        }

        private class GeminiContent
        {
            [JsonPropertyName("role")]
            public string? Role { get; set; }

            [JsonPropertyName("parts")]
            public List<GeminiPart> Parts { get; set; } = new();
        }

        private class GeminiPart
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
        }

        private class GeminiGenerationConfig
        {
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; }

            [JsonPropertyName("maxOutputTokens")]
            public int MaxOutputTokens { get; set; }
        }

        private class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public List<GeminiCandidate>? Candidates { get; set; }

            [JsonPropertyName("promptFeedback")]
            public GeminiPromptFeedback? PromptFeedback { get; set; }
        }

        private class GeminiCandidate
        {
            [JsonPropertyName("content")]
            public GeminiContent? Content { get; set; }

            [JsonPropertyName("finishReason")]
            public string? FinishReason { get; set; }
        }

        private class GeminiPromptFeedback
        {
            [JsonPropertyName("blockReason")]
            public string? BlockReason { get; set; }
        }
    }
}