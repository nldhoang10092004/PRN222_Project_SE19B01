using Microsoft.EntityFrameworkCore;

namespace CoreLibrary.Data
{
    // Partial class để bổ sung cho AppDbContext scaffolded:
    // - Bỏ connection string hardcoded
    // - Dùng cú pháp "Name=..." để EF tự đọc từ app configuration
    //   (đăng ký qua AppDbContextExtensions.AddAppDbContext).
    public partial class AppDbContext
    {
        public const string DefaultConnectionName = "DefaultConnection";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer($"Name={DefaultConnectionName}");
            }
        }
    }
}
