using CoreWeb.Models.ChatBot;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace CoreWeb.Service.AI
{
    public class WritingAIService : IWritingAiService
    {
            private readonly HttpClient _http;
            private readonly string _apiKey;
            private readonly string _model;
            private readonly string _baseUrl;

            private static readonly JsonSerializerOptions JsonOpts = new()
            {
                PropertyNameCaseInsensitive = true
            };

            public WritingAIService(HttpClient http, IConfiguration configuration)
            {
                _http = http;
                _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Thiếu cấu hình Gemini:ApiKey");
                _model = configuration["Gemini:Model"] ?? "gemini-3-flash-preview";
                _baseUrl = configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/models";
            }

            public async Task<EssayFeedbackResult> GradeEssayAsync(
                string topic, string levelHint, string studentText,
                CancellationToken cancellationToken = default)
            {
                var prompt = $@"
Bạn là giáo viên tiếng Nhật chấm bài luận cho học viên trình độ {levelHint}.
Chủ đề bài viết: ""{topic}""
Bài viết của học viên:
---
{studentText}
---

Hãy chấm điểm (0-100), liệt kê các lỗi ngữ pháp/từ vựng cần sửa (nếu có), điểm mạnh, và điểm cần cải thiện.
CHỈ trả về JSON đúng schema sau, không thêm chữ nào khác:
{{
  ""score"": number,
  ""overallComment"": string (tiếng Việt),
  ""corrections"": [{{ ""original"": string, ""corrected"": string, ""explanation"": string (tiếng Việt) }}],
  ""strengths"": [string (tiếng Việt)],
  ""improvements"": [string (tiếng Việt)]
}}";

                var json = await CallGeminiAsync(prompt, cancellationToken);
                return JsonSerializer.Deserialize<EssayFeedbackResult>(json, JsonOpts)
                       ?? new EssayFeedbackResult { OverallComment = "Không thể phân tích kết quả." };
            }

            public async Task<TranslationFeedbackResult> GradeTranslationAsync(
                string vietnameseSentence, string levelHint, string studentText,
                CancellationToken cancellationToken = default)
            {
                var prompt = $@"
Bạn là giáo viên tiếng Nhật chấm bài dịch cho học viên trình độ {levelHint}.
Câu tiếng Việt cần dịch: ""{vietnameseSentence}""
Bản dịch tiếng Nhật của học viên:
---
{studentText}
---

Đánh giá bản dịch có đúng nghĩa và ngữ pháp không, chấm điểm (0-100), đưa ra 1 bản dịch tham khảo tốt hơn (nếu cần) và giải thích ngắn gọn.
CHỈ trả về JSON đúng schema sau, không thêm chữ nào khác:
{{
  ""isCorrect"": boolean,
  ""score"": number,
  ""suggestedTranslation"": string,
  ""explanation"": string (tiếng Việt),
  ""notes"": [string (tiếng Việt)]
}}";

                var json = await CallGeminiAsync(prompt, cancellationToken);
                return JsonSerializer.Deserialize<TranslationFeedbackResult>(json, JsonOpts)
                       ?? new TranslationFeedbackResult { Explanation = "Không thể phân tích kết quả." };
            }

        public async Task<string> GenerateEssayTopicAsync(
    string levelHint, CancellationToken cancellationToken = default)
        {
            var prompt = $@"
Hãy tạo 1 đề bài luyện viết tiếng Nhật cho học viên trình độ {levelHint}.
Đề bài viết bằng tiếng Việt, ngắn gọn (1 câu), thực tế, phù hợp với vốn từ/ngữ pháp trình độ {levelHint},
không trùng các chủ đề quá quen thuộc như ""giới thiệu bản thân"" nếu có thể, hãy đa dạng chủ đề (sở thích, công việc, du lịch, gia đình, ước mơ, thói quen, ý kiến cá nhân, v.v).

CHỈ trả về JSON đúng schema sau, không thêm chữ nào khác:
{{ ""topic"": string }}";

            var json = await CallGeminiAsync(prompt, cancellationToken);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("topic", out var t)
                ? t.GetString() ?? "Viết về một chủ đề bạn yêu thích."
                : "Viết về một chủ đề bạn yêu thích.";
        }

        public async Task<string> GenerateTranslationSentenceAsync(
            string levelHint, CancellationToken cancellationToken = default)
        {
            var prompt = $@"
Hãy tạo 1 câu tiếng Việt để học viên dịch sang tiếng Nhật, phù hợp trình độ ngữ pháp {levelHint}.
Câu nên tự nhiên, độ dài vừa phải, đa dạng chủ đề (đời sống hàng ngày, công việc, cảm xúc, thời tiết, dự định, v.v),
không lặp lại các câu quá phổ biến kiểu ""Tôi là sinh viên"".

CHỈ trả về JSON đúng schema sau, không thêm chữ nào khác:
{{ ""sentence"": string }}";

            var json = await CallGeminiAsync(prompt, cancellationToken);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("sentence", out var s)
                ? s.GetString() ?? "Hôm nay tôi rất vui."
                : "Hôm nay tôi rất vui.";
        }
        private async Task<string> CallGeminiAsync(string prompt, CancellationToken cancellationToken)
            {
                var url = $"{_baseUrl.TrimEnd('/')}/{_model}:generateContent?key={_apiKey}";

                var body = new
                {
                    contents = new[]
                    {
                    new { parts = new[] { new { text = prompt } } }
                },
                    generationConfig = new
                    {
                        responseMimeType = "application/json",
                        temperature = 0.3
                    }
                };

                using var content = new StringContent(
                    JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                var response = await _http.PostAsync(url, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var respBody = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(respBody);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "{}";
            }
        }
    }
