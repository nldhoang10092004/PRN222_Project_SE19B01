-- =============================================
-- Script: SeedCoursesDummyData.sql
-- Description: Dummy data cho bảng Courses
-- Mỗi level JLPT (N5 → N1) có 4 khóa:
--   Ngữ pháp (Grammar) | Viết (Writing) | Nói (Speaking) | Luyện thi (Exam Training)
-- Idempotent: chạy nhiều lần OK nhờ IF NOT EXISTS theo Title.
-- =============================================

USE [EasyJapaneseDB];
GO

-- Lấy LevelId của 5 level JLPT (giả định InsertDummyData.sql / placement_test_seed.sql đã seed)
DECLARE @N5Id INT = (SELECT TOP 1 LevelId FROM JlptLevels WHERE LevelName = N'N5');
DECLARE @N4Id INT = (SELECT TOP 1 LevelId FROM JlptLevels WHERE LevelName = N'N4');
DECLARE @N3Id INT = (SELECT TOP 1 LevelId FROM JlptLevels WHERE LevelName = N'N3');
DECLARE @N2Id INT = (SELECT TOP 1 LevelId FROM JlptLevels WHERE LevelName = N'N2');
DECLARE @N1Id INT = (SELECT TOP 1 LevelId FROM JlptLevels WHERE LevelName = N'N1');

-- Lấy 1 Mentor bất kỳ làm người phụ trách + CreatedBy (dùng Admin đầu tiên nếu có)
DECLARE @MentorId INT = (SELECT TOP 1 MentorId FROM Mentors);
DECLARE @CreatedBy INT = (
    SELECT TOP 1 MentorId FROM Mentors ORDER BY MentorId
);
IF @CreatedBy IS NULL
    SET @CreatedBy = ISNULL(@MentorId, 1);

DECLARE @Now DATETIME = SYSUTCDATETIME();

-- ============================================================
-- N5 — Sơ cấp
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N5] Ngữ pháp cơ bản - Hán tự đầu tiên')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N5Id, @MentorId,
        N'[N5] Ngữ pháp cơ bản - Hán tự đầu tiên',
        N'Khóa học ngữ pháp tiếng Nhật N5 dành cho người mới bắt đầu. Học các mẫu câu nền tảo như です/ます, trợ từ は・が・を・に, và ~てください. Phù hợp học viên mới học hoặc ôn luyện cho kỳ thi JLPT N5.',
        1, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N5] Viết - Luyện tập viết chữ Hiragana & Katakana')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N5Id, @MentorId,
        N'[N5] Viết - Luyện tập viết chữ Hiragana & Katakana',
        N'Luyện viết đúng nét, đúng thứ tự nét cho toàn bộ bảng chữ cái Hiragana (46 chữ) và Katakana (46 chữ). Bài tập viết tay kèm file PDF tải về, giúp học viên ghi nhớ hình dạng chữ và nét mặc định.',
        1, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N5] Nói - Giao tiếp chào hỏi hằng ngày')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N5Id, @MentorId,
        N'[N5] Nói - Giao tiếp chào hỏi hằng ngày',
        N'Khóa luyện nói cơ bản: tự giới thiệu bản thân, chào hỏi, hỏi thăm sức khỏe, mua đồ ở cửa hàng tiện lợi. Mỗi bài có file nghe mẫu và bài tập bắt chước phát âm giúp tự tin hơn khi gặp người Nhật.',
        1, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N5] Luyện thi JLPT N5 - Đề thi mẫu & chiến lược làm bài')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N5Id, @MentorId,
        N'[N5] Luyện thi JLPT N5 - Đề thi mẫu & chiến lược làm bài',
        N'Khóa tổng ôn và luyện đề JLPT N5. Phân tích cấu trúc đề thi chính thức, chiến lược phân bổ thời gian cho từng phần Từ vựng - Ngữ pháp - Đọc hiểu - Nghe hiểu. Có 3 đề thi thử có chấm điểm và giải thích chi tiết.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

