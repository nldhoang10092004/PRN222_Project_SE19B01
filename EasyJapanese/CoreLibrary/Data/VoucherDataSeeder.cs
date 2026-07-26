using System;
using System.Linq;
using CoreLibrary.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreLibrary.Data
{
    public static class VoucherDataSeeder
    {
        public static void SeedVoucherData(AppDbContext context)
        {
            try
            {
                // Drop old strict SQL Server CHECK constraint on DiscountType if present
                string fixConstraintSql = @"
DECLARE @chkName NVARCHAR(200);
SELECT TOP 1 @chkName = name FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID('Vouchers') AND name LIKE '%Discou%';
IF @chkName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE Vouchers DROP CONSTRAINT [' + @chkName + ']');
END
";
                try
                {
                    context.Database.ExecuteSqlRaw(fixConstraintSql);
                }
                catch (Exception)
                {
                    // Ignore if constraint already dropped or sys views not accessible
                }

                // Ensure a valid Admin fallback exists for CreatedBy FK
                var admin = context.Admins.FirstOrDefault();
                int adminId;
                if (admin == null)
                {
                    var newAdmin = new Admin
                    {
                        AdminId = 1,
                        FullName = "System Admin",
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Admins.Add(newAdmin);
                    context.SaveChanges();
                    adminId = 1;
                }
                else
                {
                    adminId = admin.AdminId;
                }

                if (!context.Vouchers.Any())
                {
                    var v1 = new Voucher
                    {
                        Code = "HIJAPAN20",
                        Description = "Mã ưu đãi độc quyền giảm 20% cho tất cả các gói VIP Hi Japan!",
                        DiscountType = "Percent",
                        DiscountValue = 20,
                        MaxDiscountCap = 100000,
                        MinOrderValue = 50000,
                        MaxUsesTotal = 500,
                        MaxUsesPerUser = 1,
                        UsedCount = 5,
                        StartsAt = DateTime.UtcNow.AddDays(-30),
                        ExpiresAt = DateTime.UtcNow.AddYears(1),
                        IsActive = true,
                        CreatedBy = adminId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var v2 = new Voucher
                    {
                        Code = "JLPT50K",
                        Description = "Giảm trực tiếp 50.000đ khi đăng ký học viên VIP",
                        DiscountType = "Fixed",
                        DiscountValue = 50000,
                        MaxDiscountCap = 50000,
                        MinOrderValue = 100000,
                        MaxUsesTotal = 200,
                        MaxUsesPerUser = 1,
                        UsedCount = 12,
                        StartsAt = DateTime.UtcNow.AddDays(-30),
                        ExpiresAt = DateTime.UtcNow.AddYears(1),
                        IsActive = true,
                        CreatedBy = adminId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var v3 = new Voucher
                    {
                        Code = "VIPN3PRO",
                        Description = "Mã chào mừng thành viên mới giảm 30%",
                        DiscountType = "Percent",
                        DiscountValue = 30,
                        MaxDiscountCap = 150000,
                        MinOrderValue = 100000,
                        MaxUsesTotal = 100,
                        MaxUsesPerUser = 1,
                        UsedCount = 2,
                        StartsAt = DateTime.UtcNow.AddDays(-10),
                        ExpiresAt = DateTime.UtcNow.AddYears(1),
                        IsActive = true,
                        CreatedBy = adminId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.Vouchers.AddRange(v1, v2, v3);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error seeding voucher data: " + ex.Message);
            }
        }
    }
}
