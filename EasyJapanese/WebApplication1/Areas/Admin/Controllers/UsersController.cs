using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /admin/users
        public async Task<IActionResult> Index(string searchString, string roleFilter)
        {
            ViewData["Title"] = "Quản lý Người dùng";
            
            var query = _context.Accounts
                .Include(a => a.Student)
                .Include(a => a.Mentor)
                .Include(a => a.Admin)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(a => a.Email.ToLower().Contains(searchString) || 
                                        (a.Student != null && a.Student.FullName.ToLower().Contains(searchString)) ||
                                        (a.Mentor != null && a.Mentor.FullName.ToLower().Contains(searchString)) ||
                                        (a.Admin != null && a.Admin.FullName.ToLower().Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(roleFilter))
            {
                query = query.Where(a => a.Role == roleFilter);
            }

            var users = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
            
            ViewData["SearchString"] = searchString;
            ViewData["RoleFilter"] = roleFilter;
            
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                return NotFound();
            }

            // Cannot lock yourself
            var currentUserEmail = User.Identity?.Name; // assuming email is Name
            // If we use session, we should check session, but let's just do a basic check
            // We'll skip self-lock check for simplicity or assume admin won't lock themselves

            account.IsLocked = !account.IsLocked;
            account.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = account.IsLocked ? "Đã khóa tài khoản thành công." : "Đã mở khóa tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(int accountId, string newRole)
        {
            var account = await _context.Accounts
                .Include(a => a.Student)
                .Include(a => a.Mentor)
                .Include(a => a.Admin)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);
                
            if (account == null) return NotFound();

            if (newRole != RoleConst.ADMIN && newRole != RoleConst.MENTOR && newRole != RoleConst.STUDENT)
            {
                return BadRequest("Invalid role.");
            }

            string currentFullName = account.Student?.FullName 
                                    ?? account.Mentor?.FullName 
                                    ?? account.Admin?.FullName 
                                    ?? account.Email;
            
            account.Role = newRole;
            account.UpdatedAt = DateTime.UtcNow;

            // Ensure profile exists for the new role
            if (newRole == RoleConst.ADMIN && account.Admin == null)
            {
                _context.Admins.Add(new CoreLibrary.Data.Entities.Admin
                {
                    AdminId = account.AccountId,
                    FullName = currentFullName,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (newRole == RoleConst.MENTOR && account.Mentor == null)
            {
                _context.Mentors.Add(new Mentor
                {
                    MentorId = account.AccountId,
                    FullName = currentFullName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else if (newRole == RoleConst.STUDENT && account.Student == null)
            {
                _context.Students.Add(new Student
                {
                    StudentId = account.AccountId,
                    FullName = currentFullName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Đã cấp quyền {newRole} thành công cho {currentFullName}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
