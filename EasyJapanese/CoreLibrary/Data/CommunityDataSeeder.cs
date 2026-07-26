using System;
using System.Collections.Generic;
using System.Linq;
using CoreLibrary.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreLibrary.Data
{
    public static class CommunityDataSeeder
    {
        public static void SeedCommunityData(AppDbContext context)
        {
            try
            {
                // Ensure tables exist in SQL Server
                string createTablesSql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CommunityPosts')
BEGIN
    CREATE TABLE [CommunityPosts] (
        [PostId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [AuthorId] INT NOT NULL,
        [AuthorName] NVARCHAR(100) NULL,
        [AuthorRole] NVARCHAR(20) NOT NULL DEFAULT 'Student',
        [Title] NVARCHAR(255) NOT NULL,
        [Category] NVARCHAR(50) NOT NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [ImageUrl] NVARCHAR(MAX) NULL,
        [LikeCount] INT NOT NULL DEFAULT 0,
        [ViewCount] INT NOT NULL DEFAULT 0,
        [IsApproved] BIT NOT NULL DEFAULT 1,
        [IsPinned] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CommunityComments')
BEGIN
    CREATE TABLE [CommunityComments] (
        [CommentId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PostId] INT NOT NULL,
        [AuthorId] INT NOT NULL,
        [AuthorName] NVARCHAR(100) NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_CommunityComments_CommunityPosts] FOREIGN KEY ([PostId]) REFERENCES [CommunityPosts]([PostId]) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CommunityLikes')
BEGIN
    CREATE TABLE [CommunityLikes] (
        [LikeId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PostId] INT NOT NULL,
        [AccountId] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_CommunityLikes_CommunityPosts] FOREIGN KEY ([PostId]) REFERENCES [CommunityPosts]([PostId]) ON DELETE CASCADE
    );
END
";
                context.Database.ExecuteSqlRaw(createTablesSql);

                if (!context.CommunityPosts.Any())
                {
                    var post1 = new CommunityPost
                    {
                        AuthorId = 1,
                        AuthorName = "Cô Nguyễn Thị Lan",
                        AuthorRole = "Teacher",
                        Title = "Bí quyết tự học JLPT N3 trong 6 tháng cho người đi làm",
                        Category = "Kinh nghiệm học",
                        Content = @"Làm thế nào để cân bằng giữa công việc bận rộn và việc học tiếng Nhật? Bài viết chia sẻ lộ trình chi tiết giúp bạn vượt qua kỳ thi JLPT N3 một cách nhẹ nhàng — từ cách chọn tài liệu đến quản lý thời gian hiệu quả.

### 1. Lập kế hoạch học tập thực tế
Mỗi ngày dành ít nhất 45 - 60 phút chia làm 2 khung giờ:
- **Buổi sáng (20 phút)**: Ôn tập 10 từ vựng + 2 chữ Kanji mới trên ứng dụng Flashcard.
- **Buổi tối (40 phút)**: Học 2 cấu trúc ngữ pháp N3 và làm 1 bài luyện đọc ngắn.

### 2. Chọn tài liệu trọng tâm
- **Từ vựng & Kanji**: Mimikara Oboeru N3 / Shinkanzen Master N3.
- **Ngữ pháp**: Try! N3 (giải thích rất dễ hiểu kèm ví dụ thực tế).
- **Choukai (Nghe hiểu)**: Nghe podcast tiếng Nhật tin tức ngắn hàng ngày trên đường đi làm.

Chúc các bạn ôn luyện thật tốt và đạt kết quả cao trong kỳ thi sắp tới!",
                        ImageUrl = "https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e?q=80&w=900&auto=format&fit=crop",
                        LikeCount = 42,
                        ViewCount = 1580,
                        IsApproved = true,
                        IsPinned = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-15),
                        UpdatedAt = DateTime.UtcNow.AddDays(-15)
                    };

                    var post2 = new CommunityPost
                    {
                        AuthorId = 2,
                        AuthorName = "Trần Minh Anh",
                        AuthorRole = "Student",
                        Title = "Khám phá văn hóa đền chùa dịp năm mới tại Nhật Bản",
                        Category = "Văn hóa",
                        Content = @"Hatsumoude (初詣) là tục lệ đi lễ đền chùa đầu năm mới tại Nhật Bản. 

Những quy tắc cần nhớ khi đến thăm đền thờ Thần đạo (Jinja):
1. Rửa tay và súc miệng tại khu vực Chozuya trước khi vào viếng.
2. Ném đồng xu 5 yên (Go-en: mang ý nghĩa duyên lành) vào hòm công đức Saisen-bako.
3. Thực hiện đúng nghi thức **2 vái - 2 vỗ tay - 1 vái**:
   - Cúi đầu 2 lần sâu 90 độ.
   - Vỗ tay 2 lần, chắp tay cầu nguyện.
   - Cúi đầu 1 lần cuối để hoàn tất lễ.",
                        ImageUrl = "https://images.unsplash.com/photo-1528164344705-47542687000d?q=80&w=900&auto=format&fit=crop",
                        LikeCount = 28,
                        ViewCount = 940,
                        IsApproved = true,
                        IsPinned = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        UpdatedAt = DateTime.UtcNow.AddDays(-10)
                    };

                    var post3 = new CommunityPost
                    {
                        AuthorId = 3,
                        AuthorName = "Lê Hoàng Nam",
                        AuthorRole = "Student",
                        Title = "50 Từ vựng chủ đề ẩm thực Nhật Bản nhất định phải biết",
                        Category = "Từ vựng",
                        Content = @"Đi ăn nhà hàng Nhật không lo không biết gọi món với danh sách từ vựng thiết yếu này:

- ラーメン (Ramen): Mỳ Ramen
- 寿司 (Sushi): Món Sushi
- 天ぷら (Tempura): Đồ chiên giòn
- お会計 (OKaikei): Tính tiền / Hóa đơn
- おすすめ (Osusume): Món khuyến nghị của quán
- いらっしゃいませ! (Irasshaimase!): Xin chào quý khách!",
                        ImageUrl = "https://images.unsplash.com/photo-1558865869-c93f6f8482af?q=80&w=900&auto=format&fit=crop",
                        LikeCount = 35,
                        ViewCount = 1120,
                        IsApproved = true,
                        IsPinned = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-7),
                        UpdatedAt = DateTime.UtcNow.AddDays(-7)
                    };

                    var post4 = new CommunityPost
                    {
                        AuthorId = 4,
                        AuthorName = "Phạm Hải Đăng",
                        AuthorRole = "Teacher",
                        Title = "Cách viết Email tiếng Nhật chuẩn Business",
                        Category = "Công việc",
                        Content = @"Cấu trúc email chuẩn mực giúp bạn ghi điểm tuyệt đối trong mắt đối tác và đồng nghiệp người Nhật:

1. **Tiêu đề (件名)**: Rõ ràng, kèm tên công ty/họ tên.
2. **Lời chào mở đầu**: いつもお世話になっております (Cảm ơn anh/chị đã luôn giúp đỡ).
3. **Giới thiệu bản thân**: xx's company の xx と申します.
4. **Nội dung chính**: Trình bày ngắn gọn, dùng kính ngữ Sonkeigo / Kenjougo hợp lý.
5. **Lời chúc kết bài**: よろしくお願いいたします.
6. **Chữ ký (署名)**: Tên, chức vụ, SĐT, Email.",
                        ImageUrl = "https://images.unsplash.com/photo-1542051812-ba32e6c891a6?q=80&w=900&auto=format&fit=crop",
                        LikeCount = 56,
                        ViewCount = 2040,
                        IsApproved = true,
                        IsPinned = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        UpdatedAt = DateTime.UtcNow.AddDays(-5)
                    };

                    var post5 = new CommunityPost
                    {
                        AuthorId = 5,
                        AuthorName = "Vũ Bảo Ngọc",
                        AuthorRole = "Student",
                        Title = "Học Kanji theo bộ thủ — phương pháp hiệu quả nhất 2025",
                        Category = "Kanji",
                        Content = @"Thay vì học thuộc lòng từng chữ Kanji riêng lẻ, học theo bộ thủ giúp bạn nhớ lâu hơn và dễ dàng suy đoán nghĩa của từ mới chưa từng gặp.

Ví dụ:
- Bộ Thủy (氵): Liên quan đến nước (水, 江, 海, 泳).
- Bộ Mộc (木): Liên quan đến cây cối (林, 森, 樹, 枝).
- Bộ Tâm (心/忄): Liên quan đến tình cảm, suy nghĩ (情, 愛, 怒, 愁).",
                        ImageUrl = "https://images.unsplash.com/photo-1528360983277-13d401cdc186?q=80&w=900&auto=format&fit=crop",
                        LikeCount = 19,
                        ViewCount = 780,
                        IsApproved = true,
                        IsPinned = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        UpdatedAt = DateTime.UtcNow.AddDays(-3)
                    };

                    var post6 = new CommunityPost
                    {
                        AuthorId = 6,
                        AuthorName = "Đặng Thị Thảo",
                        AuthorRole = "Student",
                        Title = "Ngữ pháp て-form: Ứng dụng thực tế trong hội thoại hàng ngày",
                        Category = "Ngữ pháp",
                        Content = @"Thể て (Te-form) là một trong những cấu trúc ngữ pháp nền tảng quan trọng nhất trong tiếng Nhật:

1. **Nối hành động**: V1て、V2 (Làm V1 rồi làm V2).
2. **Xin phép**: Vてもいいです (Có thể làm V không?).
3. **Cấm đoán**: Vてはいけません (Không được làm V).
4. **Nhờ vả lịch sự**: Vてください (Xin hãy làm V).
5. **Diễn tả trạng thái tiếp diễn**: Vています (Đang làm V).",
                        ImageUrl = "https://images.unsplash.com/photo-1542931287-023b922fa89b?q=80&w=900&auto=format&fit=crop",
                        LikeCount = 31,
                        ViewCount = 1350,
                        IsApproved = true,
                        IsPinned = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        UpdatedAt = DateTime.UtcNow.AddDays(-1)
                    };

                    context.CommunityPosts.AddRange(post1, post2, post3, post4, post5, post6);
                    context.SaveChanges();

                    // Seed sample comments
                    context.CommunityComments.AddRange(
                        new CommunityComment
                        {
                            PostId = post1.PostId,
                            AuthorId = 2,
                            AuthorName = "Trần Minh Anh",
                            Content = "Bài viết rất hữu ích ạ! Cảm ơn cô Lan đã chia sẻ lộ trình chi tiết.",
                            CreatedAt = DateTime.UtcNow.AddDays(-14)
                        },
                        new CommunityComment
                        {
                            PostId = post1.PostId,
                            AuthorId = 3,
                            AuthorName = "Lê Hoàng Nam",
                            Content = "Em vừa mua quyển Try! N3 theo tư vấn của cô, đọc giải thích thích lắm ạ.",
                            CreatedAt = DateTime.UtcNow.AddDays(-12)
                        },
                        new CommunityComment
                        {
                            PostId = post4.PostId,
                            AuthorId = 5,
                            AuthorName = "Vũ Bảo Ngọc",
                            Content = "Mẫu email Business này đúng thứ em đang cần tìm để viết mail cho sếp Nhật!",
                            CreatedAt = DateTime.UtcNow.AddDays(-4)
                        }
                    );
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error seeding community data: " + ex.Message);
            }
        }
    }
}
