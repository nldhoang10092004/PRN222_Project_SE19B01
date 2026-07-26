using Microsoft.AspNetCore.Mvc;

namespace CoreWeb.Service.AI
{
        public interface IWritingAiService
        {
            Task<EssayFeedbackResult> GradeEssayAsync(
                string topic, string levelHint, string studentText,
                CancellationToken cancellationToken = default);

            Task<TranslationFeedbackResult> GradeTranslationAsync(
                string vietnameseSentence, string levelHint, string studentText,
                CancellationToken cancellationToken = default);
        Task<string> GenerateEssayTopicAsync(string levelHint, CancellationToken cancellationToken = default);

        Task<string> GenerateTranslationSentenceAsync(string levelHint, CancellationToken cancellationToken = default);
    }
    
}
