-- =============================================
-- Script: CreateLessonMaterials.sql
-- Description: Tạo bảng LessonMaterials (tài liệu đính kèm bài học)
--              và seed dữ liệu mẫu cho Lesson 1-5
-- Idempotent: có thể chạy lại nhiều lần an toàn
-- =============================================

USE [EasyJapaneseDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LessonMaterials')
BEGIN
    CREATE TABLE LessonMaterials (
        MaterialId INT IDENTITY(1,1) PRIMARY KEY,
        LessonId INT NOT NULL,
        Title NVARCHAR(255) NOT NULL,
        Url NVARCHAR(500) NOT NULL,
        FileType NVARCHAR(20) NULL,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT FK_LessonMaterials_Lessons FOREIGN KEY (LessonId)
            REFERENCES Lessons(LessonId) ON DELETE CASCADE
    );

    CREATE INDEX IX_LessonMaterials_Lesson ON LessonMaterials(LessonId);
END
GO

DECLARE @Now DATETIME2 = SYSUTCDATETIME();

-- Chỉ seed nếu Lesson đó chưa có tài liệu nào
IF NOT EXISTS (SELECT 1 FROM LessonMaterials WHERE LessonId = 1)
BEGIN
    INSERT INTO LessonMaterials (LessonId, Title, Url, FileType, SortOrder, CreatedAt)
    VALUES
        (1, N'Slide bài giảng - Bài 1', N'https://example.com/materials/lesson1-slide.pdf', N'slide', 1, @Now),
        (1, N'Tài liệu ngữ pháp bổ sung', N'https://example.com/materials/lesson1-grammar.pdf', N'pdf', 2, @Now),
        (1, N'Link tham khảo thêm', N'https://example.com/reference/lesson1', N'link', 3, @Now);
END

IF NOT EXISTS (SELECT 1 FROM LessonMaterials WHERE LessonId = 2)
BEGIN
    INSERT INTO LessonMaterials (LessonId, Title, Url, FileType, SortOrder, CreatedAt)
    VALUES
        (2, N'Slide bài giảng - Bài 2', N'https://example.com/materials/lesson2-slide.pdf', N'slide', 1, @Now),
        (2, N'Bài tập PDF - Bài 2', N'https://example.com/materials/lesson2-exercise.pdf', N'pdf', 2, @Now);
END

IF NOT EXISTS (SELECT 1 FROM LessonMaterials WHERE LessonId = 3)
BEGIN
    INSERT INTO LessonMaterials (LessonId, Title, Url, FileType, SortOrder, CreatedAt)
    VALUES
        (3, N'Slide bài giảng - Bài 3', N'https://example.com/materials/lesson3-slide.pdf', N'slide', 1, @Now),
        (3, N'Tài liệu Kanji bổ sung', N'https://example.com/materials/lesson3-kanji.pdf', N'pdf', 2, @Now),
        (3, N'Link luyện nghe thêm', N'https://example.com/reference/lesson3-listening', N'link', 3, @Now);
END

IF NOT EXISTS (SELECT 1 FROM LessonMaterials WHERE LessonId = 4)
BEGIN
    INSERT INTO LessonMaterials (LessonId, Title, Url, FileType, SortOrder, CreatedAt)
    VALUES
        (4, N'Slide bài giảng - Bài 4', N'https://example.com/materials/lesson4-slide.pdf', N'slide', 1, @Now);
END

IF NOT EXISTS (SELECT 1 FROM LessonMaterials WHERE LessonId = 5)
BEGIN
    INSERT INTO LessonMaterials (LessonId, Title, Url, FileType, SortOrder, CreatedAt)
    VALUES
        (5, N'Slide bài giảng - Bài 5', N'https://example.com/materials/lesson5-slide.pdf', N'slide', 1, @Now),
        (5, N'Tài liệu đọc hiểu bổ sung', N'https://example.com/materials/lesson5-reading.pdf', N'pdf', 2, @Now);
END
GO
