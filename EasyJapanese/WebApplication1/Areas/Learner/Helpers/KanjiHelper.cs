using CoreLibrary.Data.Entities;

namespace WebApplication1.Areas.Learner.Helpers
{
    public static class KanjiHelper
    {
        // Nguồn: https://github.com/mistval/kanji_images — GIF thứ tự nét viết,
        // đặt tên file theo mã Unicode hex của ký tự (VD: 私 -> U+79C1 -> "79c1.gif").
        private const string StrokeOrderCdnBase = "https://cdn.jsdelivr.net/gh/mistval/kanji_images/gifs/";

        public static string GetStrokeOrderUrl(KanjiEntry kanji)
        {
            if (!string.IsNullOrWhiteSpace(kanji.StrokeOrderUrl))
                return kanji.StrokeOrderUrl;

            if (string.IsNullOrEmpty(kanji.Character))
                return string.Empty;

            var codepoint = char.ConvertToUtf32(kanji.Character, 0);
            return StrokeOrderCdnBase + codepoint.ToString("x4") + ".gif";
        }
    }
}
