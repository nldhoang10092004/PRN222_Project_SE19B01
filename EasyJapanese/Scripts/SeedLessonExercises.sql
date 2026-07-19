-- =============================================
-- Script: SeedLessonExercises.sql
-- Description: Seed Exercises gắn vào đúng LessonId (không phải CourseId)
--              với 5 ExerciseType: Vocabulary, Kanji, Grammar, Reading, Listening
--              để sidebar trong Lesson.cshtml có đủ content panels
-- Idempotent: bỏ qua Lesson đã có Exercises
-- =============================================

USE [EasyJapaneseDB];
GO

DECLARE @Now DATETIME = SYSUTCDATETIME();
DECLARE @LessonId INT, @CourseId INT;

DECLARE lesson_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT LessonId, CourseId FROM Lessons ORDER BY LessonId;

OPEN lesson_cursor;
FETCH NEXT FROM lesson_cursor INTO @LessonId, @CourseId;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Chỉ seed nếu lesson này chưa có Exercise nào
    IF NOT EXISTS (SELECT 1 FROM Exercises WHERE LessonId = @LessonId)
    BEGIN
        -- Vocabulary
        INSERT INTO Exercises (CourseId, LessonId, ExerciseType, Title, Content, SortOrder, CreatedAt)
        VALUES (@CourseId, @LessonId, N'Vocabulary', N'Từ vựng trọng tâm', N'Học và luyện tập các từ vựng quan trọng trong bài.', 1, @Now);

        -- Kanji (chỉ thêm cho 1 số lesson - không phải lesson nào cũng có)
        IF (@LessonId % 2 = 1)
        BEGIN
            INSERT INTO Exercises (CourseId, LessonId, ExerciseType, Title, Content, StrokeOrderUrl, SortOrder, CreatedAt)
            VALUES (@CourseId, @LessonId, N'Kanji', N'Kanji trong bài', N'Học cách đọc và viết các chữ Hán.', NULL, 2, @Now);
        END

        -- Grammar
        INSERT INTO Exercises (CourseId, LessonId, ExerciseType, Title, Content, SortOrder, CreatedAt)
        VALUES (@CourseId, @LessonId, N'Grammar', N'Ngữ pháp cơ bản', N'Luyện tập mẫu câu ngữ pháp đã học.', 3, @Now);

        -- Reading (lesson chẵn)
        IF (@LessonId % 2 = 0)
        BEGIN
            INSERT INTO Exercises (CourseId, LessonId, ExerciseType, Title, Content, SortOrder, CreatedAt)
            VALUES (@CourseId, @LessonId, N'Reading', N'Đoạn văn đọc hiểu', N'Đọc đoạn văn ngắn và trả lời câu hỏi.', 4, @Now);
        END

        -- Listening (mọi lesson đều có)
        INSERT INTO Exercises (CourseId, LessonId, ExerciseType, Title, Content, AudioUrl, SortOrder, CreatedAt)
        VALUES (@CourseId, @LessonId, N'Listening', N'Luyện nghe', N'Nghe đoạn hội thoại và trả lời câu hỏi.', NULL, 5, @Now);
    END

    FETCH NEXT FROM lesson_cursor INTO @LessonId, @CourseId;
END

CLOSE lesson_cursor;
DEALLOCATE lesson_cursor;
GO

-- Verify
SELECT
    l.LessonId,
    l.CourseId,
    l.Title AS LessonTitle,
    (SELECT COUNT(*) FROM Exercises e WHERE e.LessonId = l.LessonId) AS ExerciseCount,
    (SELECT STRING_AGG(e.ExerciseType, ', ') FROM Exercises e WHERE e.LessonId = l.LessonId) AS ExerciseTypes
FROM Lessons l
WHERE l.CourseId = 1
ORDER BY l.SortOrder;
GO
