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
using CoreWeb.Areas.Learner.Models;
using WebApplication1.Areas.Learner.Models;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    [Route("learn/Practice")]
    public class PracticeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IAuthenticationService _auth;

        public PracticeController(AppDbContext db, IAuthenticationService auth)
        {
            _db = db;
            _auth = auth;
        }

        // GET: /learn/Practice
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = await _auth.GetCurrentUserAsync(HttpContext);
            var studentId = user?.AccountId;

            var vm = new PracticeIndexViewModel();

            if (studentId.HasValue)
            {
                vm.FlashcardDueCount = await _db.Flashcards
                    .Where(f => f.StudentId == studentId.Value
                             && f.NextReviewAt <= DateTime.UtcNow)
                    .CountAsync();

                vm.FlashcardTotal = await _db.Flashcards
                    .Where(f => f.StudentId == studentId.Value)
                    .CountAsync();
            }

            return View(vm);
        }

        // GET: /learn/Practice/Listening
        [HttpGet("Listening")]
        public async Task<IActionResult> Listening(string level = "ALL")
        {
            var user = await _auth.GetCurrentUserAsync(HttpContext);
            var studentId = user?.AccountId;

            var query = _db.Exercises
                .Include(e => e.Course)
                    .ThenInclude(c => c.Level)
                .Include(e => e.Lesson)
                .Include(e => e.Questions)
                .Where(e => e.ExerciseType == "Listening" && e.Course.IsPublished);

            if (!string.IsNullOrEmpty(level) && level != "ALL")
            {
                query = query.Where(e => e.Course.Level != null && e.Course.Level.LevelName == level);
            }

            var exercises = await query
                .OrderBy(e => e.Course.Level != null ? e.Course.Level.SortOrder : 99)
                .ThenBy(e => e.SortOrder)
                .ToListAsync();

            // Load best scores if logged in
            var bestScores = new Dictionary<int, int>();
            if (studentId.HasValue)
            {
                var results = await _db.StudentExerciseResults
                    .Where(r => r.StudentId == studentId.Value)
                    .GroupBy(r => r.ExerciseId)
                    .Select(g => new { ExerciseId = g.Key, MaxScore = g.Max(r => r.Score) })
                    .ToListAsync();

                bestScores = results.ToDictionary(r => r.ExerciseId, r => r.MaxScore);
            }

            var vm = new ListeningHubViewModel
            {
                SelectedLevel = level,
                TotalCount = exercises.Count,
                CompletedCount = bestScores.Count(kv => kv.Value >= 80),
                Exercises = exercises.Select(e => new ListeningExerciseCardViewModel
                {
                    ExerciseId = e.ExerciseId,
                    Title = EncodingFixer.FixMojibake(e.Title),
                    Content = EncodingFixer.FixMojibake(e.Content),
                    AudioUrl = e.AudioUrl,
                    CourseTitle = EncodingFixer.FixMojibake(e.Course?.Title),
                    LessonTitle = EncodingFixer.FixMojibake(e.Lesson?.Title),
                    LevelName = e.Course?.Level?.LevelName ?? "N5",
                    QuestionCount = e.Questions.Count,
                    BestScore = bestScores.ContainsKey(e.ExerciseId) ? bestScores[e.ExerciseId] : null
                }).ToList()
            };

            return View(vm);
        }

        // GET: /learn/Practice/ListeningDetail/{id}
        [HttpGet("ListeningDetail/{id}")]
        public async Task<IActionResult> ListeningDetail(int id)
        {
            var user = await _auth.GetCurrentUserAsync(HttpContext);
            var studentId = user?.AccountId;

            var exercise = await _db.Exercises
                .Include(e => e.Course)
                    .ThenInclude(c => c.Level)
                .Include(e => e.Lesson)
                .Include(e => e.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(e => e.ExerciseId == id && e.ExerciseType == "Listening");

            if (exercise == null) return NotFound();

            int? bestScore = null;
            if (studentId.HasValue)
            {
                bestScore = await _db.StudentExerciseResults
                    .Where(r => r.StudentId == studentId.Value && r.ExerciseId == id)
                    .MaxAsync(r => (int?)r.Score);
            }

            var vm = new ListeningDetailViewModel
            {
                ExerciseId = exercise.ExerciseId,
                Title = EncodingFixer.FixMojibake(exercise.Title),
                Content = EncodingFixer.FixMojibake(exercise.Content),
                AudioUrl = exercise.AudioUrl,
                CourseTitle = EncodingFixer.FixMojibake(exercise.Course?.Title),
                LessonTitle = EncodingFixer.FixMojibake(exercise.Lesson?.Title),
                LevelName = exercise.Course?.Level?.LevelName ?? "N5",
                BestScore = bestScore,
                Questions = exercise.Questions
                    .OrderBy(q => q.SortOrder)
                    .Select(q => new ListeningQuestionViewModel
                    {
                        QuestionId = q.QuestionId,
                        QuestionText = EncodingFixer.FixMojibake(q.QuestionText),
                        Options = q.AnswerOptions.Select(o => new ListeningOptionViewModel
                        {
                            OptionId = o.OptionId,
                            AnswerText = EncodingFixer.FixMojibake(o.AnswerText)
                        }).ToList()
                    }).ToList()
            };

            return View(vm);
        }

        // POST: /learn/Practice/SubmitListening
        [HttpPost("SubmitListening")]
        public async Task<IActionResult> SubmitListening([FromBody] ExerciseAnswerRequest request)
        {
            if (request == null || request.Answers == null || !request.Answers.Any())
            {
                return BadRequest("Dữ liệu nộp bài không hợp lệ.");
            }

            var questionIds = request.Answers.Select(a => a.QuestionId).ToList();
            var questions = await _db.Questions
                .Where(q => q.ExerciseId == request.ExerciseId && questionIds.Contains(q.QuestionId))
                .Include(q => q.AnswerOptions)
                .ToListAsync();

            int correctCount = 0;
            var resultList = new List<object>();

            foreach (var q in questions)
            {
                var selected = request.Answers.FirstOrDefault(a => a.QuestionId == q.QuestionId);
                var correctOption = q.AnswerOptions.FirstOrDefault(o => o.IsCorrect);
                bool isCorrect = selected != null && correctOption != null && selected.OptionId == correctOption.OptionId;

                if (isCorrect) correctCount++;

                resultList.Add(new
                {
                    questionId = q.QuestionId,
                    isCorrect = isCorrect,
                    correctOptionId = correctOption?.OptionId
                });
            }

            int totalQuestions = questions.Count;
            int scorePercent = totalQuestions > 0 ? (int)Math.Round((double)correctCount / totalQuestions * 100) : 0;

            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser?.AccountId != null)
            {
                var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == currentUser.AccountId);
                if (student != null)
                {
                    var attempt = new StudentExerciseResult
                    {
                        StudentId = student.StudentId,
                        ExerciseId = request.ExerciseId,
                        Score = scorePercent,
                        TotalQuestions = totalQuestions,
                        CorrectAnswers = correctCount,
                        SubmittedAt = DateTime.UtcNow
                    };

                    _db.StudentExerciseResults.Add(attempt);
                    await _db.SaveChangesAsync();
                }
            }

            return Json(new
            {
                answers = resultList,
                correctCount,
                totalQuestions,
                scorePercent
            });
        }

        // GET: /learn/Practice/Speaking
        [HttpGet("Speaking")]
        public IActionResult Speaking() => View("ComingSoon", new ComingSoonViewModel
        {
            FeatureName = "Luyện Nói",
            Description = "Luyện phát âm và hội thoại với AI. Tính năng đang được phát triển."
        });

        // GET: /learn/Practice/Reading
        [HttpGet]
        public async Task<IActionResult> Reading()
        {
            var exercises = await _db.Exercises
                .Include(e => e.Questions)
                .Where(e => e.ExerciseType == "Reading" && e.CourseId == null)
                .OrderBy(e => e.SortOrder)
                .ToListAsync();

            var vm = new ReadingListViewModel
            {
                Exercises = exercises.Select(e => new ReadingExerciseVm
                {
                    ExerciseId = e.ExerciseId,
                    Title = e.Title,
                    QuestionCount = e.Questions.Count
                }).ToList()
            };

            return View(vm);
        }

        // GET: /learn/Practice/ReadingDetail/{id}
        [HttpGet]
        public async Task<IActionResult> ReadingDetail(int id)
        {
            var exercise = await _db.Exercises
                .Include(e => e.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(e => e.ExerciseId == id && e.ExerciseType == "Reading" && e.CourseId == null);

            if (exercise == null)
            {
                return NotFound();
            }

            var vm = new ReadingDetailViewModel
            {
                ExerciseId = exercise.ExerciseId,
                Title = exercise.Title,
                Content = exercise.Content ?? "",
                Questions = exercise.Questions.OrderBy(q => q.SortOrder).Select(q => new ReadingQuestionVm
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    SortOrder = q.SortOrder,
                    AnswerOptions = q.AnswerOptions.Select(o => new ReadingOptionVm
                    {
                        OptionId = o.OptionId,
                        AnswerText = o.AnswerText
                    }).ToList()
                }).ToList()
            };

            return View(vm);
        }

        // POST: /learn/Practice/ReadingSubmit
        [HttpPost]
        public async Task<IActionResult> ReadingSubmit(int id, Dictionary<int, int> answers)
        {
            var exercise = await _db.Exercises
                .Include(e => e.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(e => e.ExerciseId == id);

            if (exercise == null)
            {
                return NotFound();
            }

            var results = new List<QuestionResultVm>();
            var correctCount = 0;

            foreach (var question in exercise.Questions.OrderBy(q => q.SortOrder))
            {
                var selectedOptionId = answers.ContainsKey(question.QuestionId) ? answers[question.QuestionId] : 0;
                var selectedOption = question.AnswerOptions.FirstOrDefault(o => o.OptionId == selectedOptionId);
                var correctOption = question.AnswerOptions.FirstOrDefault(o => o.IsCorrect);
                var isCorrect = selectedOption != null && selectedOption.IsCorrect;

                if (isCorrect) correctCount++;

                results.Add(new QuestionResultVm
                {
                    QuestionText = question.QuestionText,
                    SelectedAnswer = selectedOption?.AnswerText ?? "Chưa trả lời",
                    CorrectAnswer = correctOption?.AnswerText ?? "",
                    IsCorrect = isCorrect
                });
            }

            var resultVm = new ReadingResultViewModel
            {
                ExerciseId = exercise.ExerciseId,
                Title = exercise.Title,
                TotalQuestions = exercise.Questions.Count,
                CorrectCount = correctCount,
                ScorePercent = exercise.Questions.Count > 0 ? (decimal)correctCount / exercise.Questions.Count * 100 : 0,
                Results = results
            };

            TempData["ReadingResult"] = System.Text.Json.JsonSerializer.Serialize(resultVm);
            return RedirectToAction(nameof(ReadingResult));
        }

        // GET: /learn/Practice/ReadingResult
        [HttpGet]
        public IActionResult ReadingResult()
        {
            var resultJson = TempData["ReadingResult"] as string;
            if (string.IsNullOrEmpty(resultJson))
            {
                return RedirectToAction(nameof(Reading));
            }

            var vm = System.Text.Json.JsonSerializer.Deserialize<ReadingResultViewModel>(resultJson);
            return View(vm);
        }
    }

    public class PracticeIndexViewModel
    {
        public int FlashcardDueCount { get; set; }
        public int FlashcardTotal { get; set; }
    }

    public class ComingSoonViewModel
    {
        public string FeatureName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ListeningHubViewModel
    {
        public string SelectedLevel { get; set; } = "ALL";
        public int TotalCount { get; set; }
        public int CompletedCount { get; set; }
        public List<ListeningExerciseCardViewModel> Exercises { get; set; } = new();
    }

    public class ListeningExerciseCardViewModel
    {
        public int ExerciseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? AudioUrl { get; set; }
        public string? CourseTitle { get; set; }
        public string? LessonTitle { get; set; }
        public string LevelName { get; set; } = "N5";
        public int QuestionCount { get; set; }
        public int? BestScore { get; set; }
    }

    public class ListeningDetailViewModel
    {
        public int ExerciseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? AudioUrl { get; set; }
        public string? CourseTitle { get; set; }
        public string? LessonTitle { get; set; }
        public string LevelName { get; set; } = "N5";
        public int? BestScore { get; set; }
        public List<ListeningQuestionViewModel> Questions { get; set; } = new();
    }

    public class ListeningQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<ListeningOptionViewModel> Options { get; set; } = new();
    }

    public class ListeningOptionViewModel
    {
        public int OptionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
    }
}