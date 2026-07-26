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
    public class FlashcardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IAuthenticationService _auth;

        public FlashcardController(AppDbContext db, IAuthenticationService auth)
        {
            _db = db;
            _auth = auth;
        }

        private async Task<int?> GetCurrentStudentIdAsync()
        {
            var user = await _auth.GetCurrentUserAsync(HttpContext);
            return user?.AccountId;
        }

        // GET: /learn/Flashcard  → danh sách Set chung (mentor tạo, không gắn CourseId)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (studentId == null)
                return RedirectToAction("Index", "Login", new { area = "" });

            var sets = await _db.FlashcardSets
                .Where(s => s.CourseId == null)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var setIds = sets.Select(s => s.FlashcardSetId).ToList();

            // Số thẻ mẫu (template) trong mỗi set
            var templateCounts = await _db.Flashcards
                .Where(f => f.StudentId == null && f.FlashcardSetId.HasValue && setIds.Contains(f.FlashcardSetId.Value))
                .GroupBy(f => f.FlashcardSetId!.Value)
                .Select(g => new { SetId = g.Key, Total = g.Count() })
                .ToListAsync();
            var templateMap = templateCounts.ToDictionary(t => t.SetId, t => t.Total);

            // Tiến độ của học viên hiện tại trong mỗi set (nếu đã bắt đầu)
            var studentCounts = await _db.Flashcards
                .Where(f => f.StudentId == studentId.Value && f.FlashcardSetId.HasValue && setIds.Contains(f.FlashcardSetId.Value))
                .GroupBy(f => f.FlashcardSetId!.Value)
                .Select(g => new
                {
                    SetId = g.Key,
                    Due = g.Count(f => f.NextReviewAt == null || f.NextReviewAt <= DateTime.UtcNow)
                })
                .ToListAsync();
            var studentMap = studentCounts.ToDictionary(s => s.SetId, s => s.Due);

            var vm = sets.Select(s => new FlashcardSetListItemViewModel
            {
                FlashcardSetId = s.FlashcardSetId,
                Title = s.Title,
                Description = s.Description,
                ImageUrl = s.ImageUrl,
                TotalCount = templateMap.TryGetValue(s.FlashcardSetId, out var t) ? t : 0,
                DueCount = studentMap.TryGetValue(s.FlashcardSetId, out var d) ? d : 0,
                Started = studentMap.ContainsKey(s.FlashcardSetId)
            }).ToList();

            return View(vm);
        }

        // GET: /learn/Flashcard/Set/5  → xem trước nội dung set trước khi học
        [HttpGet("learn/Flashcard/Set/{setId:int}")]
        public async Task<IActionResult> Set(int setId)
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (studentId == null)
                return RedirectToAction("Index", "Login", new { area = "" });

            var set = await _db.FlashcardSets
                .FirstOrDefaultAsync(s => s.FlashcardSetId == setId && s.CourseId == null);
            if (set == null) return NotFound();

            var templates = await _db.Flashcards
                .Where(f => f.FlashcardSetId == setId && f.StudentId == null)
                .OrderBy(f => f.CreatedAt)
                .Select(f => new FlashcardPreviewItem { FrontText = f.FrontText, BackText = f.BackText })
                .ToListAsync();

            var vm = new FlashcardSetDetailViewModel
            {
                FlashcardSetId = set.FlashcardSetId,
                Title = set.Title,
                Description = set.Description,
                PreviewCards = templates,
                TotalCount = templates.Count
            };
            return View(vm);
        }

        // GET: /learn/Flashcard/Review/5 → tự động nhân bản thẻ cho học viên nếu chưa có, rồi vào trang ôn tập
        [HttpGet("learn/Flashcard/Review/{setId:int}")]
        public async Task<IActionResult> Review(int setId)
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (studentId == null)
                return RedirectToAction("Index", "Login", new { area = "" });

            var set = await _db.FlashcardSets
                .FirstOrDefaultAsync(s => s.FlashcardSetId == setId && s.CourseId == null);
            if (set == null) return NotFound();

            await EnsureStudentCardsClonedAsync(setId, studentId.Value);

            var dueCount = await _db.Flashcards
                .CountAsync(f => f.StudentId == studentId.Value && f.FlashcardSetId == setId
                              && (f.NextReviewAt == null || f.NextReviewAt <= DateTime.UtcNow));
            var totalCount = await _db.Flashcards
                .CountAsync(f => f.StudentId == studentId.Value && f.FlashcardSetId == setId);

            return View(new FlashcardReviewPageViewModel
            {
                FlashcardSetId = setId,
                SetTitle = set.Title,
                DueCount = dueCount,
                TotalCount = totalCount
            });
        }

        // GET: /learn/Flashcard/GetDueCards?setId=5
        [HttpGet]
        public async Task<IActionResult> GetDueCards(int setId, int take = 20)
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (studentId == null) return Unauthorized();

            var cards = await _db.Flashcards
                .Where(f => f.StudentId == studentId.Value && f.FlashcardSetId == setId
                         && (f.NextReviewAt == null || f.NextReviewAt <= DateTime.UtcNow))
                .OrderBy(f => f.NextReviewAt)
                .Take(take)
                .Select(f => new FlashcardCardViewModel
                {
                    FlashcardId = f.FlashcardId,
                    FrontText = f.FrontText,
                    BackText = f.BackText,
                    ImageUrl = f.ImageUrl
                })
                .ToListAsync();

            return Json(new { cards });
        }

        // POST: /learn/Flashcard/SubmitAnswer
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer([FromBody] FlashcardAnswerRequest req)
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (studentId == null) return Unauthorized();

            var card = await _db.Flashcards
                .FirstOrDefaultAsync(f => f.FlashcardId == req.FlashcardId && f.StudentId == studentId.Value);
            if (card == null) return NotFound();

            var (newEf, newCount, nextReview) = Sm2Calculator.Calculate(card.Efactor, card.ReviewCount, req.Quality);
            card.Efactor = newEf;
            card.ReviewCount = newCount;
            card.NextReviewAt = nextReview;

            await _db.SaveChangesAsync();

            var remainingDue = await _db.Flashcards
                .CountAsync(f => f.StudentId == studentId.Value && f.FlashcardSetId == card.FlashcardSetId
                              && (f.NextReviewAt == null || f.NextReviewAt <= DateTime.UtcNow));

            return Ok(new { nextReviewAt = nextReview, remainingDue });
        }

        // Nhân bản thẻ mẫu (StudentId = null) thành thẻ riêng của học viên nếu chưa có.
        // Đối chiếu theo FrontText+BackText để không nhân bản trùng khi gọi lại nhiều lần.
        private async Task EnsureStudentCardsClonedAsync(int setId, int studentId)
        {
            var templates = await _db.Flashcards
                .Where(f => f.FlashcardSetId == setId && f.StudentId == null)
                .ToListAsync();
            if (templates.Count == 0) return;

            var existing = await _db.Flashcards
                .Where(f => f.FlashcardSetId == setId && f.StudentId == studentId)
                .Select(f => f.FrontText + "|" + f.BackText)
                .ToListAsync();
            var existingSet = existing.ToHashSet();

            var toClone = templates
                .Where(t => !existingSet.Contains(t.FrontText + "|" + t.BackText))
                .ToList();

            if (toClone.Count == 0) return;

            foreach (var t in toClone)
            {
                _db.Flashcards.Add(new Flashcard
                {
                    StudentId = studentId,
                    CourseId = null,
                    FlashcardSetId = setId,
                    FrontText = t.FrontText,
                    BackText = t.BackText,
                    ImageUrl = t.ImageUrl,
                    Efactor = 2.5m,
                    ReviewCount = 0,
                    NextReviewAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}