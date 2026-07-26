using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreLibrary.Backup
{
    public class BackupService : IBackupService
    {
        private readonly string _connectionString;
        private readonly BackupOptions _options;
        private readonly ILogger<BackupService> _logger;

        public BackupService(
            IConfiguration configuration,
            IOptions<BackupOptions> options,
            ILogger<BackupService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> BackupAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.BackupDirectory))
                throw new InvalidOperationException("Backup:BackupDirectory chưa được cấu hình trong appsettings.json.");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var databaseName = connection.Database;
            var fileName = $"{databaseName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak";
            var backupPath = Path.Combine(_options.BackupDirectory, fileName);

            // BACKUP DATABASE không nhận tham số hoá cho tên DB/path -> escape đơn giản bằng dấu ']]'
            var safeDbName = databaseName.Replace("]", "]]");
            var sql = $"BACKUP DATABASE [{safeDbName}] TO DISK = @BackupPath WITH INIT, COMPRESSION, STATS = 10";

            await using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = 0 // backup DB lớn có thể lâu, để SQL Server tự quản lý timeout
            };
            command.Parameters.AddWithValue("@BackupPath", backupPath);

            _logger.LogInformation("Bắt đầu backup database {Database} ra {Path}", databaseName, backupPath);
            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("Backup database {Database} hoàn tất: {Path}", databaseName, backupPath);

            CleanupOldBackups(databaseName);
            return backupPath;
        }

        private void CleanupOldBackups(string databaseName)
        {
            if (_options.RetentionDays <= 0) return;

            var cutoff = DateTime.UtcNow.AddDays(-_options.RetentionDays);
            var pattern = $"{databaseName}_*.bak";

            foreach (var file in Directory.EnumerateFiles(_options.BackupDirectory, pattern))
            {
                if (File.GetLastWriteTimeUtc(file) >= cutoff) continue;

                try
                {
                    File.Delete(file);
                    _logger.LogInformation("Đã xóa backup cũ quá {Days} ngày: {File}", _options.RetentionDays, file);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không xóa được backup cũ: {File}", file);
                }
            }
        }
    }
}
