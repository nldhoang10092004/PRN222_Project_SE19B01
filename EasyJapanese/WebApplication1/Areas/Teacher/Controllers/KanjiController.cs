using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using Microsoft.EntityFrameworkCore;
using CoreLibrary.Const;

namespace WebApplication1.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.MENTOR)]
    public class KanjiController : Controller
    {
        private readonly AppDbContext _context;

        public KanjiController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? levelId)
        {
            ViewData["Title"] = "Quản lý Thư viện Kanji";
            ViewData["SearchString"] = searchString;
            ViewData["LevelId"] = levelId;

            var query = _context.KanjiEntries
                .Include(k => k.Level)
                .Include(k => k.KanjiExamples)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(k => k.Character.Contains(searchString) ||
                                        (k.Meaning != null && k.Meaning.ToLower().Contains(searchString)));
            }

            if (levelId.HasValue)
            {
                query = query.Where(k => k.LevelId == levelId.Value);
            }

            var kanjis = await query.OrderBy(k => k.Level.LevelName).ThenBy(k => k.Character).ToListAsync();

            ViewBag.Levels = await _context.JlptLevels.OrderBy(l => l.LevelName).ToListAsync();

            return View(kanjis);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Thêm Kanji mới";
            ViewBag.Levels = await _context.JlptLevels.OrderBy(l => l.LevelName).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int levelId, string character, string? meaning,
            string? onYomi, string? kunYomi, int? strokeCount, string? strokeOrderUrl)
        {
            if (string.IsNullOrWhiteSpace(character))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập ký tự Kanji.";
                ViewBag.Levels = await _context.JlptLevels.OrderBy(l => l.LevelName).ToListAsync();
                return View();
            }

            var kanji = new CoreLibrary.Data.Entities.KanjiEntry
            {
                LevelId = levelId,
                Character = character,
                Meaning = meaning,
                OnYomi = onYomi,
                KunYomi = kunYomi,
                StrokeCount = strokeCount,
                StrokeOrderUrl = strokeOrderUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.KanjiEntries.Add(kanji);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm Kanji thành công.";
            return RedirectToAction("Index", "Kanji");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var kanji = await _context.KanjiEntries
                .Include(k => k.KanjiExamples)
                .FirstOrDefaultAsync(k => k.KanjiId == id);

            if (kanji == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy Kanji.";
                return RedirectToAction("Index", "Kanji");
            }

            ViewData["Title"] = "Chỉnh sửa Kanji";
            ViewBag.Levels = await _context.JlptLevels.OrderBy(l => l.LevelName).ToListAsync();
            return View(kanji);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int levelId, string character, string? meaning,
            string? onYomi, string? kunYomi, int? strokeCount, string? strokeOrderUrl)
        {
            var kanji = await _context.KanjiEntries.FindAsync(id);
            if (kanji == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy Kanji.";
                return RedirectToAction("Index", "Kanji");
            }

            if (string.IsNullOrWhiteSpace(character))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập ký tự Kanji.";
                ViewBag.Levels = await _context.JlptLevels.OrderBy(l => l.LevelName).ToListAsync();
                return View(kanji);
            }

            kanji.LevelId = levelId;
            kanji.Character = character;
            kanji.Meaning = meaning;
            kanji.OnYomi = onYomi;
            kanji.KunYomi = kunYomi;
            kanji.StrokeCount = strokeCount;
            kanji.StrokeOrderUrl = strokeOrderUrl;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật Kanji thành công.";
            return RedirectToAction("Index", "Kanji");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var kanji = await _context.KanjiEntries
                .Include(k => k.KanjiExamples)
                .FirstOrDefaultAsync(k => k.KanjiId == id);

            if (kanji == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy Kanji.";
                return RedirectToAction("Index", "Kanji");
            }

            _context.KanjiEntries.Remove(kanji);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa Kanji thành công.";
            return RedirectToAction("Index", "Kanji");
        }
    }
}
