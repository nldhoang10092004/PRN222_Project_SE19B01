-- =============================================
-- Script: SeedKanjiEntriesSample.sql
-- Description: Seed vài Kanji N5 mẫu để test tính năng tra cứu
-- Idempotent: bỏ qua nếu Character đã tồn tại ở đúng Level
-- =============================================

USE [EasyJapaneseDB];
GO

DECLARE @N5 INT = (SELECT LevelId FROM JlptLevels WHERE LevelName = N'N5');

-- 私 (Watashi - Tôi)
IF NOT EXISTS (SELECT 1 FROM KanjiEntries WHERE Character = N'私' AND LevelId = @N5)
BEGIN
    INSERT INTO KanjiEntries (LevelId, Character, Meaning, OnYomi, KunYomi, StrokeCount, StrokeOrderUrl)
    VALUES (@N5, N'私', N'Tôi, riêng tư', N'shi', N'watashi', 7, NULL);

    INSERT INTO KanjiExamples (KanjiId, Word, Reading, Meaning, SortOrder)
    VALUES
        (SCOPE_IDENTITY(), N'私', N'わたし', N'Tôi', 1),
        (SCOPE_IDENTITY(), N'私立', N'しりつ', N'Tư lập (trường tư)', 2);
END
GO

DECLARE @N5 INT = (SELECT LevelId FROM JlptLevels WHERE LevelName = N'N5');

-- 人 (Hito/Jin - Người)
IF NOT EXISTS (SELECT 1 FROM KanjiEntries WHERE Character = N'人' AND LevelId = @N5)
BEGIN
    DECLARE @KanjiId2 INT;

    INSERT INTO KanjiEntries (LevelId, Character, Meaning, OnYomi, KunYomi, StrokeCount, StrokeOrderUrl)
    VALUES (@N5, N'人', N'Người', N'jin, nin', N'hito', 2, NULL);

    SET @KanjiId2 = SCOPE_IDENTITY();

    INSERT INTO KanjiExamples (KanjiId, Word, Reading, Meaning, SortOrder)
    VALUES
        (@KanjiId2, N'日本人', N'にほんじん', N'Người Nhật', 1),
        (@KanjiId2, N'人気', N'にんき', N'Nổi tiếng, được yêu thích', 2);
END
GO

DECLARE @N5 INT = (SELECT LevelId FROM JlptLevels WHERE LevelName = N'N5');

-- 先 (Sen - Trước)
IF NOT EXISTS (SELECT 1 FROM KanjiEntries WHERE Character = N'先' AND LevelId = @N5)
BEGIN
    DECLARE @KanjiId3 INT;

    INSERT INTO KanjiEntries (LevelId, Character, Meaning, OnYomi, KunYomi, StrokeCount, StrokeOrderUrl)
    VALUES (@N5, N'先', N'Trước, sớm', N'sen', N'saki', 6, NULL);

    SET @KanjiId3 = SCOPE_IDENTITY();

    INSERT INTO KanjiExamples (KanjiId, Word, Reading, Meaning, SortOrder)
    VALUES
        (@KanjiId3, N'先生', N'せんせい', N'Giáo viên, thầy/cô', 1),
        (@KanjiId3, N'先週', N'せんしゅう', N'Tuần trước', 2);
END
GO

DECLARE @N5 INT = (SELECT LevelId FROM JlptLevels WHERE LevelName = N'N5');

-- 生 (Sei - Sinh)
IF NOT EXISTS (SELECT 1 FROM KanjiEntries WHERE Character = N'生' AND LevelId = @N5)
BEGIN
    DECLARE @KanjiId4 INT;

    INSERT INTO KanjiEntries (LevelId, Character, Meaning, OnYomi, KunYomi, StrokeCount, StrokeOrderUrl)
    VALUES (@N5, N'生', N'Sinh, sống', N'sei', N'i-kiru', 5, NULL);

    SET @KanjiId4 = SCOPE_IDENTITY();

    INSERT INTO KanjiExamples (KanjiId, Word, Reading, Meaning, SortOrder)
    VALUES
        (@KanjiId4, N'学生', N'がくせい', N'Học sinh, sinh viên', 1),
        (@KanjiId4, N'生活', N'せいかつ', N'Sinh hoạt, cuộc sống', 2);
END
GO

-- Verify
SELECT k.KanjiId, k.Character, k.Meaning, k.OnYomi, k.KunYomi,
       (SELECT COUNT(*) FROM KanjiExamples e WHERE e.KanjiId = k.KanjiId) AS ExampleCount
FROM KanjiEntries k
ORDER BY k.KanjiId;
GO
