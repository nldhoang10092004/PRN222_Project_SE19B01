namespace CoreLibrary.Backup
{
    public class BackupOptions
    {
        public const string SectionName = "Backup";

        /// <summary>Thư mục lưu file .bak — PHẢI là đường dẫn tuyệt đối và tồn tại trên máy chạy SQL Server (không phải máy chạy worker, nếu khác máy).</summary>
        public string BackupDirectory { get; set; } = string.Empty;

        /// <summary>Số ngày giữ lại file backup cũ, quá hạn sẽ tự xóa.</summary>
        public int RetentionDays { get; set; } = 7;
    }
}
