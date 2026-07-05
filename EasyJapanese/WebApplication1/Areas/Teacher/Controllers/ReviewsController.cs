using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using CoreLibrary.Utility;

namespace WebApplication1.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.MENTOR)]
    [Route("teacher/reviews")]
    public class ReviewsController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Đánh giá học viên";
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            // Fetch reviews for courses created by this teacher
            var reviews = await _context.CourseReviews
                .Include(r => r.Course)
                .Include(r => r.Student)
                .Include(r => r.ReviewResponses)
                    .ThenInclude(rp => rp.Responder)
                .Where(r => r.Course.CreatedBy == mentorId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        [HttpPost("respond")]
        public async Task<IActionResult> Respond(int reviewId, string replyText)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            if (string.IsNullOrWhiteSpace(replyText))
            {
                TempData["ErrorMessage"] = "Nội dung phản hồi không được để trống.";
                return RedirectToAction(nameof(Index));
            }

            // Verify teacher owns the course that this review belongs to
            var review = await _context.CourseReviews
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.Course.CreatedBy == mentorId);

            if (review == null) return Forbid();

            // Check if there is an existing response
            var existingResponse = await _context.ReviewResponses
                .FirstOrDefaultAsync(rp => rp.ReviewId == reviewId && rp.ResponderId == mentorId);

            if (existingResponse != null)
            {
                existingResponse.Response = replyText;
                existingResponse.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                var response = new ReviewResponse
                {
                    ReviewId = reviewId,
                    ResponderId = mentorId,
                    Response = replyText,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ReviewResponses.Add(response);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã gửi phản hồi đánh giá thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
