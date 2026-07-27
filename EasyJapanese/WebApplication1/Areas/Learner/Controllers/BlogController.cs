using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreLibrary.Authentication;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreWeb.Areas.Learner.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    [Route("learn/blog")]
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuthenticationService _auth;
        private readonly IWebHostEnvironment _env;
        private const int PageSize = 6;

        public BlogController(AppDbContext context, IAuthenticationService auth, IWebHostEnvironment env)
        {
            _context = context;
            _auth = auth;
            _env = env;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? category, string? search, string? sort, int page = 1)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var query = _context.CommunityPosts
                .Include(p => p.CommunityComments)
                .Where(p => p.IsApproved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(s) || p.Content.ToLower().Contains(s));
            }

            // Featured pinned post (if no category/search filter, get pinned post)
            CommunityPost? featured = null;
            if (string.IsNullOrWhiteSpace(category) && string.IsNullOrWhiteSpace(search))
            {
                featured = await _context.CommunityPosts
                    .Include(p => p.CommunityComments)
                    .FirstOrDefaultAsync(p => p.IsPinned && p.IsApproved);
                
                if (featured == null)
                {
                    featured = await query.OrderByDescending(p => p.LikeCount).FirstOrDefaultAsync();
                }

                if (featured != null)
                {
                    query = query.Where(p => p.PostId != featured.PostId);
                }
            }

            // Sort
            query = sort switch
            {
                "popular" => query.OrderByDescending(p => p.ViewCount),
                "liked" => query.OrderByDescending(p => p.LikeCount),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            var posts = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var recentPosts = await _context.CommunityPosts
                .Where(p => p.IsApproved)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            var vm = new CommunityIndexViewModel
            {
                FullName = currentUser.FullName ?? currentUser.Email,
                CurrentUserId = currentUser.AccountId,
                FeaturedPost = featured,
                Posts = posts,
                RecentPosts = recentPosts,
                CurrentCategory = category ?? "",
                SearchQuery = search ?? "",
                CurrentSort = sort ?? "newest",
                Page = page,
                TotalPages = totalPages,
                TotalCount = totalCount
            };

            return View(vm);
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var post = await _context.CommunityPosts
                .Include(p => p.CommunityComments)
                .Include(p => p.CommunityLikes)
                .FirstOrDefaultAsync(p => p.PostId == id && p.IsApproved);

            if (post == null) return NotFound();

            // Increment view count
            post.ViewCount += 1;
            await _context.SaveChangesAsync();

            var comments = await _context.CommunityComments
                .Where(c => c.PostId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            bool isLiked = post.CommunityLikes.Any(l => l.AccountId == currentUser.AccountId);

            var vm = new CommunityDetailsViewModel
            {
                Post = post,
                Comments = comments,
                IsLikedByCurrentUser = isLiked,
                CurrentUserId = currentUser.AccountId
            };

            return View(vm);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string category, string content, IFormFile? imageFile, string? imageUrl)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null) return RedirectToAction("Index", "Login", new { area = "" });

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Tiêu đề và nội dung không được để trống.";
                return RedirectToAction(nameof(Index));
            }

            string finalImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? "https://images.unsplash.com/photo-1528360983277-13d401cdc186?q=80&w=900&auto=format&fit=crop" : imageUrl;

            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "community");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    finalImageUrl = $"/images/community/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Image upload error: " + ex.Message);
                }
            }

            var post = new CommunityPost
            {
                AuthorId = currentUser.AccountId,
                AuthorName = currentUser.FullName ?? currentUser.Email,
                AuthorRole = currentUser.Role ?? "Student",
                Title = title.Trim(),
                Category = string.IsNullOrWhiteSpace(category) ? "Kinh nghiệm học" : category,
                Content = content.Trim(),
                ImageUrl = finalImageUrl,
                IsApproved = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CommunityPosts.Add(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng bài viết thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null) return RedirectToAction("Index", "Login", new { area = "" });

            var post = await _context.CommunityPosts.FindAsync(id);
            if (post == null) return NotFound();

            if (post.AuthorId != currentUser.AccountId && currentUser.Role != "Admin")
            {
                return Forbid();
            }

            _context.CommunityPosts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa bài viết thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("like/{id}")]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null) return Json(new { success = false, message = "Chưa đăng nhập" });

            var post = await _context.CommunityPosts.Include(p => p.CommunityLikes).FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null) return Json(new { success = false, message = "Bài viết không tồn tại" });

            var existingLike = post.CommunityLikes.FirstOrDefault(l => l.AccountId == currentUser.AccountId);
            bool isLiked;
            if (existingLike != null)
            {
                _context.CommunityLikes.Remove(existingLike);
                post.LikeCount = Math.Max(0, post.LikeCount - 1);
                isLiked = false;
            }
            else
            {
                _context.CommunityLikes.Add(new CommunityLike
                {
                    PostId = id,
                    AccountId = currentUser.AccountId,
                    CreatedAt = DateTime.UtcNow
                });
                post.LikeCount += 1;
                isLiked = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, isLiked = isLiked, likeCount = post.LikeCount });
        }

        [HttpPost("comment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null) return RedirectToAction("Index", "Login", new { area = "" });

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Bình luận không được để trống.";
                return RedirectToAction("Details", new { id = postId });
            }

            var comment = new CommunityComment
            {
                PostId = postId,
                AuthorId = currentUser.AccountId,
                AuthorName = currentUser.FullName ?? currentUser.Email,
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.CommunityComments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã gửi bình luận thành công!";
            return RedirectToAction("Details", new { id = postId });
        }
    }
}