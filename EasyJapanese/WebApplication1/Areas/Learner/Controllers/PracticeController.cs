using CoreLibrary.Authentication;
using CoreLibrary.Const;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Areas.Learner.Models;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
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
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _auth.GetCurrentUserAsync(HttpContext);
            var studentId = user?.AccountId;

            var vm = new PracticeIndexViewModel();

            // Flashcard count due
            if (studentId.HasValue)
            {
                vm.FlashcardDueCount = await _db.Flashcards
                    .Where(f => f.StudentId == studentId.Value
                             && f.NextReviewAt <= System.DateTime.UtcNow)
                    .CountAsync();

                vm.FlashcardTotal = await _db.Flashcards
                    .Where(f => f.StudentId == studentId.Value)
                    .CountAsync();
            }

            return View(vm);
        }

        // GET: /learn/Practice/Listening
        [HttpGet]
        public IActionResult Listening() => View("ComingSoon", new ComingSoonViewModel
        {
            FeatureName = "Luyện Nghe",
            Description = "Kho bài nghe phong phú từ N5 → N1, kèm transcript và bài tập điền từ."
        });

        // GET: /learn/Practice/Speaking
        [HttpGet]
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
}