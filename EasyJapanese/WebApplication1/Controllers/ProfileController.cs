using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreLibrary.Authentication;
using CoreLibrary.Const;
using CoreLibrary.Data;
using CoreLibrary.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Profile;

namespace WebApplication1.Controllers
{
    [Route("Learner/Profile")]
    [Route("Teacher/Profile")]
    [Route("Admin/Profile")]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IAuthenticationService _auth;
        private readonly IWebHostEnvironment _env;

        public ProfileController(AppDbContext db, IAuthenticationService auth, IWebHostEnvironment env)
        {
            _db = db;
            _auth = auth;
            _env = env;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(string? tab, CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null) return RedirectToAction("Index", "Login", new { area = "" });

            var vm = await BuildViewModelAsync(currentUser.AccountId, currentUser.Role, cancellationToken);
            if (vm == null) return RedirectToAction("Index", "Login", new { area = "" });

            ViewBag.ActiveTab = tab == "security" ? "security" : "info";
            return View(vm);
        }

        [HttpPost]
        [Route("Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model, IFormFile? avatarFile, CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null) return RedirectToAction("Index", "Login", new { area = "" });

            string? newAvatarUrl = null;
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(avatarFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream, cancellationToken);
                }
                newAvatarUrl = "/images/avatars/" + uniqueFileName;
            }

            var trimmedFullName = model.FullName?.Trim();

            switch (currentUser.Role)
            {
                case RoleConst.STUDENT:
                    {
                        var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == currentUser.AccountId, cancellationToken);
                        if (student == null) return NotFound();
                        if (!string.IsNullOrWhiteSpace(trimmedFullName)) student.FullName = trimmedFullName;
                        student.PhoneNumber = model.PhoneNumber;
                        student.DateOfBirth = model.DateOfBirth;
                        if (newAvatarUrl != null) student.AvatarUrl = newAvatarUrl;
                        student.UpdatedAt = DateTime.UtcNow;
                        break;
                    }
                case RoleConst.MENTOR:
                    {
                        var mentor = await _db.Mentors.FirstOrDefaultAsync(m => m.MentorId == currentUser.AccountId, cancellationToken);
                        if (mentor == null) return NotFound();
                        if (!string.IsNullOrWhiteSpace(trimmedFullName)) mentor.FullName = trimmedFullName;
                        mentor.PhoneNumber = model.PhoneNumber;
                        mentor.Bio = model.Bio;
                        mentor.Expertise = model.Expertise;
                        if (newAvatarUrl != null) mentor.AvatarUrl = newAvatarUrl;
                        mentor.UpdatedAt = DateTime.UtcNow;
                        break;
                    }
                case RoleConst.ADMIN:
                    {
                        var admin = await _db.Admins.FirstOrDefaultAsync(a => a.AdminId == currentUser.AccountId, cancellationToken);
                        if (admin == null) return NotFound();
                        if (!string.IsNullOrWhiteSpace(trimmedFullName)) admin.FullName = trimmedFullName;
                        if (newAvatarUrl != null) admin.AvatarUrl = newAvatarUrl;
                        break;
                    }
                default:
                    return Forbid();
            }

            await _db.SaveChangesAsync(cancellationToken);

            // Cập nhật lại session để header/sidebar hiện tên mới ngay, không cần đăng nhập lại
            currentUser.FullName = !string.IsNullOrWhiteSpace(trimmedFullName) ? trimmedFullName : currentUser.FullName;
            HttpContext.Session.SetObject(IAuthenticationService.SessionKeyCurrentUser, currentUser);

            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<ProfileViewModel?> BuildViewModelAsync(int accountId, string role, CancellationToken cancellationToken)
        {
            switch (role)
            {
                case RoleConst.STUDENT:
                    {
                        var student = await _db.Students
                            .Include(s => s.StudentNavigation)
                            .FirstOrDefaultAsync(s => s.StudentId == accountId, cancellationToken);
                        if (student == null) return null;

                        var placement = await _db.StudentPlacementResults
                            .Include(r => r.RecommendedLevel)
                            .Where(r => r.StudentId == accountId && r.CompletedAt != null)
                            .OrderByDescending(r => r.CompletedAt)
                            .FirstOrDefaultAsync(cancellationToken);

                        return new ProfileViewModel
                        {
                            Role = RoleConst.STUDENT,
                            FullName = student.FullName,
                            Email = student.StudentNavigation.Email,
                            AvatarUrl = student.AvatarUrl,
                            PhoneNumber = student.PhoneNumber,
                            DateOfBirth = student.DateOfBirth,
                            JlptLevel = placement?.RecommendedLevel?.LevelName ?? "N5"
                        };
                    }
                case RoleConst.MENTOR:
                    {
                        var mentor = await _db.Mentors
                            .Include(m => m.MentorNavigation)
                            .FirstOrDefaultAsync(m => m.MentorId == accountId, cancellationToken);
                        if (mentor == null) return null;

                        return new ProfileViewModel
                        {
                            Role = RoleConst.MENTOR,
                            FullName = mentor.FullName,
                            Email = mentor.MentorNavigation.Email,
                            AvatarUrl = mentor.AvatarUrl,
                            PhoneNumber = mentor.PhoneNumber,
                            Bio = mentor.Bio,
                            Expertise = mentor.Expertise
                        };
                    }
                case RoleConst.ADMIN:
                    {
                        var admin = await _db.Admins
                            .Include(a => a.AdminNavigation) 
                            .FirstOrDefaultAsync(a => a.AdminId == accountId, cancellationToken);
                        if (admin == null) return null;

                        return new ProfileViewModel
                        {
                            Role = RoleConst.ADMIN,
                            FullName = admin.FullName,
                            Email = admin.AdminNavigation.Email,
                            AvatarUrl = admin.AvatarUrl
                        };
                    }
                default:
                    return null;
            }
        }

        [HttpPost]
        [Route("ChangePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword, CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null) return RedirectToAction("Index", "Login", new { area = "" });

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["SecurityErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                return RedirectToAction(nameof(Index), new { tab = "security" });
            }

            if (newPassword != confirmPassword)
            {
                TempData["SecurityErrorMessage"] = "Mật khẩu mới và xác nhận không khớp.";
                return RedirectToAction(nameof(Index), new { tab = "security" });
            }

            if (newPassword.Length < 8 || !newPassword.Any(char.IsDigit) || !newPassword.Any(char.IsLetter))
            {
                TempData["SecurityErrorMessage"] = "Mật khẩu mới phải có ít nhất 8 ký tự, gồm cả chữ và số.";
                return RedirectToAction(nameof(Index), new { tab = "security" });
            }

            var account = await _db.Accounts.FirstOrDefaultAsync(a => a.AccountId == currentUser.AccountId, cancellationToken);
            if (account == null || string.IsNullOrEmpty(account.PasswordHash))
            {
                TempData["SecurityErrorMessage"] = "Không tìm thấy tài khoản.";
                return RedirectToAction(nameof(Index), new { tab = "security" });
            }

            if (!CoreLibrary.Utility.PasswordUtil.VerifyPassword(currentPassword, account.PasswordHash))
            {
                TempData["SecurityErrorMessage"] = "Mật khẩu hiện tại không đúng.";
                return RedirectToAction(nameof(Index), new { tab = "security" });
            }

            account.PasswordHash = CoreLibrary.Utility.PasswordUtil.HashPassword(newPassword);
            account.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            TempData["SecuritySuccessMessage"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Index), new { tab = "security" });
        }
    }
}