-- ============================================================
-- N4 — Sơ cấp nâng cao
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N4] Ngữ pháp trung cấp - Thể bị động & sai khiến')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N4Id, @MentorId,
        N'[N4] Ngữ pháp trung cấp - Thể bị động & sai khiến',
        N'Hệ thống hóa các mẫu ngữ pháp N4 quan trọng: thể bị động (れる/られる), thể sai khiến (せる/させる), các mẫu ~のに・~ために・~ように. Bài tập áp dụng vào tình huống thực tế kèm đáp án giải thích.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N4] Viết - Viết đoạn văn ngắn & email đơn giản')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N4Id, @MentorId,
        N'[N4] Viết - Viết đoạn văn ngắn & email đơn giản',
        N'Luyện viết đoạn văn 150-300 chữ về chủ đề quen thuộc (gia đình, sở thích, công việc). Thực hành viết email xin việc, email hỏi thông tin. Hướng dẫn cách dùng kính ngữ và bố cục đoạn văn N4.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N4] Nói - Hội thoại đời sống & công sở')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N4Id, @MentorId,
        N'[N4] Nói - Hội thoại đời sống & công sở',
        N'Rèn luyện kỹ năng nói trong các tình huống thường gặp: gọi điện đặt lịch, xin nghỉ phép, hỏi đường, trò chuyện với đồng nghiệp. Mỗi bài có hội thoại mẫu + bài tập đóng vai theo cặp.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N4] Luyện thi JLPT N4 - Đề thi thử & chiến lược')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N4Id, @MentorId,
        N'[N4] Luyện thi JLPT N4 - Đề thi thử & chiến lược',
        N'Ôn tập toàn diện kiến thức N4 và luyện đề thi thử. Phân tích dạng bài hay ra trong kỳ thi thật, mẹo loại trừ đáp án sai phần Nghe hiểu, từ vựng và đọc hiểu. 5 đề thi thử có chấm điểm và giải thích chi tiết.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

-- ============================================================
-- N3 — Trung cấp
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N3] Ngữ pháp trung cấp nâng cao - Mẫu câu tự nhiên')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N3Id, @MentorId,
        N'[N3] Ngữ pháp trung cấp nâng cao - Mẫu câu tự nhiên',
        N'Nắm vững ~てみる・~てしまう・~ようにしている・~ことにする・~わけではない... Các mẫu ngữ pháp N3 giúp nói viết tự nhiên như người Nhật. Bài tập đa dạng theo tình huống kèm video giải thích.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N3] Viết - Luận ngắn 400-800 chữ')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N3Id, @MentorId,
        N'[N3] Viết - Luận ngắn 400-800 chữ',
        N'Luyện viết bài luận ngắn theo các chủ đề thường gặp trong kỳ thi JLPT N3: ý kiến cá nhân, so sánh, phân tích nguyên nhân - kết quả. Hướng dẫn cách dùng từ nối, bố cục và kiểm tra chính tả.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N3] Nói - Thuyết trình & tranh luận ngắn')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N3Id, @MentorId,
        N'[N3] Nói - Thuyết trình & tranh luận ngắn',
        N'Rèn luyện kỹ năng thuyết trình ngắn 3-5 phút và phản biện tranh luận bằng tiếng Nhật. Học cách mở đầu ấn tượng, lập luận logic, kết thúc gọn gàng. Có bài tập thu âm và giáo viên nhận xét.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N3] Luyện thi JLPT N3 - Chiến lược chinh phục N3')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N3Id, @MentorId,
        N'[N3] Luyện thi JLPT N3 - Chiến lược chinh phục N3',
        N'Khóa tổng ôn N3 chuyên sâu: lộ trình 8 tuần, phân tích cấu trúc đề thi mới nhất, mẹo làm bài phần Đọc hiểu đoạn dài, luyện nghe tốc độ thật. 6 đề thi thử có chấm điểm và giải thích từng câu.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

