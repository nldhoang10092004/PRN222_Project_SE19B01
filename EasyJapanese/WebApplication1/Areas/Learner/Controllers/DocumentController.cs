using CoreLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class DocumentController : Controller
    {
        private const int PageSize = 12;

        private readonly AppDbContext _db;

        public DocumentController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /learn/Document?level=N5&sort=az&page=1
        [HttpGet]
        public async Task<IActionResult> Index(string? level, string? sort, int page = 1)
        {
            var levels = await _db.JlptLevels
                .OrderBy(l => l.SortOrder)
                .ToListAsync();

            var query = _db.KanjiEntries
                .Include(k => k.Level)
                .Include(k => k.Examples)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(level))
            {
                query = query.Where(k => k.Level.LevelName == level);
            }

            query = string.Equals(sort, "az", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(k => k.Character)
                : query.OrderByDescending(k => k.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            var items = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Levels = levels;
            ViewBag.CurrentLevel = level ?? "";
            ViewBag.CurrentSort = string.IsNullOrWhiteSpace(sort) ? "newest" : sort.ToLowerInvariant();
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(items);
        }
    }
}
