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
    [Route("teacher/courses")]
    public class CoursesController : Controller
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        private async Task EnsureMentorRowExists(int mentorId, string defaultName)
        {
            var exists = await _context.Mentors.AnyAsync(m => m.MentorId == mentorId);
            if (!exists)
            {
                var mentor = new Mentor
                {
                    MentorId = mentorId,
                    FullName = string.IsNullOrEmpty(defaultName) ? "Giáo viên" : defaultName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Mentors.Add(mentor);
                await _context.SaveChangesAsync();
            }
        }

        // GET: /teacher/courses
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var mentorId = GetCurrentMentorId();
            if (mentorId == 0) return RedirectToAction("Index", "Login", new { area = "" });

            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            await EnsureMentorRowExists(mentorId, user?.FullName);

            var courses = await _context.Courses
                .Include(c => c.Level)
                .Include(c => c.Enrollments)
                .Where(c => c.CreatedBy == mentorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(courses);
        }

        // GET: /teacher/courses/create
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var mentorId = GetCurrentMentorId();
            if (mentorId == 0) return RedirectToAction("Index", "Login", new { area = "" });

            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            await EnsureMentorRowExists(mentorId, user?.FullName);

            ViewData["LevelId"] = new SelectList(_context.JlptLevels, "LevelId", "LevelName");
            return View();
        }

        // POST: /teacher/courses/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,LevelId,IsFree")] Course course)
        {
            var mentorId = GetCurrentMentorId();
            if (mentorId == 0) return RedirectToAction("Index", "Login", new { area = "" });

            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            await EnsureMentorRowExists(mentorId, user?.FullName);

            // Remove navigation properties from validation list
            ModelState.Remove("Level");
            ModelState.Remove("CreatedByNavigation");

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

        // GET: /teacher/courses/edit/5
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var mentorId = GetCurrentMentorId();
            if (mentorId == 0) return RedirectToAction("Index", "Login", new { area = "" });

            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            await EnsureMentorRowExists(mentorId, user?.FullName);

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id && c.CreatedBy == mentorId);
            if (course == null) return NotFound();

            ViewData["LevelId"] = new SelectList(_context.JlptLevels, "LevelId", "LevelName", course.LevelId);
            return View(course);
        }

        // POST: /teacher/courses/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,Title,Description,LevelId,IsFree")] Course course)
        {
            var mentorId = GetCurrentMentorId();
            if (mentorId == 0) return RedirectToAction("Index", "Login", new { area = "" });

            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            await EnsureMentorRowExists(mentorId, user?.FullName);

            if (id != course.CourseId) return NotFound();

            var existing = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id && c.CreatedBy == mentorId);
            if (existing == null) return Forbid();

            ModelState.Remove("Level");
            ModelState.Remove("CreatedByNavigation");

            if (ModelState.IsValid)
            {
                existing.Title = course.Title;
                existing.Description = course.Description;
                existing.LevelId = course.LevelId;
                existing.IsFree = course.IsFree;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin khóa học thành công.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["LevelId"] = new SelectList(_context.JlptLevels, "LevelId", "LevelName", course.LevelId);
            return View(course);
        }

        // POST: /teacher/courses/delete/5
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var mentorId = GetCurrentMentorId();
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Flashcards)
                .Include(c => c.Quizzes)
                .FirstOrDefaultAsync(c => c.CourseId == id && c.CreatedBy == mentorId);

            if (course == null) return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa khóa học thành công.";
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost("toggle-publish")]
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
