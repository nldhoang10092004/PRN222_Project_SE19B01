using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using Microsoft.EntityFrameworkCore;
using CoreLibrary.Const;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    [Route("admin/lesson-materials")]
    public class LessonMaterialController : Controller
    {
        private readonly AppDbContext _context;

        public LessonMaterialController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, int? lessonId)
        {
            ViewData["Title"] = "Quản lý Tài liệu Bài học";
            ViewData["SearchString"] = searchString;
            ViewData["LessonId"] = lessonId;

            var query = _context.LessonMaterials
                .Include(m => m.Lesson)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(m => m.Title.ToLower().Contains(searchString));
            }

            if (lessonId.HasValue)
            {
                query = query.Where(m => m.LessonId == lessonId.Value);
            }

            var materials = await query.OrderBy(m => m.SortOrder).ThenByDescending(m => m.CreatedAt).ToListAsync();
            return View(materials);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Thêm Tài liệu mới";
            ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync();
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int lessonId, string title, string url, string? fileType, int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin.";
                ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync();
                return View();
            }

            var material = new CoreLibrary.Data.Entities.LessonMaterial
            {
                LessonId = lessonId,
                Title = title,
                Url = url,
                FileType = fileType,
                SortOrder = sortOrder,
                CreatedAt = DateTime.UtcNow
            };

            _context.LessonMaterials.Add(material);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm tài liệu thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var material = await _context.LessonMaterials.FindAsync(id);
            if (material == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài liệu.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Chỉnh sửa Tài liệu";
            ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync();
            return View(material);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int lessonId, string title, string url, string? fileType, int sortOrder)
        {
            var material = await _context.LessonMaterials.FindAsync(id);
            if (material == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài liệu.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin.";
                ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Title).ToListAsync();
                return View(material);
            }

            material.LessonId = lessonId;
            material.Title = title;
            material.Url = url;
            material.FileType = fileType;
            material.SortOrder = sortOrder;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật tài liệu thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var material = await _context.LessonMaterials.FindAsync(id);
            if (material == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài liệu.";
                return RedirectToAction(nameof(Index));
            }

            _context.LessonMaterials.Remove(material);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa tài liệu thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
