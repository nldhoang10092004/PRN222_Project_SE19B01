using CoreWeb.Models.ChatBot;
using CoreWeb.Service.ChatBot;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreWeb.Service.ChatBot
{
    public class JishoDictionaryService : IJishoDictionaryService
    {
        private readonly HttpClient _http;
        private readonly JishoOptions _options;

        public JishoDictionaryService(HttpClient http, IOptions<JishoOptions> options)
        {
            _http = http;
            _options = options.Value;
        }

        public async Task<List<JishoWordResult>> SearchAsync(string keyword, CancellationToken ct = default)
        {
            var url = $"{_options.BaseUrl}?keyword={Uri.EscapeDataString(keyword)}";

            using var res = await _http.GetAsync(url, ct);
            if (!res.IsSuccessStatusCode) return new List<JishoWordResult>();

            var json = await res.Content.ReadAsStringAsync(ct);
            var parsed = JsonSerializer.Deserialize<JishoApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed?.Data == null) return new List<JishoWordResult>();

            return parsed.Data
                .Take(3) 
                .Select(d =>
                {
                    var jp = d.Japanese?.FirstOrDefault();
                    return new JishoWordResult
                    {
                        Word = jp?.Word ?? jp?.Reading ?? keyword,
                        Reading = jp?.Reading ?? "",
                        Meanings = d.Senses?
                            .SelectMany(s => s.EnglishDefinitions ?? new List<string>())
                            .Take(5)
                            .ToList() ?? new List<string>(),
                        PartsOfSpeech = d.Senses?
                            .SelectMany(s => s.PartsOfSpeech ?? new List<string>())
                            .Distinct()
                            .Take(3)
                            .ToList() ?? new List<string>()
                    };
                })
                .ToList();
        }

        // ── DTO khớp cấu trúc JSON của Jisho API ──
        private class JishoApiResponse
        {
            [JsonPropertyName("data")]
            public List<JishoDataItem>? Data { get; set; }
        }

        private class JishoDataItem
        {
            [JsonPropertyName("japanese")]
            public List<JishoJapanese>? Japanese { get; set; }

            [JsonPropertyName("senses")]
            public List<JishoSense>? Senses { get; set; }
        }

        private class JishoJapanese
        {
            [JsonPropertyName("word")]
            public string? Word { get; set; }

            [JsonPropertyName("reading")]
            public string? Reading { get; set; }
        }

        private class JishoSense
        {
            [JsonPropertyName("english_definitions")]
            public List<string>? EnglishDefinitions { get; set; }

            [JsonPropertyName("parts_of_speech")]
            public List<string>? PartsOfSpeech { get; set; }
        }
    }
}