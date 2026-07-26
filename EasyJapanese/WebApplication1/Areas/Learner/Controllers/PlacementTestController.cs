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
        private readonly IAuthenticationService _auth;
        private readonly AppDbContext _db;

        private const int PASS_THRESHOLD_PER_BAND = 8;

        public PlacementTestController(
            IAuthenticationService auth,
            AppDbContext db)
        {
            _auth = auth;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Start(CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            return View();
        }

        // GET: /Learner/PlacementTest/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var test = await _db.PlacementTests
                .Include(t => t.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(t => t.IsActive);

            if (test == null)
            {
                return Content("Chưa có bài test trình độ nào được kích hoạt.");
            }

            var questions = test.Questions
                .OrderBy(q => q.SortOrder)
                .Select(q => new
                {
                    questionId = q.QuestionId,
                    text = q.QuestionText,
                    options = q.AnswerOptions
                        .OrderBy(o => o.OptionId)
                        .Select(o => new
                        {
                            optionId = o.OptionId,
                            text = o.AnswerText
                        })
                        .ToList()
                })
                .ToList();

            ViewBag.QuestionsJson =
                System.Text.Json.JsonSerializer.Serialize(questions);

            ViewBag.Duration = test.Duration;
            ViewBag.TestId = test.TestId;

            return View();
        }

        // POST: /Learner/PlacementTest/SaveResult
        [HttpPost]
        public async Task<IActionResult> SaveResult(
            [FromBody] PlacementSubmitRequest req)
        {
            var test = await _db.PlacementTests
                .Include(t => t.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(t =>
                    t.TestId == req.TestId && t.IsActive);

            if (test == null)
            {
                return NotFound();
            }

            var correctAnswers = test.Questions
                .ToDictionary(
                    q => q.QuestionId,
                    q => q.AnswerOptions.First(o => o.IsCorrect).OptionId
                );

            int correctCount = req.Answers.Count(a =>
                correctAnswers.TryGetValue(a.QuestionId, out var correctOptionId)
                && correctOptionId == a.SelectedOptionId
            );

            int totalPoints = test.Questions.Sum(q => q.Points);

            int recommendedLevelId = MapToLevel(
                test,
                correctCount,
                req.Answers
            );

            await SaveResultIfLoggedIn(
                test.TestId,
                correctCount,
                totalPoints,
                recommendedLevelId
            );

            var recommendedLevel = await _db.JlptLevels.FindAsync(recommendedLevelId);

            return Ok(new
            {
                correctCount,
                totalPoints,
                recommendedLevelId,
                recommendedLevelName = recommendedLevel?.LevelName ?? "N5"
            });
        }

        private int MapToLevel(
            PlacementTest test,
            int correctCount,
            List<PlacementAnswerDto> answers)
        {
            var orderedQuestions = test.Questions
                .OrderBy(q => q.SortOrder)
                .ToList();

            var answersByQuestion = answers.ToDictionary(
                a => a.QuestionId,
                a => a.SelectedOptionId
            );

            int CountCorrectInBand(int startIndex, int endIndex)
            {
                var band = orderedQuestions
                    .Skip(startIndex)
                    .Take(endIndex - startIndex);

                return band.Count(q =>
                    answersByQuestion.TryGetValue(
                        q.QuestionId,
                        out var selectedOptionId
                    )
                    && q.AnswerOptions.Any(o =>
                        o.OptionId == selectedOptionId
                        && o.IsCorrect
                    )
                );
            }

            int correctN5 = CountCorrectInBand(0, 10);
            int correctN4 = CountCorrectInBand(10, 20);
            int correctN3 = CountCorrectInBand(20, 30);
            int correctN2 = CountCorrectInBand(30, 40);

            if (correctN2 >= PASS_THRESHOLD_PER_BAND)
                return GetLevelId("N2");

            if (correctN3 >= PASS_THRESHOLD_PER_BAND)
                return GetLevelId("N3");

            if (correctN4 >= PASS_THRESHOLD_PER_BAND)
                return GetLevelId("N4");

            return GetLevelId("N5");
        }

        private int GetLevelId(string levelName)
        {
            return _db.JlptLevels
                .First(l => l.LevelName == levelName)
                .LevelId;
        }

        private async Task<bool> SaveResultIfLoggedIn(
            int testId, int score, int totalPoints, int recommendedLevelId)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null) return false;

            var student = await _db.Students
                .FirstOrDefaultAsync(s => s.StudentNavigation.AccountId == currentUser.AccountId);
            if (student == null) return false;

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
            return true;
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
}