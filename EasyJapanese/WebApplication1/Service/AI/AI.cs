using Microsoft.AspNetCore.Mvc;

namespace CoreWeb.Service.AI
{
        public class EssayFeedbackResult
        {
            public int Score { get; set; } // 0-100
            public string OverallComment { get; set; } = "";
            public List<GrammarCorrection> Corrections { get; set; } = new();
            public List<string> Strengths { get; set; } = new();
            public List<string> Improvements { get; set; } = new();
        }

        public class GrammarCorrection
        {
            public string Original { get; set; } = "";
            public string Corrected { get; set; } = "";
            public string Explanation { get; set; } = "";
        }

        public class TranslationFeedbackResult
        {
            public bool IsCorrect { get; set; }
            public int Score { get; set; } // 0-100
            public string SuggestedTranslation { get; set; } = "";
            public string Explanation { get; set; } = "";
            public List<string> Notes { get; set; } = new();
        }
    
}
