using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Learner.Models
{
    public class WritingIndexViewModel
    {
        public string EssayLevel { get; set; } = "";
        public string EssayTopic { get; set; } = "";
        public string TranslationLevel { get; set; } = "";
        public string TranslationSentence { get; set; } = "";
    }

    public class EssayGradeRequest
    {
        public string Topic { get; set; } = "";
        public string Level { get; set; } = "";
        public string StudentText { get; set; } = "";
    }

    public class TranslationGradeRequest
    {
        public string VietnameseSentence { get; set; } = "";
        public string Level { get; set; } = "";
        public string StudentText { get; set; } = "";
    }
}
