using CoreLibrary.Authentication;
using CoreLibrary.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
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
        private readonly IWebHostEnvironment _env;

        public ProfileController(AppDbContext db, IAuthenticationService auth, IWebHostEnvironment env)
        {
            _db = db;
            _auth = auth;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null) return RedirectToAction("Index", "Login", new { area = "" });

            var studentId = currentUser.AccountId;
            var student = await _db.Students
                .Include(s => s.StudentNavigation)
                .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);

            if (student == null) return RedirectToAction("Index", "Login", new { area = "" });

            var placement = await _db.StudentPlacementResults
                .Include(r => r.RecommendedLevel)
                .Where(r => r.StudentId == studentId && r.CompletedAt != null)
                .OrderByDescending(r => r.CompletedAt)
                .FirstOrDefaultAsync(cancellationToken);

            ViewBag.JlptLevel = placement?.RecommendedLevel?.LevelName ?? "N5";

            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string fullName, string phoneNumber, IFormFile avatarFile, CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null) return RedirectToAction("Index", "Login", new { area = "" });

            var studentId = currentUser.AccountId;
            var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);
            if (student == null) return NotFound();

            student.FullName = fullName;
            student.PhoneNumber = phoneNumber;

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(avatarFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream, cancellationToken);
                }

                student.AvatarUrl = "/images/avatars/" + uniqueFileName;
            }

            await _db.SaveChangesAsync(cancellationToken);

            return RedirectToAction("Index");
        }
    }
}
