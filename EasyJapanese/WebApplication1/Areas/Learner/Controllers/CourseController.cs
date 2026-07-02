using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreLibrary.Data;
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
        public async Task<IActionResult> Index()
        {
            var courses = await _db.Courses
                .Include(c => c.Level)
                .Include(c => c.Mentor)
                .ToListAsync();
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
