using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using CoreLibrary.Utility;

namespace WebApplication1.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.MENTOR)]
    public class CoursesController : Controller
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /teacher/courses
        public async Task<IActionResult> Index()
        {
            var mentorId = GetCurrentMentorId();
            if (mentorId == 0) return RedirectToAction("Index", "Login", new { area = "" });

            var courses = await _context.Courses
                .Include(c => c.Level)
                .Include(c => c.Enrollments)
                .Where(c => c.CreatedBy == mentorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(courses);
        }

        // GET: /teacher/courses/create
        public IActionResult Create()
        {
            ViewData["LevelId"] = new SelectList(_context.JlptLevels, "LevelId", "LevelName");
            return View();
        }

        // POST: /teacher/courses/create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,LevelId,IsFree")] Course course)
        {
            var mentorId = GetCurrentMentorId();
            if (mentorId == 0) return RedirectToAction("Index", "Login", new { area = "" });

            if (ModelState.IsValid)
            {
                course.CreatedBy = mentorId;
                course.MentorId = mentorId;
                course.IsPublished = false; // default draft
                course.CreatedAt = DateTime.UtcNow;
                course.UpdatedAt = DateTime.UtcNow;

                _context.Add(course);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Khởi tạo khóa học thành công.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["LevelId"] = new SelectList(_context.JlptLevels, "LevelId", "LevelName", course.LevelId);
            return View(course);
        }
        
        [HttpPost]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var mentorId = GetCurrentMentorId();
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id && c.CreatedBy == mentorId);
            if (course == null) return NotFound();
            
            course.IsPublished = !course.IsPublished;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private int GetCurrentMentorId()
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            return user?.AccountId ?? 0;
        }
    }
}
