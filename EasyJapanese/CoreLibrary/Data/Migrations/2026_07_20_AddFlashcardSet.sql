-- ============================================================
-- Migration: Tách FlashcardSet khỏi Course
-- Mục đích: Flashcard trở thành tài nguyên public, có thể gom
--           vào các Set (FlashcardSet) độc lập. Set có thể liên
--           kết tùy chọn với 1 Course qua CourseId nullable.
--
-- Chạy trên database đang có dữ liệu Flashcards.
-- Toàn bộ wrap trong transaction để rollback nếu lỗi.
-- ============================================================

BEGIN TRANSACTION;

BEGIN TRY

-- 1. Tạo bảng FlashcardSets
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FlashcardSets')
BEGIN
    CREATE TABLE dbo.FlashcardSets (
        FlashcardSetId   INT IDENTITY(1,1) NOT NULL,
        Title            NVARCHAR(200) NOT NULL,
        Description      NVARCHAR(MAX) NULL,
        ImageUrl         NVARCHAR(500) NULL,
        CourseId         INT NULL,
        CreatedBy        INT NOT NULL,
        CreatedAt        DATETIME NOT NULL CONSTRAINT DF_FlashcardSets_CreatedAt DEFAULT (getutcdate()),
        UpdatedAt        DATETIME NOT NULL CONSTRAINT DF_FlashcardSets_UpdatedAt DEFAULT (getutcdate()),
        CONSTRAINT PK_FlashcardSets PRIMARY KEY (FlashcardSetId)
    );

    ALTER TABLE dbo.FlashcardSets
        ADD CONSTRAINT FK_FlashcardSets_Course
        FOREIGN KEY (CourseId) REFERENCES dbo.Courses(CourseId);

    ALTER TABLE dbo.FlashcardSets
        ADD CONSTRAINT FK_FlashcardSets_Creator
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Accounts(AccountId);
END

-- 2. Thêm cột FlashcardSetId (nullable) vào Flashcards
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Flashcards') AND name = 'FlashcardSetId'
)
BEGIN
    ALTER TABLE dbo.Flashcards ADD FlashcardSetId INT NULL;
END

-- 3. Thêm cột ImageUrl vào Flashcards (ảnh riêng từng thẻ)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Flashcards') AND name = 'ImageUrl'
)
BEGIN
    ALTER TABLE dbo.Flashcards ADD ImageUrl NVARCHAR(500) NULL;
END

-- 4. FK Flashcards.FlashcardSetId → FlashcardSets
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Flashcards_FlashcardSet'
)
BEGIN
    ALTER TABLE dbo.Flashcards
        ADD CONSTRAINT FK_Flashcards_FlashcardSet
        FOREIGN KEY (FlashcardSetId) REFERENCES dbo.FlashcardSets(FlashcardSetId);
END

-- 5. Bỏ FK cứng CourseId (cho phép flashcard public, không gắn Course)
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK__Flashcard__Cours__3493CFA7'
)
BEGIN
    ALTER TABLE dbo.Flashcards DROP CONSTRAINT FK__Flashcard__Cours__3493CFA7;
END

COMMIT TRANSACTION;
PRINT 'Migration applied successfully.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Migration failed: ' + ERROR_MESSAGE();
    THROW;
END CATCH;