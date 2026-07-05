using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreLibrary.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class CourseController : Controller
    {
        private readonly AppDbContext _db;

        public CourseController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /learn/Course
        public async Task<IActionResult> Index(
            string? level,
            string? price,
            string? sort,
            string? q,
            CancellationToken cancellationToken)
        {
            var query = _db.Courses
                .Include(c => c.Level)
                .Include(c => c.Mentor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(level))
            {
                query = query.Where(c => c.Level != null && c.Level.LevelName == level);
            }

            if (string.Equals(price, "free", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => c.IsFree);
            }
            else if (string.Equals(price, "paid", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => !c.IsFree);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(c =>
                    c.Title.Contains(term) ||
                    (c.Description != null && c.Description.Contains(term)));
            }

            query = sort?.ToLowerInvariant() switch
            {
                "name" => query.OrderBy(c => c.Title),
                "name-desc" => query.OrderByDescending(c => c.Title),
                _ => query.OrderBy(c => c.Level!.SortOrder)
                         .ThenByDescending(c => c.CreatedAt)
            };

            var courses = await query.ToListAsync(cancellationToken);

            ViewBag.CurrentLevel = level ?? "";
            ViewBag.CurrentPrice = price ?? "";
            ViewBag.CurrentSort = string.IsNullOrWhiteSpace(sort) ? "level" : sort.ToLowerInvariant();
            ViewBag.CurrentQuery = q ?? "";

            return View(courses);
        }

        // GET: /learn/Course/Detail/1
        public async Task<IActionResult> Detail(int id = 1)
        {
            var course = await _db.Courses
                .Include(c => c.Level)
                .Include(c => c.Mentor)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();
            return View(course);
        }

        // GET: /learn/Course/Lesson/1
        public async Task<IActionResult> Lesson(int id = 1)
        {
            var lesson = await _db.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == id);
                
            if (lesson == null) return NotFound();
            return View(lesson);
        }
        // GET: /learn/Course/StartBasic
        public async Task<IActionResult> StartBasic()
        {
            var basicCourse = await _db.Courses
                .Include(c => c.Level)
                .OrderBy(c => c.CourseId)
                .FirstOrDefaultAsync(c => c.Level != null && c.Level.LevelName == "N5");
            
            if (basicCourse != null)
            {
                return RedirectToAction("Detail", new { id = basicCourse.CourseId });
            }
            
            return RedirectToAction("Index");
        }
    }
}
