using CoreLibrary.Authentication;
using CoreLibrary.Const;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS.Models.V2.PaymentRequests;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class MembershipController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IAuthenticationService _auth;
        private readonly IPaymentService _payment;

        public MembershipController(AppDbContext db, IAuthenticationService auth, IPaymentService payment)
        {
            _db = db;
            _auth = auth;
            _payment = payment;
        }

        [HttpGet]
        public async Task<IActionResult> Plans(CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var plans = await _db.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.DurationDays)
                .ToListAsync(cancellationToken);

            return View(plans);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int planId, CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.PlanId == planId, cancellationToken);
            if (plan == null) return NotFound();

            return View(plan);
        }

        public class ApplyVoucherDto
        {
            public int PlanId { get; set; }
            public string VoucherCode { get; set; } = string.Empty;
        }

        [HttpPost("learn/membership/apply-voucher")]
        [HttpPost("Learner/Membership/ApplyVoucher")]
        public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherDto request)
        {
            if (string.IsNullOrWhiteSpace(request?.VoucherCode))
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá." });

            var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.PlanId == request.PlanId);
            if (plan == null) return Json(new { success = false, message = "Gói học không tồn tại." });

            var voucherCodeClean = request.VoucherCode.Trim().ToLower();
            var voucher = await _db.Vouchers.FirstOrDefaultAsync(v => v.Code.ToLower() == voucherCodeClean && v.IsActive);
            if (voucher == null)
                return Json(new { success = false, message = "Mã giảm giá không tồn tại hoặc đã bị ẩn." });

            if (voucher.StartsAt.HasValue && voucher.StartsAt.Value.Date > DateTime.UtcNow.Date)
                return Json(new { success = false, message = "Mã giảm giá chưa đến ngày áp dụng." });

            if (voucher.ExpiresAt.HasValue && voucher.ExpiresAt.Value.Date.AddDays(1) < DateTime.UtcNow)
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn sử dụng." });

            if (voucher.ApplicablePlanId.HasValue && voucher.ApplicablePlanId.Value != plan.PlanId)
                return Json(new { success = false, message = "Mã giảm giá không áp dụng cho gói học này." });

            if (voucher.MaxUsesTotal.HasValue && voucher.UsedCount >= voucher.MaxUsesTotal.Value)
                return Json(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng." });

            if (plan.Price < voucher.MinOrderValue)
                return Json(new { success = false, message = $"Giá trị đơn hàng tối thiểu để dùng mã này là {voucher.MinOrderValue:N0}đ." });

            decimal discountAmount = 0;
            bool isPercent = voucher.DiscountType.StartsWith("Percent", StringComparison.OrdinalIgnoreCase);

            if (isPercent)
            {
                discountAmount = plan.Price * (voucher.DiscountValue / 100m);
                if (voucher.MaxDiscountCap.HasValue && voucher.MaxDiscountCap.Value > 0)
                {
                    discountAmount = Math.Min(discountAmount, voucher.MaxDiscountCap.Value);
                }
            }
            else
            {
                discountAmount = voucher.DiscountValue;
            }

            discountAmount = Math.Min(discountAmount, plan.Price);
            decimal finalPrice = Math.Max(0, plan.Price - discountAmount);

            return Json(new
            {
                success = true,
                voucherId = voucher.VoucherId,
                voucherCode = voucher.Code,
                originalPrice = plan.Price,
                discountAmount = discountAmount,
                finalPrice = finalPrice,
                message = "Áp dụng mã giảm giá thành công!"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(int planId, string paymentMethod, string? voucherCode, CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null) return Unauthorized();

            var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.PlanId == planId, cancellationToken);
            if (plan == null) return NotFound();

            decimal discountAmount = 0;
            decimal finalAmount = plan.Price;
            int? appliedVoucherId = null;

            if (!string.IsNullOrWhiteSpace(voucherCode))
            {
                var voucherCodeClean = voucherCode.Trim().ToLower();
                var voucher = await _db.Vouchers.FirstOrDefaultAsync(v => v.Code.ToLower() == voucherCodeClean && v.IsActive, cancellationToken);
                if (voucher != null && (voucher.ExpiresAt == null || voucher.ExpiresAt.Value.Date.AddDays(1) >= DateTime.UtcNow))
                {
                    bool isPercent = voucher.DiscountType.StartsWith("Percent", StringComparison.OrdinalIgnoreCase);
                    if (isPercent)
                    {
                        discountAmount = plan.Price * (voucher.DiscountValue / 100m);
                        if (voucher.MaxDiscountCap.HasValue && voucher.MaxDiscountCap.Value > 0)
                        {
                            discountAmount = Math.Min(discountAmount, voucher.MaxDiscountCap.Value);
                        }
                    }
                    else
                    {
                        discountAmount = voucher.DiscountValue;
                    }

                    discountAmount = Math.Min(discountAmount, plan.Price);
                    finalAmount = Math.Max(0, plan.Price - discountAmount);
                    appliedVoucherId = voucher.VoucherId;

                    // Update voucher usage count
                    voucher.UsedCount += 1;
                    _db.VoucherUsages.Add(new VoucherUsage
                    {
                        VoucherId = voucher.VoucherId,
                        StudentId = currentUser.AccountId,
                        DiscountApplied = discountAmount,
                        AppliedAt = DateTime.UtcNow
                    });
                }
            }

            // Create Transaction in DB
            var transaction = new Transaction
            {
                StudentId = currentUser.AccountId,
                PlanId = plan.PlanId,
                VoucherId = appliedVoucherId,
                OriginalAmount = plan.Price,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                PaymentMethod = "PayOS",
                PaymentStatus = TransactionStatusConst.PENDING,
                CreatedAt = DateTime.UtcNow
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync(cancellationToken);

            // Handle 100% free voucher (finalAmount == 0)
            if (finalAmount <= 0)
            {
                transaction.PaymentStatus = TransactionStatusConst.PAID;
                transaction.PaidAt = DateTime.UtcNow;
                transaction.PaymentRef = "VOUCHER_FREE_100";

                // Add or extend student VIP membership directly
                var existingMembership = await _db.StudentMemberships
                    .FirstOrDefaultAsync(m => m.StudentId == currentUser.AccountId && m.IsActive && m.EndDate > DateTime.UtcNow, cancellationToken);

                DateTime startDate = DateTime.UtcNow;
                DateTime endDate = startDate.AddDays(plan.DurationDays);

                if (existingMembership != null)
                {
                    existingMembership.EndDate = existingMembership.EndDate.AddDays(plan.DurationDays);
                }
                else
                {
                    _db.StudentMemberships.Add(new StudentMembership
                    {
                        StudentId = currentUser.AccountId,
                        PlanId = plan.PlanId,
                        TransactionId = transaction.TransactionId,
                        StartDate = startDate,
                        EndDate = endDate,
                        IsActive = true
                    });
                }

                await _db.SaveChangesAsync(cancellationToken);
                return RedirectToAction(nameof(Success), new { transactionId = transaction.TransactionId });
            }

            // Generate unique orderCode
            var orderCode = long.Parse($"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{transaction.TransactionId:D6}");

            // Return and cancel URLs
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var returnUrl = $"{baseUrl}/learn/membership/success?transactionId={transaction.TransactionId}";
            var cancelUrl = $"{baseUrl}/learn/membership/cancelled?transactionId={transaction.TransactionId}";

            // PayOS payment link
            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (int)transaction.FinalAmount,
                Description = "Thanh toán gói thành viên",
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl
            };

            var paymentResponse = await _payment.CreatePaymentLinkAsync(paymentRequest, cancellationToken);

            transaction.PaymentRef = orderCode.ToString();
            await _db.SaveChangesAsync(cancellationToken);

            return Redirect(paymentResponse.CheckoutUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Success(int transactionId, CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var transaction = await _db.Transactions
                .Include(t => t.Plan)
                .Include(t => t.StudentMemberships)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.StudentId == currentUser.AccountId, cancellationToken);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        [HttpGet]
        public async Task<IActionResult> Cancelled(int transactionId, CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var transaction = await _db.Transactions
                .Include(t => t.Plan)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.StudentId == currentUser.AccountId, cancellationToken);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }
    }
}
