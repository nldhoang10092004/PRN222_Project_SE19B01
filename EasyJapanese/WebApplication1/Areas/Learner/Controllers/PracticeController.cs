using CoreLibrary.Authentication;
using CoreLibrary.Const;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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
        public IActionResult Reading() => View("ComingSoon", new ComingSoonViewModel
        {
            FeatureName = "Luyện Đọc",
            Description = "Bài đọc hiểu đa dạng chủ đề, từ vựn Hán tự và ngữ pháp đi kèm."
        });
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