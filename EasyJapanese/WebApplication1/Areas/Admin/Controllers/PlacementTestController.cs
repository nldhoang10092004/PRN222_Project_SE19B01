using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    [Route("admin/placement-tests")]
    public class PlacementTestController : Controller
    {
        private readonly AppDbContext _context;

        public PlacementTestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý Bài test Đầu vào";

            var tests = await _context.PlacementTests
                .Include(t => t.Questions)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tests);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Tạo Bài test mới";
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlacementTest model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Tạo Bài test mới";
                return View(model);
            }

            var adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }

            model.CreatedBy = adminId.Value;
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            _context.PlacementTests.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã tạo bài test thành công.";
            return RedirectToAction(nameof(ManageQuestions), new { id = model.TestId });
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var test = await _context.PlacementTests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Chỉnh sửa Bài test";
            return View(test);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PlacementTest model)
        {
            if (id != model.TestId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Chỉnh sửa Bài test";
                return View(model);
            }

            var test = await _context.PlacementTests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }

            test.Title = model.Title;
            test.Description = model.Description;
            test.Duration = model.Duration;
            test.PassScore = model.PassScore;
            test.IsActive = model.IsActive;
            test.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật bài test thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var test = await _context.PlacementTests
                .Include(t => t.StudentPlacementResults)
                .FirstOrDefaultAsync(t => t.TestId == id);

            if (test == null)
            {
                return NotFound();
            }

            if (test.StudentPlacementResults.Any())
            {
                TempData["ErrorMessage"] = "Không thể xóa bài test đã có học viên tham gia. Vui lòng đặt trạng thái Inactive thay vì xóa.";
                return RedirectToAction(nameof(Index));
            }

            _context.PlacementTests.Remove(test);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa bài test thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("manage-questions/{id}")]
        public async Task<IActionResult> ManageQuestions(int id)
        {
            var test = await _context.PlacementTests
                .Include(t => t.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(t => t.TestId == id);

            if (test == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Quản lý Câu hỏi - {test.Title}";
            ViewData["TestId"] = id;
            ViewData["TestTitle"] = test.Title;

            var questions = test.Questions.OrderBy(q => q.SortOrder).ToList();
            return View(questions);
        }
    }
}
