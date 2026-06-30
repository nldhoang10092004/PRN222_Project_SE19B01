using CoreLibrary.Authentication;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public MembershipController(AppDbContext db, IAuthenticationService auth)
        {
            _db = db;
            _auth = auth;
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

            // MOCK PAYMENT PROCESS
            var transaction = new Transaction
            {
                StudentId = currentUser.AccountId,
                PlanId = plan.PlanId,
                OriginalAmount = plan.Price,
                DiscountAmount = 0,
                FinalAmount = plan.Price,
                PaymentMethod = paymentMethod,
                PaymentStatus = "Completed",
                PaidAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            
            _db.Transactions.Add(transaction);

            // Create membership
            var membership = new StudentMembership
            {
                StudentId = currentUser.AccountId,
                PlanId = plan.PlanId,
                Transaction = transaction,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
                IsActive = true
            };

            _db.StudentMemberships.Add(membership);
            await _db.SaveChangesAsync(cancellationToken);

            // Create a fake payment log (if the entity exists, but let's skip to keep it simple)

            // Redirect back to dashboard where they will now see State 2 (Placement test)
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
