using CoreLibrary.Authentication;
using CoreLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IAuthenticationService _auth;

        public ProfileController(AppDbContext db, IAuthenticationService auth)
        {
            _db = db;
            _auth = auth;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var studentId = currentUser.AccountId;
            var student = await _db.Students
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);

            if (student == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            // Lấy thông tin Jlpt goal nếu có
            var placement = await _db.StudentPlacementResults
                .Include(r => r.RecommendedLevel)
                .Where(r => r.StudentId == studentId && r.CompletedAt != null)
                .OrderByDescending(r => r.CompletedAt)
                .FirstOrDefaultAsync(cancellationToken);

            ViewBag.JlptLevel = placement?.RecommendedLevel?.LevelName ?? "N5";

            return View(student);
        }
    }
}
