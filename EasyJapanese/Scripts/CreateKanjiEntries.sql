-- =============================================
-- Script: CreateKanjiEntries.sql
-- Description: Tạo bảng KanjiEntries (thư viện tra cứu Kanji, độc lập theo JLPT Level)
--              và KanjiExamples (từ ví dụ ghép từ, kiểu Mazii)
-- Idempotent: dùng IF NOT EXISTS trước khi tạo bảng
-- =============================================

USE [EasyJapaneseDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'KanjiEntries')
BEGIN
    CREATE TABLE KanjiEntries (
        KanjiId        INT IDENTITY(1,1) PRIMARY KEY,
        LevelId        INT NOT NULL,
        Character      NVARCHAR(10)  NOT NULL,
        Meaning        NVARCHAR(300) NULL,
        OnYomi         NVARCHAR(200) NULL,
        KunYomi        NVARCHAR(200) NULL,
        StrokeCount    INT NULL,
        StrokeOrderUrl NVARCHAR(500) NULL,  -- URL ảnh GIF minh họa thứ tự nét viết
        CreatedAt      DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),

        CONSTRAINT FK_KanjiEntries_Level FOREIGN KEY (LevelId)
            REFERENCES JlptLevels(LevelId)
    );

    CREATE INDEX IX_KanjiEntries_LevelId ON KanjiEntries(LevelId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'KanjiExamples')
BEGIN
    CREATE TABLE KanjiExamples (
        ExampleId  INT IDENTITY(1,1) PRIMARY KEY,
        KanjiId    INT NOT NULL,
        Word       NVARCHAR(50)  NOT NULL,  -- Từ ghép chứa Kanji, ví dụ: 先生
        Reading    NVARCHAR(100) NULL,      -- Cách đọc furigana, ví dụ: せんせい
        Meaning    NVARCHAR(300) NULL,      -- Nghĩa tiếng Việt, ví dụ: giáo viên
        SortOrder  INT NOT NULL DEFAULT (0),

        CONSTRAINT FK_KanjiExamples_Kanji FOREIGN KEY (KanjiId)
            REFERENCES KanjiEntries(KanjiId) ON DELETE CASCADE
    );

    CREATE INDEX IX_KanjiExamples_KanjiId ON KanjiExamples(KanjiId);
END
GO
