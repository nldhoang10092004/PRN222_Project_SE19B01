using CoreLibrary.Const;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreLibrary.Reconciliation
{
    public class TransactionReconciliationService : ITransactionReconciliationService
    {
        private readonly AppDbContext _db;
        private readonly IPaymentService _payment;
        private readonly ReconciliationOptions _options;
        private readonly ILogger<TransactionReconciliationService> _logger;

        public TransactionReconciliationService(
            AppDbContext db,
            IPaymentService payment,
            IOptions<ReconciliationOptions> options,
            ILogger<TransactionReconciliationService> logger)
        {
            _db = db;
            _payment = payment;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<int> ReconcilePendingTransactionsAsync(CancellationToken cancellationToken = default)
        {
            var threshold = DateTime.UtcNow.AddMinutes(-_options.PendingThresholdMinutes);

            var stuckTransactions = await _db.Transactions
                .Where(t => t.PaymentStatus == TransactionStatusConst.PENDING
                    && t.CreatedAt < threshold
                    && t.PaymentRef != null)
                .ToListAsync(cancellationToken);

            var updatedCount = 0;
            foreach (var transaction in stuckTransactions)
            {
                if (!long.TryParse(transaction.PaymentRef, out var orderCode))
                {
                    _logger.LogWarning("PaymentRef không hợp lệ cho TransactionId={TransactionId}: {PaymentRef}",
                        transaction.TransactionId, transaction.PaymentRef);
                    continue;
                }

                try
                {
                    var link = await _payment.GetPaymentLinkAsync(orderCode, cancellationToken);
                    if (await ApplyStatusAsync(transaction, link.Status.ToString(), cancellationToken))
                        updatedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không đồng bộ được trạng thái PayOS cho TransactionId={TransactionId}",
                        transaction.TransactionId);
                }
            }

            if (updatedCount > 0)
            {
                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Đã đồng bộ lại {Count} transaction Pending quá hạn", updatedCount);
            }

            return updatedCount;
        }

        private async Task<bool> ApplyStatusAsync(Transaction transaction, string payOsStatus, CancellationToken cancellationToken)
        {
            if (payOsStatus.Equals("PAID", StringComparison.OrdinalIgnoreCase))
            {
                transaction.PaymentStatus = TransactionStatusConst.PAID;
                transaction.PaidAt = DateTime.UtcNow;

                var existingMembership = await _db.StudentMemberships
                    .FirstOrDefaultAsync(m => m.TransactionId == transaction.TransactionId, cancellationToken);

                if (existingMembership == null)
                {
                    var plan = await _db.SubscriptionPlans
                        .FirstOrDefaultAsync(p => p.PlanId == transaction.PlanId, cancellationToken);

                    if (plan != null)
                    {
                        _db.StudentMemberships.Add(new StudentMembership
                        {
                            StudentId = transaction.StudentId,
                            PlanId = transaction.PlanId,
                            TransactionId = transaction.TransactionId,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
                            IsActive = true
                        });
                    }
                }

                return true;
            }

            if (payOsStatus.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase)
                || payOsStatus.Equals("EXPIRED", StringComparison.OrdinalIgnoreCase))
            {
                transaction.PaymentStatus = payOsStatus.Equals("EXPIRED", StringComparison.OrdinalIgnoreCase)
                    ? TransactionStatusConst.FAILED
                    : TransactionStatusConst.CANCELLED;
                return true;
            }

            // Vẫn PENDING/PROCESSING ở PayOS -> chưa đổi gì, chờ vòng quét tiếp theo
            return false;
        }
    }
}