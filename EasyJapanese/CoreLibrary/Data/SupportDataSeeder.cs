using System;
using System.Collections.Generic;
using System.Linq;
using CoreLibrary.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreLibrary.Data
{
    public static class SupportDataSeeder
    {
        public static void SeedSupportData(AppDbContext context)
        {
            try
            {
                // Ensure SQL tables exist in SQL Server
                string createTablesSql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SupportTickets')
BEGIN
    CREATE TABLE [SupportTickets] (
        [TicketId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserEmail] NVARCHAR(255) NOT NULL,
        [UserFullName] NVARCHAR(255) NULL,
        [Category] NVARCHAR(100) NOT NULL DEFAULT N'Hỗ trợ trực tuyến',
        [Subject] NVARCHAR(255) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT N'Open',
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SupportMessages')
BEGIN
    CREATE TABLE [SupportMessages] (
        [MessageId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TicketId] INT NOT NULL,
        [Sender] NVARCHAR(50) NOT NULL DEFAULT N'User',
        [MessageText] NVARCHAR(MAX) NOT NULL,
        [SentAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_SupportMessages_SupportTickets] FOREIGN KEY ([TicketId]) REFERENCES [SupportTickets]([TicketId]) ON DELETE CASCADE
    );
END
";
                context.Database.ExecuteSqlRaw(createTablesSql);

                if (!context.SupportTickets.Any())
                {
                    var ticket1 = new SupportTicket
                    {
                        UserFullName = "Nguyễn Văn An",
                        UserEmail = "an.nguyen@gmail.com",
                        Category = "Thanh toán",
                        Subject = "Không tự động nâng cấp VIP sau khi quét mã thanh toán",
                        Status = "Open",
                        CreatedAt = DateTime.UtcNow.AddHours(-3),
                        UpdatedAt = DateTime.UtcNow.AddHours(-1)
                    };

                    var ticket2 = new SupportTicket
                    {
                        UserFullName = "Trần Thị Bình",
                        UserEmail = "binh.tran@yahoo.com",
                        Category = "Tài khoản",
                        Subject = "Lỗi không đăng nhập được bằng Google",
                        Status = "InProgress",
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        UpdatedAt = DateTime.UtcNow.AddHours(-4)
                    };

                    var ticket3 = new SupportTicket
                    {
                        UserFullName = "Phạm Hồng Đăng",
                        UserEmail = "dangpham@outlook.com",
                        Category = "Nội dung bài học",
                        Subject = "Sai đáp án bài tập trắc nghiệm Kanji N3",
                        Status = "Resolved",
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        UpdatedAt = DateTime.UtcNow.AddDays(-2)
                    };

                    context.SupportTickets.AddRange(ticket1, ticket2, ticket3);
                    context.SaveChanges();

                    context.SupportMessages.AddRange(
                        new SupportMessage
                        {
                            TicketId = ticket1.TicketId,
                            Sender = "User",
                            MessageText = "Chào Ad, mình vừa thực hiện thanh toán chuyển khoản gói 3 tháng qua cổng VNPAY lúc 9h sáng nay.",
                            SentAt = DateTime.UtcNow.AddHours(-3)
                        },
                        new SupportMessage
                        {
                            TicketId = ticket1.TicketId,
                            Sender = "User",
                            MessageText = "Tiền trong tài khoản ngân hàng của mình đã bị trừ 199.000đ rồi, mã giao dịch là VP120349. Nhưng tài khoản trên web vẫn báo là Basic. Mong Ad kích hoạt hộ nhé.",
                            SentAt = DateTime.UtcNow.AddHours(-1)
                        },

                        new SupportMessage
                        {
                            TicketId = ticket2.TicketId,
                            Sender = "User",
                            MessageText = "Chào admin, em dùng tài khoản Google để đăng nhập nhưng cứ bị báo lỗi Auth Timeout hoài.",
                            SentAt = DateTime.UtcNow.AddDays(-1)
                        },
                        new SupportMessage
                        {
                            TicketId = ticket2.TicketId,
                            Sender = "Admin",
                            MessageText = "Chào em, em thử đăng nhập bằng trình duyệt ẩn danh hoặc xóa cookie xem có được không nhé.",
                            SentAt = DateTime.UtcNow.AddHours(-5)
                        },
                        new SupportMessage
                        {
                            TicketId = ticket2.TicketId,
                            Sender = "User",
                            MessageText = "Em đã xóa cookie và thử lại rồi nhưng vẫn báo lỗi như cũ ạ, hình như do server kết nối Google có vấn đề.",
                            SentAt = DateTime.UtcNow.AddHours(-4)
                        },

                        new SupportMessage
                        {
                            TicketId = ticket3.TicketId,
                            Sender = "User",
                            MessageText = "Ở bài trắc nghiệm N3 phần Kanji ôn tập bài 4, đáp án câu 5 đang bị sai. Mong thầy cô xem lại.",
                            SentAt = DateTime.UtcNow.AddDays(-3)
                        },
                        new SupportMessage
                        {
                            TicketId = ticket3.TicketId,
                            Sender = "Admin",
                            MessageText = "Cảm ơn bạn đã đóng góp. Đội ngũ học thuật đã chỉnh sửa lại đáp án chính xác của câu hỏi này.",
                            SentAt = DateTime.UtcNow.AddDays(-2)
                        }
                    );
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error seeding support data: " + ex.Message);
            }
        }
    }
}
