using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using CoreLibrary.Utility;
using OfficeOpenXml;

namespace WebApplication1.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.MENTOR)]
    [Route("teacher/flashcards")]
    public class FlashcardsController : Controller
    {
        private readonly AppDbContext _context;

        static FlashcardsController()
        {
            ExcelPackage.License.SetNonCommercialPersonal("EasyJapanese");
        }

        public FlashcardsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? courseId)
        {
            ViewData["Title"] = "Quản lý Flashcards";
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            // Load courses taught by this mentor
            var courses = await _context.Courses
                .Where(c => c.CreatedBy == mentorId)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;

            // Query flashcards
            var query = _context.Flashcards
                .Include(f => f.Course)
                .Where(f => f.Course.CreatedBy == mentorId);

            if (courseId.HasValue)
            {
                query = query.Where(f => f.CourseId == courseId.Value);
            }

            var flashcards = await query
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(flashcards);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Flashcard model)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            // Verify course belongs to this mentor
            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == model.CourseId && c.CreatedBy == mentorId);
            if (!courseExists) return Forbid();

            // Find a default student to satisfy foreign key
            var defaultStudent = await _context.Students.FirstOrDefaultAsync();
            if (defaultStudent == null)
            {
                TempData["ErrorMessage"] = "Không thể tạo Flashcard vì hệ thống chưa có Học viên nào đăng ký để liên kết.";
                return RedirectToAction(nameof(Index));
            }

            model.StudentId = defaultStudent.StudentId;
            model.Efactor = 2.5m;
            model.ReviewCount = 0;
            model.CreatedAt = DateTime.UtcNow;

            _context.Flashcards.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo Flashcard mới thành công.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit(Flashcard model)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var flashcard = await _context.Flashcards
                .Include(f => f.Course)
                .FirstOrDefaultAsync(f => f.FlashcardId == model.FlashcardId && f.Course.CreatedBy == mentorId);

            if (flashcard == null) return NotFound();

            flashcard.FrontText = model.FrontText;
            flashcard.BackText = model.BackText;
            flashcard.CourseId = model.CourseId;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật Flashcard thành công.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int flashcardId)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var flashcard = await _context.Flashcards
                .Include(f => f.Course)
                .FirstOrDefaultAsync(f => f.FlashcardId == flashcardId && f.Course.CreatedBy == mentorId);

            if (flashcard == null) return NotFound();

            var courseId = flashcard.CourseId;
            _context.Flashcards.Remove(flashcard);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa Flashcard thành công.";
            return RedirectToAction(nameof(Index), new { courseId });
        }

        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel(int courseId, IFormFile excelFile)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            // Verify course belongs to this mentor
            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == courseId && c.CreatedBy == mentorId);
            if (!courseExists) return Forbid();

            // Find a default student to satisfy foreign key
            var defaultStudent = await _context.Students.FirstOrDefaultAsync();
            if (defaultStudent == null)
            {
                TempData["ErrorMessage"] = "Không thể nhập Flashcard vì hệ thống chưa có Học viên nào đăng ký để liên kết.";
                return RedirectToAction(nameof(Index), new { courseId });
            }

            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một file Excel hợp lệ.";
                return RedirectToAction(nameof(Index), new { courseId });
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            TempData["ErrorMessage"] = "File Excel trống hoặc không có Worksheet.";
                            return RedirectToAction(nameof(Index), new { courseId });
                        }

                        var rowCount = worksheet.Dimension.Rows;
                        int importedCount = 0;

                        // Start from row 2 (skipping header)
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var front = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                            var back = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

                            if (string.IsNullOrEmpty(front) || string.IsNullOrEmpty(back)) continue;

                            var flashcard = new Flashcard
                            {
                                CourseId = courseId,
                                StudentId = defaultStudent.StudentId,
                                FrontText = front,
                                BackText = back,
                                Efactor = 2.5m,
                                ReviewCount = 0,
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.Flashcards.Add(flashcard);
                            importedCount++;
                        }

                        if (importedCount > 0)
                        {
                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = $"Đã nhập thành công {importedCount} thẻ Flashcards từ file Excel.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Không tìm thấy dữ liệu hợp lệ trong file Excel.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xử lý file Excel: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { courseId });
        }
    }
}
