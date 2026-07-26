using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CoreLibrary.Utility;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    [Route("admin/exams")]
    public class ExamsController : Controller
    {
        private readonly AppDbContext _context;

        public ExamsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý Bài thi";
            var tests = await _context.PlacementTests
                .Include(t => t.Questions)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return View(tests);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(string title, string description, int duration, int passScore)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var adminId = user?.AccountId ?? 0;

            // Make sure admin exists in Admins table
            var adminExists = await _context.Admins.AnyAsync(a => a.AdminId == adminId);
            if (!adminExists)
            {
                // Create a basic Admin entry if it doesn't exist to satisfy foreign key
                var newAdmin = new CoreLibrary.Data.Entities.Admin
                {
                    AdminId = adminId,
                    FullName = user?.FullName ?? "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Admins.Add(newAdmin);
                await _context.SaveChangesAsync();
            }

            var test = new PlacementTest
            {
                Title = title,
                Description = description,
                Duration = duration,
                PassScore = passScore,
                IsActive = true,
                CreatedBy = adminId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PlacementTests.Add(test);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo đề thi mới thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit(int testId, string title, string description, int duration, int passScore)
        {
            var test = await _context.PlacementTests.FindAsync(testId);
            if (test == null) return NotFound();

            test.Title = title;
            test.Description = description;
            test.Duration = duration;
            test.PassScore = passScore;
            test.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật đề thi thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("toggle-status")]
        public async Task<IActionResult> ToggleStatus(int testId)
        {
            var test = await _context.PlacementTests.FindAsync(testId);
            if (test == null) return NotFound();

            test.IsActive = !test.IsActive;
            test.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = test.IsActive ? "Đã kích hoạt đề thi." : "Đã hủy kích hoạt đề thi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int testId)
        {
            var test = await _context.PlacementTests
                .Include(t => t.Questions)
                .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(t => t.TestId == testId);

            if (test == null) return NotFound();

            _context.PlacementTests.Remove(test);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa đề thi thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("questions/{testId}")]
        public async Task<IActionResult> Questions(int testId)
        {
            var test = await _context.PlacementTests
                .Include(t => t.Questions)
                .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(t => t.TestId == testId);

            if (test == null) return NotFound();

            ViewData["Title"] = $"Câu hỏi: {test.Title}";
            return View(test);
        }

        [HttpPost("questions/{testId}/create")]
        public async Task<IActionResult> CreateQuestion(int testId, string questionText, int points, List<string> options, int correctIndex)
        {
            var testExists = await _context.PlacementTests.AnyAsync(t => t.TestId == testId);
            if (!testExists) return NotFound();

            var question = new Question
            {
                TestId = testId,
                QuestionText = questionText,
                QuestionType = "MultipleChoice",
                Points = points,
                SortOrder = 0
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            for (int i = 0; i < options.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(options[i])) continue;
                var opt = new AnswerOption
                {
                    QuestionId = question.QuestionId,
                    AnswerText = options[i],
                    IsCorrect = (i == correctIndex)
                };
                _context.AnswerOptions.Add(opt);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đêm câu hỏi mới thành công.";
            return RedirectToAction(nameof(Questions), new { testId });
        }

        [HttpPost("questions/{testId}/edit")]
        public async Task<IActionResult> EditQuestion(int testId, int questionId, string questionText, int points, List<string> options, int correctIndex)
        {
            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.TestId == testId);

            if (question == null) return NotFound();

            question.QuestionText = questionText;
            question.Points = points;

            _context.AnswerOptions.RemoveRange(question.AnswerOptions);

            for (int i = 0; i < options.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(options[i])) continue;
                var opt = new AnswerOption
                {
                    QuestionId = question.QuestionId,
                    AnswerText = options[i],
                    IsCorrect = (i == correctIndex)
                };
                _context.AnswerOptions.Add(opt);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật câu hỏi thành công.";
            return RedirectToAction(nameof(Questions), new { testId });
        }

        [HttpPost("questions/{testId}/delete")]
        public async Task<IActionResult> DeleteQuestion(int testId, int questionId)
        {
            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.TestId == testId);

            if (question == null) return NotFound();

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa câu hỏi thành công.";
            return RedirectToAction(nameof(Questions), new { testId });
        }
    }
}
