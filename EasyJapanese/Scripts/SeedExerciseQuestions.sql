-- =============================================
-- Script: SeedExerciseQuestions.sql
-- Description: Seed Questions + AnswerOptions cho mỗi Exercise
--              (ExercisePractice API cần Question.ExerciseId để trả JSON)
-- Idempotent: bỏ qua Exercise đã có Questions
-- =============================================

USE [EasyJapaneseDB];
GO

DECLARE @ExerciseId INT, @ExerciseType NVARCHAR(50), @Title NVARCHAR(255);
DECLARE @QuestionId INT;
DECLARE @i INT;

DECLARE exercise_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT ExerciseId, ExerciseType, Title FROM Exercises ORDER BY ExerciseId;

OPEN exercise_cursor;
FETCH NEXT FROM exercise_cursor INTO @ExerciseId, @ExerciseType, @Title;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Questions WHERE ExerciseId = @ExerciseId)
    BEGIN
        SET @i = 1;
        WHILE @i <= 3
        BEGIN
            DECLARE @QText NVARCHAR(MAX);

            SET @QText = CASE @ExerciseType
                WHEN N'Vocabulary' THEN N'Câu ' + CAST(@i AS NVARCHAR) + N': Từ vựng nào có nghĩa đúng trong bài "' + @Title + N'"?'
                WHEN N'Kanji' THEN N'Câu ' + CAST(@i AS NVARCHAR) + N': Chọn cách đọc đúng của chữ Kanji trong bài "' + @Title + N'"?'
                WHEN N'Grammar' THEN N'Câu ' + CAST(@i AS NVARCHAR) + N': Chọn mẫu câu ngữ pháp đúng trong bài "' + @Title + N'"?'
                WHEN N'Reading' THEN N'Câu ' + CAST(@i AS NVARCHAR) + N': Theo đoạn văn, đáp án nào đúng?'
                WHEN N'Listening' THEN N'Câu ' + CAST(@i AS NVARCHAR) + N': Theo đoạn hội thoại vừa nghe, đáp án nào đúng?'
                ELSE N'Câu ' + CAST(@i AS NVARCHAR) + N': Chọn đáp án đúng.'
            END;

            INSERT INTO Questions (ExerciseId, QuestionText, QuestionType, Points, SortOrder)
            VALUES (@ExerciseId, @QText, N'SingleChoice', 10, @i);

            SET @QuestionId = SCOPE_IDENTITY();

            INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect)
            VALUES
                (@QuestionId, N'Đáp án A (đúng)', 1),
                (@QuestionId, N'Đáp án B', 0),
                (@QuestionId, N'Đáp án C', 0),
                (@QuestionId, N'Đáp án D', 0);

            SET @i = @i + 1;
        END
    END

    FETCH NEXT FROM exercise_cursor INTO @ExerciseId, @ExerciseType, @Title;
END

CLOSE exercise_cursor;
DEALLOCATE exercise_cursor;
GO

-- Verify
SELECT
    e.ExerciseId,
    e.ExerciseType,
    e.Title,
    COUNT(q.QuestionId) AS QuestionCount
FROM Exercises e
LEFT JOIN Questions q ON q.ExerciseId = e.ExerciseId
GROUP BY e.ExerciseId, e.ExerciseType, e.Title
ORDER BY e.ExerciseId;
GO
