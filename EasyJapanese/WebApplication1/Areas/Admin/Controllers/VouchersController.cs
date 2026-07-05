using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using CoreLibrary.Utility;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    [Route("admin/vouchers")]
    public class VouchersController : Controller
    {
        private readonly AppDbContext _context;

        public VouchersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý Vouchers";
            var vouchers = await _context.Vouchers
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
            
            // For create/edit form, load active plans
            ViewBag.Plans = await _context.SubscriptionPlans.Where(p => p.IsActive).ToListAsync();
            return View(vouchers);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Voucher model)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var adminId = user?.AccountId ?? 0;

            // Make sure admin exists in Admins table
            var adminExists = await _context.Admins.AnyAsync(a => a.AdminId == adminId);
            if (!adminExists)
            {
                var newAdmin = new CoreLibrary.Data.Entities.Admin
                {
                    AdminId = adminId,
                    FullName = user?.FullName ?? "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Admins.Add(newAdmin);
                await _context.SaveChangesAsync();
            }

            model.Code = model.Code.Trim().ToUpper();
            model.UsedCount = 0;
            model.IsActive = true;
            model.CreatedBy = adminId;
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            _context.Vouchers.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo mã giảm giá mới thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit(Voucher model)
        {
            var voucher = await _context.Vouchers.FindAsync(model.VoucherId);
            if (voucher == null) return NotFound();

            voucher.Code = model.Code.Trim().ToUpper();
            voucher.Description = model.Description;
            voucher.DiscountType = model.DiscountType;
            voucher.DiscountValue = model.DiscountValue;
            voucher.MaxDiscountCap = model.MaxDiscountCap;
            voucher.MinOrderValue = model.MinOrderValue;
            voucher.MaxUsesTotal = model.MaxUsesTotal;
            voucher.StartsAt = model.StartsAt;
            voucher.ExpiresAt = model.ExpiresAt;
            voucher.ApplicablePlanId = model.ApplicablePlanId;
            voucher.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật mã giảm giá thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("toggle-status")]
        public async Task<IActionResult> ToggleStatus(int voucherId)
        {
            var voucher = await _context.Vouchers.FindAsync(voucherId);
            if (voucher == null) return NotFound();

            voucher.IsActive = !voucher.IsActive;
            voucher.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = voucher.IsActive ? "Đã kích hoạt Voucher." : "Đã tạm ẩn Voucher.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int voucherId)
        {
            var voucher = await _context.Vouchers.FindAsync(voucherId);
            if (voucher == null) return NotFound();

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa Voucher thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
