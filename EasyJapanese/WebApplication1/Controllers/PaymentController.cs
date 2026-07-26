using CoreLibrary.Const;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS.Models.Webhooks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IPaymentService _payment;

        public PaymentController(AppDbContext db, IPaymentService payment)
        {
            _db = db;
            _payment = payment;
        }

        [HttpPost]
        [Route("/payment/webhook")]
        public async Task<IActionResult> Webhook([FromBody] Webhook webhook, CancellationToken cancellationToken)
        {
            try
            {
                // Verify webhook signature từ PayOS
                var webhookData = await _payment.VerifyWebhookAsync(webhook, cancellationToken);

                // Tìm transaction theo orderCode (lưu trong PaymentRef)
                var orderCode = webhookData.OrderCode.ToString();
                var transaction = await _db.Transactions
                    .FirstOrDefaultAsync(t => t.PaymentRef == orderCode);

                if (transaction == null)
                {
                    return NotFound(new { message = "Transaction không tồn tại" });
                }

                // Chỉ xử lý nếu transaction đang PENDING
                if (transaction.PaymentStatus != TransactionStatusConst.PENDING)
                {
                    return Ok(new { message = "Transaction đã được xử lý trước đó" });
                }

                // Kiểm tra trạng thái thanh toán từ PayOS
                if (webhookData.Code == "00" && webhookData.Description2 == "success")
                {
                    // Thanh toán thành công
                    transaction.PaymentStatus = TransactionStatusConst.PAID;
                    transaction.PaidAt = DateTime.UtcNow;

                    // Tạo/activate membership
                    var existingMembership = await _db.StudentMemberships
                        .FirstOrDefaultAsync(m => m.TransactionId == transaction.TransactionId);

                    if (existingMembership == null)
                    {
                        var plan = await _db.SubscriptionPlans
                            .FirstOrDefaultAsync(p => p.PlanId == transaction.PlanId);

                        if (plan != null)
                        {
                            var membership = new StudentMembership
                            {
                                StudentId = transaction.StudentId,
                                PlanId = transaction.PlanId,
                                TransactionId = transaction.TransactionId,
                                StartDate = DateTime.UtcNow,
                                EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
                                IsActive = true
                            };

                            _db.StudentMemberships.Add(membership);
                        }
                    }
                }
                else
                {
                    // Thanh toán thất bại
                    transaction.PaymentStatus = TransactionStatusConst.FAILED;
                }

                await _db.SaveChangesAsync();

                return Ok(new { message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                // Log error (có thể thêm ILogger sau)
                return BadRequest(new { message = "Webhook verification failed", error = ex.Message });
            }
        }
    }
}