-- ============================================================
-- N2 — Trung cấp cao
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N2] Ngữ pháp N2 - Cấu trúc nâng cao & văn viết')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N2Id, @MentorId,
        N'[N2] Ngữ pháp N2 - Cấu trúc nâng cao & văn viết',
        N'Hệ thống ngữ pháp N2 trọng tâm: ~にもかかわらず・~次第・~ものだから・~というより... Các mẫu hay xuất hiện trong bài đọc hiểu và nghe hiểu kỳ thi JLPT N2. Bài tập vận dụng trong văn viết công việc và email.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N2] Viết - Báo cáo công việc & email kinh doanh')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N2Id, @MentorId,
        N'[N2] Viết - Báo cáo công việc & email kinh doanh',
        N'Luyện viết báo cáo công việc, email xin nghỉ, đề xuất dự án bằng tiếng Nhật. Học keigo (kính ngữ) cần thiết cho môi trường công sở Nhật. Bài mẫu kèm chữa chi tiết từ giáo viên.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N2] Nói - Họp & thuyết trình trong môi trường làm việc')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N2Id, @MentorId,
        N'[N2] Nói - Họp & thuyết trình trong môi trường làm việc',
        N'Rèn luyện kỹ năng phát biểu trong cuộc họp, thuyết trình báo cáo, trao đổi công việc qua điện thoại. Cách dùng kính ngữ 尊敬語 và 謙譲語 phù hợp ngữ cảnh.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N2] Luyện thi JLPT N2 - Lộ trình 12 tuần')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N2Id, @MentorId,
        N'[N2] Luyện thi JLPT N2 - Lộ trình 12 tuần',
        N'Khóa luyện thi N2 toàn diện trong 12 tuần. Phân tích đề thi các năm gần nhất, chiến lược làm bài phần Đọc hiểu dài (800-1000 chữ) và Nghe hiểu tốc độ cao. 8 đề thi thử có giải thích chi tiết.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

-- ============================================================
-- N1 — Cao cấp
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N1] Ngữ pháp N1 - Văn phong học thuật & báo chí')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N1Id, @MentorId,
        N'[N1] Ngữ pháp N1 - Văn phong học thuật & báo chí',
        N'Ngữ pháp N1 trong văn viết học thuật và báo chí Nhật Bản: ~にしては・~だけあって・~にとどまらず・~を兼ねて... Phân tích cấu trúc câu dài, cách đọc hiểu đoạn văn nhiều lớp nghĩa. Bài tập nâng cao theo đề thi thật.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N1] Viết - Luận văn học thuật & báo cáo chuyên ngành')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N1Id, @MentorId,
        N'[N1] Viết - Luận văn học thuật & báo cáo chuyên ngành',
        N'Luyện viết bài nghiên cứu, báo cáo kỹ thuật chuyên ngành bằng tiếng Nhật. Cách trích dẫn, tổng quan tài liệu, lập luận phản biện theo chuẩn học thuật Nhật. Bài mẫu + chữa chi tiết.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N1] Nói - Diễn thuyết & phỏng vấn chuyên môn')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N1Id, @MentorId,
        N'[N1] Nói - Diễn thuyết & phỏng vấn chuyên môn',
        N'Luyện nói trình độ N1: diễn thuyết trước đám đông, phỏng vấn xin việc chuyên môn, thảo luận học thuật. Rèn luyện khả năng diễn đạt mạch lạc, dùng từ vựng học thuật, xử lý tình huống khó.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Title = N'[N1] Luyện thi JLPT N1 - Đề thi & mẹo đạt điểm cao')
BEGIN
    INSERT INTO Courses (LevelId, MentorId, Title, Description, IsFree, IsPublished, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        @N1Id, @MentorId,
        N'[N1] Luyện thi JLPT N1 - Đề thi & mẹo đạt điểm cao',
        N'Khóa tổng ôn N1: chiến lược làm bài phần Đọc hiểu siêu dài, nghe tốc độ thật 1000+ từ/phút, kanji N1 trọng tâm. 10 đề thi thử có giải thích từng câu + đánh giá năng lực đầu ra.',
        0, 1, @CreatedBy, @Now, @Now
    );
END

-- ============================================================
-- Verify
-- ============================================================
SELECT l.LevelName, c.Title, c.IsFree, c.IsPublished
FROM Courses c
JOIN JlptLevels l ON l.LevelId = c.LevelId
WHERE c.Title LIKE N'\[N%]%' ESCAPE '\'
ORDER BY l.SortOrder, c.Title;

SELECT l.LevelName, COUNT(*) AS CourseCount
FROM Courses c
JOIN JlptLevels l ON l.LevelId = c.LevelId
WHERE c.Title LIKE N'\[N%]%' ESCAPE '\'
GROUP BY l.LevelName, l.SortOrder
ORDER BY l.SortOrder;
GO