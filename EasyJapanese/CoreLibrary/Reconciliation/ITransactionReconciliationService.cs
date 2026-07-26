namespace CoreLibrary.Reconciliation
{
    public interface ITransactionReconciliationService
    {
        /// <summary>Quét Transaction đang Pending quá lâu, đồng bộ trạng thái thật từ PayOS.</summary>
        /// <returns>Số transaction đã được cập nhật trạng thái.</returns>
        Task<int> ReconcilePendingTransactionsAsync(CancellationToken cancellationToken = default);
    }
}
