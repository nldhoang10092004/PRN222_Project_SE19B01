using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CoreLibrary.Utility;
using CoreLibrary.Const;

namespace WebApplication1.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.MENTOR)]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Tổng quan Giáo viên";
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var courses = await _context.Courses
                .Include(c => c.Enrollments)
                .Where(c => c.CreatedBy == mentorId)
                .ToListAsync();

            ViewData["TotalCourses"] = courses.Count;
            ViewData["ActiveCourses"] = courses.Count(c => c.IsPublished);
            ViewData["TotalStudents"] = courses.SelectMany(c => c.Enrollments).Select(e => e.StudentId).Distinct().Count();

            // Lấy điểm số trung bình khóa học do giáo viên dạy
            var averageRating = await _context.CourseReviews
                .Where(r => r.Course.CreatedBy == mentorId)
                .AverageAsync(r => (double?)r.Rating) ?? 0.0;

            ViewData["AverageRating"] = averageRating.ToString("0.0");
            
            // Unanswered Q&A could be implemented if there's a table, otherwise use dummy for now
            ViewData["UnansweredQA"] = 0;

            var activeCourseList = courses.Where(c => c.IsPublished).Take(5).ToList();

            return View(activeCourseList);
        }
    }
}
