using CoreWeb.Service.AI;
using WebApplication1.Areas.Learner.Models;
using CoreLibrary.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class WritingController : Controller
    {
        private readonly IWritingAiService _ai;
        private static readonly string[] Levels = { "N5", "N4", "N3" };

        public WritingController(IWritingAiService ai)
        {
            _ai = ai;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = new WritingIndexViewModel
            {
                EssayLevel = PickRandomLevel(),
                TranslationLevel = PickRandomLevel()
            };
            return View("~/Areas/Learner/Views/Practice/Writing.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> NextEssayTopic(CancellationToken cancellationToken)
        {
            var level = PickRandomLevel();
            try
            {
                var topic = await _ai.GenerateEssayTopicAsync(level, cancellationToken);
                return Json(new { level, topic });
            }
            catch
            {
                return StatusCode(502, new { message = "Không thể tạo đề bài lúc này, vui lòng thử lại." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> NextTranslationPrompt(CancellationToken cancellationToken)
        {
            var level = PickRandomLevel();
            try
            {
                var sentence = await _ai.GenerateTranslationSentenceAsync(level, cancellationToken);
                return Json(new { level, sentence });
            }
            catch
            {
                return StatusCode(502, new { message = "Không thể tạo câu dịch lúc này, vui lòng thử lại." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GradeEssay(
            [FromBody] EssayGradeRequest req, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(req.StudentText))
                return BadRequest(new { message = "Vui lòng nhập bài viết." });

            try
            {
                var result = await _ai.GradeEssayAsync(req.Topic, req.Level, req.StudentText, cancellationToken);
                return Ok(result);
            }
            catch
            {
                return StatusCode(502, new { message = "Không thể chấm bài lúc này, vui lòng thử lại." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GradeTranslation([FromBody] TranslationGradeRequest req, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(req.StudentText))
                return BadRequest(new { message = "Vui lòng nhập câu dịch." });

            try
            {
                var result = await _ai.GradeTranslationAsync(req.VietnameseSentence, req.Level, req.StudentText, cancellationToken);
                return Ok(result);
            }
            catch
            {
                return StatusCode(502, new { message = "Không thể chấm câu dịch lúc này, vui lòng thử lại." });
            }
        }

        private static string PickRandomLevel() => Levels[Random.Shared.Next(Levels.Length)];
    }

}