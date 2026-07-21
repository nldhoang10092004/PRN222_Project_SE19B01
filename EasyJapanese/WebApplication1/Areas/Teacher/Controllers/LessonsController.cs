using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using CoreLibrary.Utility;

namespace WebApplication1.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.MENTOR)]
    [Route("teacher/lessons")]
    public class LessonsController : Controller
    {
        private readonly AppDbContext _context;

        public LessonsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /teacher/lessons?courseId=1
        [HttpGet("")]
        public async Task<IActionResult> Index(int courseId)
        {
            var mentorId = GetCurrentMentorId();
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.CreatedBy == mentorId);

            if (course == null) return NotFound("Không tìm thấy khóa học hoặc bạn không có quyền.");

            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();

            ViewData["CourseId"] = courseId;
            ViewData["CourseTitle"] = course.Title;

            return View(lessons);
        }

        // GET: /teacher/lessons/create?courseId=1
        [HttpGet("create")]
        public IActionResult Create(int courseId)
        {
            ViewData["CourseId"] = courseId;
            return View();
        }

        // POST: /teacher/lessons/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,Title,LessonType,VideoUrl,Content,Duration,IsPreview,SortOrder")] Lesson lesson)
        {
            var mentorId = GetCurrentMentorId();
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == lesson.CourseId && c.CreatedBy == mentorId);
            if (course == null) return NotFound();

            ModelState.Remove("Course");

            if (ModelState.IsValid)
            {
                lesson.CreatedAt = DateTime.UtcNow;
                _context.Add(lesson);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm bài giảng thành công.";
                return RedirectToAction(nameof(Index), new { courseId = lesson.CourseId });
            }
            
            ViewData["CourseId"] = lesson.CourseId;
            return View(lesson);
        }

        // GET: /teacher/lessons/edit/5
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var mentorId = GetCurrentMentorId();
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == id && l.Course.CreatedBy == mentorId);

            if (lesson == null) return NotFound();

            ViewData["CourseId"] = lesson.CourseId;
            return View(lesson);
        }

        // POST: /teacher/lessons/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LessonId,CourseId,Title,LessonType,VideoUrl,Content,Duration,IsPreview,SortOrder")] Lesson lesson)
        {
            var mentorId = GetCurrentMentorId();
            if (id != lesson.LessonId) return NotFound();

            var existing = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == id && l.Course.CreatedBy == mentorId);

            if (existing == null) return Forbid();

            ModelState.Remove("Course");

            if (ModelState.IsValid)
            {
                existing.Title = lesson.Title;
                existing.LessonType = lesson.LessonType;
                existing.VideoUrl = lesson.VideoUrl;
                existing.Content = lesson.Content;
                existing.Duration = lesson.Duration;
                existing.IsPreview = lesson.IsPreview;
                existing.SortOrder = lesson.SortOrder;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật bài giảng thành công.";
                return RedirectToAction(nameof(Index), new { courseId = lesson.CourseId });
            }

            ViewData["CourseId"] = lesson.CourseId;
            return View(lesson);
        }

        // POST: /teacher/lessons/delete/5
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var mentorId = GetCurrentMentorId();
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == id && l.Course.CreatedBy == mentorId);

            if (lesson == null) return NotFound();

            var courseId = lesson.CourseId;
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa bài giảng thành công.";
            return RedirectToAction(nameof(Index), new { courseId });
        }

        // GET: /teacher/lessons/manage?courseId=1&lessonId=2
        [HttpGet("manage")]
        public async Task<IActionResult> Manage(int? courseId, int? lessonId)
        {
            var mentorId = GetCurrentMentorId();
            var courses = await _context.Courses
                .Where(c => c.CreatedBy == mentorId)
                .OrderBy(c => c.Title)
                .ToListAsync();

            ViewBag.Courses = courses;

            if (!courseId.HasValue || !courses.Any(c => c.CourseId == courseId.Value))
            {
                return View((ManageLessonViewModel)null!);
            }

            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId.Value)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();

            ViewBag.Lessons = lessons;
            ViewBag.SelectedCourseId = courseId.Value;

            if (!lessonId.HasValue || !lessons.Any(l => l.LessonId == lessonId.Value))
            {
                return View((ManageLessonViewModel)null!);
            }

            var lesson = lessons.First(l => l.LessonId == lessonId.Value);
            var lessonCourse = courses.First(c => c.CourseId == courseId.Value);

            var quizzes = await _context.Quizzes
                .Where(q => q.LessonId == lessonId.Value)
                .Include(q => q.Questions)
                .OrderBy(q => q.SortOrder).ThenBy(q => q.CreatedAt)
                .ToListAsync();

            // LessonsByCourse for JS filter
            var lessonsByCourse = lessons
                .GroupBy(l => l.CourseId)
                .ToDictionary(g => g.Key, g => g.Select(l => new { l.LessonId, l.Title }).ToList());

            var vm = new ManageLessonViewModel
            {
                CourseId = courseId.Value,
                CourseTitle = lessonCourse.Title,
                LessonId = lessonId.Value,
                LessonTitle = lesson.Title,
                Quizzes = quizzes.Select(q => new ManageQuizItemViewModel
                {
                    QuizId = q.QuizId,
                    Title = q.Title,
                    Duration = q.Duration,
                    PassScore = q.PassScore,
                    QuestionCount = q.Questions?.Count ?? 0
                }).ToList()
            };

            ViewBag.LessonsByCourse = lessonsByCourse;
            return View(vm);
        }

        private int GetCurrentMentorId()
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            return user?.AccountId ?? 0;
        }
    }

    public class ManageLessonViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public List<ManageQuizItemViewModel> Quizzes { get; set; } = new();
    }

    public class ManageQuizItemViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? Duration { get; set; }
        public int PassScore { get; set; }
        public int QuestionCount { get; set; }
    }
}
