namespace CoreLibrary.Backup
{
    public interface IBackupService
    {
        /// <summary>Backup toàn bộ database (BACKUP DATABASE) ra file .bak, dọn backup cũ quá hạn retention.</summary>
        /// <returns>Đường dẫn file .bak vừa tạo.</returns>
        Task<string> BackupAsync(CancellationToken cancellationToken = default);
    }
}
