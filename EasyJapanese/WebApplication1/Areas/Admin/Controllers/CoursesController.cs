using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CoreLibrary.Const;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    [Route("admin/courses")]
    public class CoursesController : Controller
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["Title"] = "Quản lý Khóa học";
            ViewData["SearchString"] = searchString;

            var query = _context.Courses
                .Include(c => c.Level)
                .Include(c => c.Mentor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(searchString));
            }

            var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(courses);
        }

        [HttpPost("toggle-publish")]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.IsPublished = !course.IsPublished;
            course.UpdatedAt = System.DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = course.IsPublished ? "Đã cho phép hiển thị khóa học." : "Đã khóa (ẩn) khóa học thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
