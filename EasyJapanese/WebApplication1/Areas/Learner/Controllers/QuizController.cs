using CoreLibrary.Authentication;
using CoreLibrary.Const;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class QuizController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IAuthenticationService _auth;

        public QuizController(AppDbContext db, IAuthenticationService auth)
        {
            _db = db;
            _auth = auth;
        }

        // GET: /learn/Quiz/Start?courseId=1
        [HttpGet]
        public async Task<IActionResult> Start(int courseId)
        {
            var course = await _db.Courses
                .Include(c => c.Level)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null) return NotFound();

            // Check access
            if (!course.IsFree && !await HasAccessAsync())
            {
                TempData["LockedMessage"] = "Bạn cần đăng ký Membership để làm quiz này.";
                return RedirectToAction("Index", "Membership");
            }

            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.CourseId == courseId);

            if (quiz == null)
            {
                TempData["ErrorMessage"] = "Khóa học này chưa có quiz cuối khóa.";
                return RedirectToAction("Detail", "Course", new { id = courseId });
            }

            ViewBag.CourseName = EncodingFixer.FixMojibake(course.Title);
            ViewBag.LevelName = course.Level?.LevelName ?? "";
            ViewBag.QuizTitle = EncodingFixer.FixMojibake(quiz.Title);
            ViewBag.Duration = quiz.Duration ?? 45;
            ViewBag.QuestionCount = quiz.Questions.Count;
            ViewBag.PassScore = quiz.PassScore;
            ViewBag.QuizId = quiz.QuizId;

            return View();
        }

        // GET: /learn/Quiz/Take?quizId=1
        [HttpGet]
        public async Task<IActionResult> Take(int quizId)
        {
            var quiz = await _db.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            if (quiz == null) return NotFound();

            // Check access
            if (quiz.Course != null && !quiz.Course.IsFree && !await HasAccessAsync())
            {
                return Forbid();
            }

            var questions = quiz.Questions
                .OrderBy(q => q.SortOrder)
                .Select(q => new
                {
                    questionId = q.QuestionId,
                    text = EncodingFixer.FixMojibake(q.QuestionText),
                    points = q.Points,
                    difficultyLevel = q.DifficultyLevel,
                    options = q.AnswerOptions
                        .Select(o => new
                        {
                            optionId = o.OptionId,
                            text = EncodingFixer.FixMojibake(o.AnswerText)
                        })
                        .ToList()
                })
                .ToList();

            ViewBag.QuestionsJson = System.Text.Json.JsonSerializer.Serialize(questions);
            ViewBag.QuizId = quizId;
            ViewBag.QuizTitle = quiz.Title;
            ViewBag.Duration = quiz.Duration ?? 45;

            return View();
        }

        // POST: /learn/Quiz/Submit
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] QuizSubmitRequest request)
        {
            var quiz = await _db.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuizId == request.QuizId);

            if (quiz == null) return NotFound();

            // Check access
            if (quiz.Course != null && !quiz.Course.IsFree && !await HasAccessAsync())
                return Forbid();

            var correctAnswers = quiz.Questions
                .ToDictionary(
                    q => q.QuestionId,
                    q => q.AnswerOptions.FirstOrDefault(o => o.IsCorrect)?.OptionId ?? 0
                );

            int score = 0;
            int totalPoints = quiz.Questions.Sum(q => q.Points);

            foreach (var ans in request.Answers)
            {
                if (correctAnswers.TryGetValue(ans.QuestionId, out var correctOptionId)
                    && correctOptionId == ans.SelectedOptionId)
                {
                    var question = quiz.Questions.First(q => q.QuestionId == ans.QuestionId);
                    score += question.Points;
                }
            }

            bool isPassed = totalPoints > 0 && (score * 100.0 / totalPoints) >= quiz.PassScore;

            // Save attempt
            await SaveAttemptAsync(quiz.QuizId, score, totalPoints, isPassed, request.Answers);

            return Ok(new
            {
                score,
                totalPoints,
                isPassed,
                passScore = quiz.PassScore,
                courseId = quiz.CourseId
            });
        }

        private async Task SaveAttemptAsync(
            int quizId,
            int score,
            int totalPoints,
            bool isPassed,
            List<QuizAnswerDto> answers)
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (!studentId.HasValue) return;

            var selectedAnswersJson = System.Text.Json.JsonSerializer.Serialize(answers);

            _db.QuizAttempts.Add(new QuizAttempt
            {
                StudentId = studentId.Value,
                QuizId = quizId,
                Score = score,
                TotalPoints = totalPoints,
                IsPassed = isPassed,
                SelectedAnswers = selectedAnswersJson,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

        private async Task<int?> GetCurrentStudentIdAsync()
        {
            var user = await _auth.GetCurrentUserAsync(HttpContext);
            if (user == null || user.Role != RoleConst.STUDENT)
                return null;
            return user.AccountId;
        }

        private async Task<bool> HasAccessAsync()
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (!studentId.HasValue) return false;

            return await _db.StudentMemberships
                .AnyAsync(m => m.StudentId == studentId.Value
                            && m.IsActive
                            && m.EndDate > DateTime.UtcNow);
        }
    }

    public class QuizSubmitRequest
    {
        public int QuizId { get; set; }
        public List<QuizAnswerDto> Answers { get; set; } = new();
    }

    public class QuizAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }
}
