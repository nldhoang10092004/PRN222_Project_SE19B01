using CoreLibrary.Authentication;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Areas.Learner.Models;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class PlacementTestController : Controller
    {
        private readonly AppDbContext _db;
        private const int PASS_THRESHOLD_PER_BAND = 8;

        public PlacementTestController(AppDbContext db) => _db = db;

        // GET: /learn/PlacementTest
        public async Task<IActionResult> Index()
        {
            var test = await _db.PlacementTests
                .Include(t => t.Questions).ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(t => t.IsActive);

            if (test == null)
                return Content("Chưa có bài test trình độ nào được kích hoạt.");

            // Serialize câu hỏi xuống client (không bao gồm IsCorrect để tránh lộ đáp án)
            var questions = test.Questions
                .OrderBy(q => q.SortOrder)
                .Select(q => new
                {
                    questionId = q.QuestionId,
                    text = q.QuestionText,
                    options = q.AnswerOptions
                        .OrderBy(o => o.OptionId)
                        .Select(o => new { optionId = o.OptionId, text = o.AnswerText })
                        .ToList()
                })
                .ToList();

            ViewBag.QuestionsJson = System.Text.Json.JsonSerializer.Serialize(questions);
            ViewBag.Duration = test.Duration;
            ViewBag.TestId = test.TestId;

            return View();
        }

        // POST: /learn/PlacementTest/SaveResult
        [HttpPost]
        public async Task<IActionResult> SaveResult([FromBody] PlacementSubmitRequest req)
        {
            var test = await _db.PlacementTests
                .Include(t => t.Questions).ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(t => t.TestId == req.TestId && t.IsActive);
            if (test == null) return NotFound();

            var correctAnswers = test.Questions
                .ToDictionary(q => q.QuestionId, q => q.AnswerOptions.First(o => o.IsCorrect).OptionId);

            int correctCount = req.Answers.Count(a => correctAnswers.TryGetValue(a.QuestionId, out var oid) && oid == a.SelectedOptionId);
            int totalPoints = test.Questions.Sum(q => q.Points);

            var recommendedLevelId = MapToLevel(test, correctCount, req.Answers);

            await SaveResultIfLoggedIn(test.TestId, correctCount, totalPoints, recommendedLevelId);

            var recommendedLevel = await _db.JlptLevels.FindAsync(recommendedLevelId);

            return Ok(new
            {
                correctCount,
                totalPoints,
                recommendedLevelId,
                recommendedLevelName = recommendedLevel?.LevelName ?? "N5"
            });
        }

        // ===== Helpers =====

        private int MapToLevel(PlacementTest test, int correctCount, List<PlacementAnswerDto> answers)
        {
            var orderedQs = test.Questions.OrderBy(q => q.SortOrder).ToList();
            var answersByQ = answers.ToDictionary(a => a.QuestionId, a => a.SelectedOptionId);

            int CountCorrectInBand(int startIdx, int endIdx)
            {
                var band = orderedQs.Skip(startIdx).Take(endIdx - startIdx);
                return band.Count(q => answersByQ.TryGetValue(q.QuestionId, out var oid)
                                       && q.AnswerOptions.Any(o => o.OptionId == oid && o.IsCorrect));
            }

            var correctN5 = CountCorrectInBand(0, 10);
            var correctN4 = CountCorrectInBand(10, 20);
            var correctN3 = CountCorrectInBand(20, 30);
            var correctN2 = CountCorrectInBand(30, 40);

            if (correctN2 >= PASS_THRESHOLD_PER_BAND) return GetLevelId("N2");
            if (correctN3 >= PASS_THRESHOLD_PER_BAND && correctN2 < PASS_THRESHOLD_PER_BAND) return GetLevelId("N3");
            if (correctN4 >= PASS_THRESHOLD_PER_BAND && correctN3 < PASS_THRESHOLD_PER_BAND) return GetLevelId("N4");
            return GetLevelId("N5");
        }

        private int GetLevelId(string levelName)
        {
            return _db.JlptLevels.First(l => l.LevelName == levelName).LevelId;
        }

        private async Task SaveResultIfLoggedIn(int testId, int score, int totalPoints, int recommendedLevelId)
        {
            var currentUser = HttpContext.Session.GetObject<CurrentUser>(IAuthenticationService.SessionKeyCurrentUser);
            if (currentUser == null) return;

            var student = await _db.Students
                .FirstOrDefaultAsync(s => s.StudentNavigation.AccountId == currentUser.AccountId);
            if (student == null) return;

            _db.StudentPlacementResults.Add(new StudentPlacementResult
            {
                StudentId = student.StudentId,
                TestId = testId,
                Score = score,
                TotalPoints = totalPoints,
                RecommendedLevelId = recommendedLevelId,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
    }

    public class PlacementSubmitRequest
    {
        public int TestId { get; set; }
        public List<PlacementAnswerDto> Answers { get; set; } = new();
    }

    public class PlacementAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }
}