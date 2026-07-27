using System;
using System.Linq;
using System.Threading.Tasks;
using CoreLibrary.Const;
using CoreLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    [Route("admin/community")]
    public class CommunityController : Controller
    {
        private readonly AppDbContext _context;

        public CommunityController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? searchString, string? categoryFilter)
        {
            ViewData["Title"] = "Quản lý Cộng đồng";
            ViewData["SearchString"] = searchString;
            ViewData["CategoryFilter"] = categoryFilter;

            var query = _context.CommunityPosts
                .Include(p => p.CommunityComments)
                .Include(p => p.CommunityLikes)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim().ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(s) || p.AuthorName.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(categoryFilter))
            {
                query = query.Where(p => p.Category == categoryFilter);
            }

            var posts = await query
                .OrderByDescending(p => p.IsPinned)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Chi tiết Bài viết & Bình luận";
            var post = await _context.CommunityPosts
                .Include(p => p.CommunityComments)
                .Include(p => p.CommunityLikes)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound();

            return View(post);
        }

        [HttpPost("toggle-approval/{id}")]
        public async Task<IActionResult> ToggleApproval(int id)
        {
            var post = await _context.CommunityPosts.FindAsync(id);
            if (post == null) return NotFound();

            post.IsApproved = !post.IsApproved;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = post.IsApproved ? "Đã duyệt bài viết." : "Đã ẩn bài viết.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("toggle-pin/{id}")]
        public async Task<IActionResult> TogglePin(int id)
        {
            var post = await _context.CommunityPosts.FindAsync(id);
            if (post == null) return NotFound();

            post.IsPinned = !post.IsPinned;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = post.IsPinned ? "Đã ghim bài viết lên đầu." : "Đã bỏ ghim bài viết.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete-post/{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.CommunityPosts
                .Include(p => p.CommunityComments)
                .Include(p => p.CommunityLikes)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound();

            _context.CommunityPosts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa bài viết thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete-comment/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.CommunityComments.FindAsync(id);
            if (comment == null) return NotFound();

            int postId = comment.PostId;
            _context.CommunityComments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa bình luận thành công.";
            return RedirectToAction(nameof(Details), new { id = postId });
        }
    }
}
