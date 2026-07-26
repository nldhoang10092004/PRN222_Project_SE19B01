using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;
using Microsoft.EntityFrameworkCore;
using CoreLibrary.Utility;
using OfficeOpenXml;

namespace WebApplication1.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.MENTOR)]
    [Route("teacher/flashcard-sets")]
    public class FlashcardSetsController : Controller
    {
        private readonly AppDbContext _context;

        static FlashcardSetsController()
        {
            ExcelPackage.License.SetNonCommercialPersonal("EasyJapanese");
        }

        public FlashcardSetsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? courseId)
        {
            ViewData["Title"] = "Quản lý Bộ Flashcards";

            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(
                CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            // Load courses của mentor để filter
            var courses = await _context.Courses
                .Where(c => c.CreatedBy == mentorId)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;

            // Query flashcard sets
            var query = _context.FlashcardSets
                .Include(fs => fs.Course)
                .Include(fs => fs.Flashcards)
                .Where(fs => fs.CreatedBy == mentorId);

            if (courseId.HasValue)
            {
                query = query.Where(fs => fs.CourseId == courseId.Value);
            }

            var flashcardSets = await query
                .OrderByDescending(fs => fs.CreatedAt)
                .ToListAsync();

            return View(flashcardSets);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Tạo Bộ Flashcards Mới";

            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(
                CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            // Load courses để mentor chọn
            var courses = await _context.Courses
                .Where(c => c.CreatedBy == mentorId)
                .ToListAsync();

            ViewBag.Courses = courses;
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlashcardSet model)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(
                CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            // Validate course thuộc mentor
            if (model.CourseId.HasValue)
            {
                var courseExists = await _context.Courses
                    .AnyAsync(c => c.CourseId == model.CourseId.Value && c.CreatedBy == mentorId);

                if (!courseExists)
                {
                    TempData["ErrorMessage"] = "Khóa học không hợp lệ.";
                    return RedirectToAction(nameof(Create));
                }
            }

            model.CreatedBy = mentorId;
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            _context.FlashcardSets.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo bộ flashcards thành công.";
            return RedirectToAction(nameof(Details), new { id = model.FlashcardSetId });
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(
                CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var flashcardSet = await _context.FlashcardSets
                .Include(fs => fs.Course)
                .Include(fs => fs.Flashcards)
                .FirstOrDefaultAsync(fs => fs.FlashcardSetId == id && fs.CreatedBy == mentorId);

            if (flashcardSet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bộ flashcards.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = flashcardSet.Title;
            return View(flashcardSet);
        }

        [HttpPost("add-card")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCard(int flashcardSetId, string frontText, string backText)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(
                CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var flashcardSet = await _context.FlashcardSets
                .FirstOrDefaultAsync(fs => fs.FlashcardSetId == flashcardSetId && fs.CreatedBy == mentorId);

            if (flashcardSet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bộ flashcards.";
                return RedirectToAction(nameof(Index));
            }

            var flashcard = new Flashcard
            {
                FlashcardSetId = flashcardSetId,
                StudentId = null,
                CourseId = flashcardSet.CourseId,
                FrontText = frontText,
                BackText = backText,
                Efactor = 2.5m,
                ReviewCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Flashcards.Add(flashcard);
            flashcardSet.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm flashcard mới.";
            return RedirectToAction(nameof(Details), new { id = flashcardSetId });
        }

        [HttpPost("edit-card")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCard(int flashcardSetId, int flashcardId, string frontText, string backText)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(
                CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var flashcardSet = await _context.FlashcardSets
                .FirstOrDefaultAsync(fs => fs.FlashcardSetId == flashcardSetId && fs.CreatedBy == mentorId);

            if (flashcardSet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bộ flashcards.";
                return RedirectToAction(nameof(Index));
            }

            var flashcard = await _context.Flashcards
                .FirstOrDefaultAsync(f => f.FlashcardId == flashcardId && f.FlashcardSetId == flashcardSetId);

            if (flashcard == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy flashcard.";
                return RedirectToAction(nameof(Details), new { id = flashcardSetId });
            }

            flashcard.FrontText = frontText;
            flashcard.BackText = backText;
            flashcardSet.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật flashcard.";
            return RedirectToAction(nameof(Details), new { id = flashcardSetId });
        }

        [HttpPost("remove-card")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCard(int flashcardSetId, int flashcardId)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(
                CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var flashcardSet = await _context.FlashcardSets
                .FirstOrDefaultAsync(fs => fs.FlashcardSetId == flashcardSetId && fs.CreatedBy == mentorId);

            if (flashcardSet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bộ flashcards.";
                return RedirectToAction(nameof(Index));
            }

            var flashcard = await _context.Flashcards
                .FirstOrDefaultAsync(f => f.FlashcardId == flashcardId && f.FlashcardSetId == flashcardSetId);

            if (flashcard == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy flashcard.";
                return RedirectToAction(nameof(Details), new { id = flashcardSetId });
            }

            _context.Flashcards.Remove(flashcard);
            flashcardSet.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa flashcard.";
            return RedirectToAction(nameof(Details), new { id = flashcardSetId });
        }

        [HttpPost("import-excel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(int flashcardSetId, IFormFile excelFile)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(
                CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var flashcardSet = await _context.FlashcardSets
                .FirstOrDefaultAsync(fs => fs.FlashcardSetId == flashcardSetId && fs.CreatedBy == mentorId);

            if (flashcardSet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bộ flashcards.";
                return RedirectToAction(nameof(Index));
            }

            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file Excel hợp lệ.";
                return RedirectToAction(nameof(Details), new { id = flashcardSetId });
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
                            return RedirectToAction(nameof(Details), new { id = flashcardSetId });
                        }

                        var rowCount = worksheet.Dimension.Rows;
                        int importedCount = 0;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var front = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                            var back = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

                            if (string.IsNullOrEmpty(front) || string.IsNullOrEmpty(back)) continue;

                            var flashcard = new Flashcard
                            {
                                FlashcardSetId = flashcardSetId,
                                StudentId = null,
                                CourseId = flashcardSet.CourseId,
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
                            flashcardSet.UpdatedAt = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = $"Đã import thành công {importedCount} flashcard(s).";
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

            return RedirectToAction(nameof(Details), new { id = flashcardSetId });
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(
                CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var flashcardSet = await _context.FlashcardSets
                .Include(fs => fs.Course)
                .FirstOrDefaultAsync(fs => fs.FlashcardSetId == id && fs.CreatedBy == mentorId);

            if (flashcardSet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bộ flashcards.";
                return RedirectToAction(nameof(Index));
            }

            var courses = await _context.Courses
                .Where(c => c.CreatedBy == mentorId)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewData["Title"] = "Sửa Bộ Flashcards";
            return View(flashcardSet);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FlashcardSet model)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(
                CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var flashcardSet = await _context.FlashcardSets
                .FirstOrDefaultAsync(fs => fs.FlashcardSetId == id && fs.CreatedBy == mentorId);

            if (flashcardSet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bộ flashcards.";
                return RedirectToAction(nameof(Index));
            }

            if (model.CourseId.HasValue)
            {
                var courseExists = await _context.Courses
                    .AnyAsync(c => c.CourseId == model.CourseId.Value && c.CreatedBy == mentorId);

                if (!courseExists)
                {
                    TempData["ErrorMessage"] = "Khóa học không hợp lệ.";
                    return RedirectToAction(nameof(Edit), new { id });
                }
            }

            flashcardSet.Title = model.Title;
            flashcardSet.Description = model.Description;
            flashcardSet.ImageUrl = model.ImageUrl;
            flashcardSet.CourseId = model.CourseId;
            flashcardSet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật bộ flashcards.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(
                CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var flashcardSet = await _context.FlashcardSets
                .Include(fs => fs.Flashcards)
                .FirstOrDefaultAsync(fs => fs.FlashcardSetId == id && fs.CreatedBy == mentorId);

            if (flashcardSet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bộ flashcards.";
                return RedirectToAction(nameof(Index));
            }

            _context.Flashcards.RemoveRange(flashcardSet.Flashcards);
            _context.FlashcardSets.Remove(flashcardSet);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa bộ flashcards thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
