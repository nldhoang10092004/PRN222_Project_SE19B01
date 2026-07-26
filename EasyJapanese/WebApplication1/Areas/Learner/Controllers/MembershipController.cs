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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(int planId, string paymentMethod, CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null) return Unauthorized();

            var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.PlanId == planId, cancellationToken);
            if (plan == null) return NotFound();

            // Tạo Transaction với status PENDING
            var transaction = new Transaction
            {
                StudentId = currentUser.AccountId,
                PlanId = plan.PlanId,
                OriginalAmount = plan.Price,
                DiscountAmount = 0,
                FinalAmount = plan.Price,
                PaymentMethod = "PayOS",
                PaymentStatus = TransactionStatusConst.PENDING,
                CreatedAt = DateTime.UtcNow
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync(cancellationToken);

            // Generate unique orderCode (timestamp + TransactionId)
            var orderCode = long.Parse($"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{transaction.TransactionId:D6}");

            // Build returnUrl và cancelUrl
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var returnUrl = $"{baseUrl}/learn/membership/success?transactionId={transaction.TransactionId}";
            var cancelUrl = $"{baseUrl}/learn/membership/cancelled?transactionId={transaction.TransactionId}";

            // Tạo PayOS payment link
            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (int)transaction.FinalAmount,
                Description = "Thanh toán gói thành viên",
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl
            };

            var paymentResponse = await _payment.CreatePaymentLinkAsync(paymentRequest, cancellationToken);

            // Lưu orderCode vào PaymentRef
            transaction.PaymentRef = orderCode.ToString();
            await _db.SaveChangesAsync(cancellationToken);

            // Redirect user đến PayOS checkout page
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
