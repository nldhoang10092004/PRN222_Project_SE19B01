namespace CoreLibrary.Reconciliation
{
    public class ReconciliationOptions
    {
        public const string SectionName = "Reconciliation";

        /// <summary>Số phút transaction ở trạng thái Pending thì coi là "treo", cần đồng bộ lại với PayOS.</summary>
        public int PendingThresholdMinutes { get; set; } = 30;
    }
}
