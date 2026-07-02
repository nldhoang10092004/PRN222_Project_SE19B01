-- =============================================
-- Script: InsertDummyData.sql
-- Description: Thêm dữ liệu mẫu vào DB EasyJapanese
-- Author: Antigravity AI
-- =============================================

USE [EasyJapaneseDB]
GO

-- 1. Thêm JlptLevels
IF NOT EXISTS (SELECT * FROM [JlptLevels] WHERE [LevelName] = 'N5')
    INSERT INTO [JlptLevels] ([LevelName], [Description], [SortOrder]) VALUES ('N5', N'Cấp độ cơ bản nhất, hiểu tiếng Nhật cơ bản.', 1);
IF NOT EXISTS (SELECT * FROM [JlptLevels] WHERE [LevelName] = 'N4')
    INSERT INTO [JlptLevels] ([LevelName], [Description], [SortOrder]) VALUES ('N4', N'Hiểu tiếng Nhật cơ bản, giao tiếp hàng ngày.', 2);
IF NOT EXISTS (SELECT * FROM [JlptLevels] WHERE [LevelName] = 'N3')
    INSERT INTO [JlptLevels] ([LevelName], [Description], [SortOrder]) VALUES ('N3', N'Cấp độ trung cấp, hiểu bài báo, hội thoại tự nhiên.', 3);
IF NOT EXISTS (SELECT * FROM [JlptLevels] WHERE [LevelName] = 'N2')
    INSERT INTO [JlptLevels] ([LevelName], [Description], [SortOrder]) VALUES ('N2', N'Trung cấp cao, làm việc được trong môi trường tiếng Nhật.', 4);
IF NOT EXISTS (SELECT * FROM [JlptLevels] WHERE [LevelName] = 'N1')
    INSERT INTO [JlptLevels] ([LevelName], [Description], [SortOrder]) VALUES ('N1', N'Cấp độ cao nhất, thông thạo tiếng Nhật.', 5);
GO

-- 2. Thêm Accounts cho Mentor
IF NOT EXISTS (SELECT * FROM [Accounts] WHERE [Email] = 'mentor1@hijapan.com')
BEGIN
    INSERT INTO [Accounts] ([Email], [PasswordHash], [Role], [IsEmailVerified], [IsLocked], [CreatedAt])
    VALUES ('mentor1@hijapan.com', 'dummy_hash', 'Mentor', 1, 0, GETUTCDATE());
    
    DECLARE @Mentor1Id INT = SCOPE_IDENTITY();
    
    INSERT INTO [Mentors] ([MentorId], [FullName], [Bio], [Expertise], [PhoneNumber], [AvatarUrl], [CreatedAt])
    VALUES (@Mentor1Id, N'Cô Nguyễn Thị A', N'Giáo viên kinh nghiệm 5 năm giảng dạy tiếng Nhật cơ bản N5, N4.', N'Ngữ pháp cơ bản', '0987654321', 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&h=150&fit=crop', GETUTCDATE());
END

IF NOT EXISTS (SELECT * FROM [Accounts] WHERE [Email] = 'mentor2@hijapan.com')
BEGIN
    INSERT INTO [Accounts] ([Email], [PasswordHash], [Role], [IsEmailVerified], [IsLocked], [CreatedAt])
    VALUES ('mentor2@hijapan.com', 'dummy_hash', 'Mentor', 1, 0, GETUTCDATE());
    
    DECLARE @Mentor2Id INT = SCOPE_IDENTITY();
    
    INSERT INTO [Mentors] ([MentorId], [FullName], [Bio], [Expertise], [PhoneNumber], [AvatarUrl], [CreatedAt])
    VALUES (@Mentor2Id, N'Thầy Trần Văn B', N'Cựu du học sinh Nhật Bản, 3 năm làm việc tại Tokyo.', N'Giao tiếp, Kaiwa', '0123456789', 'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=150&h=150&fit=crop', GETUTCDATE());
END

IF NOT EXISTS (SELECT * FROM [Accounts] WHERE [Email] = 'mentor3@hijapan.com')
BEGIN
    INSERT INTO [Accounts] ([Email], [PasswordHash], [Role], [IsEmailVerified], [IsLocked], [CreatedAt])
    VALUES ('mentor3@hijapan.com', 'dummy_hash', 'Mentor', 1, 0, GETUTCDATE());
    
    DECLARE @Mentor3Id INT = SCOPE_IDENTITY();
    
    INSERT INTO [Mentors] ([MentorId], [FullName], [Bio], [Expertise], [PhoneNumber], [AvatarUrl], [CreatedAt])
    VALUES (@Mentor3Id, N'Cô Lê Thị C', N'Chuyên gia đào tạo luyện thi JLPT N3, N2.', N'Luyện thi JLPT', '0999888777', 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&h=150&fit=crop', GETUTCDATE());
END
GO

-- 3. Thêm Khóa học
DECLARE @N5LevelId INT = (SELECT TOP 1 LevelId FROM [JlptLevels] WHERE LevelName = 'N5');
DECLARE @N4LevelId INT = (SELECT TOP 1 LevelId FROM [JlptLevels] WHERE LevelName = 'N4');
DECLARE @N3LevelId INT = (SELECT TOP 1 LevelId FROM [JlptLevels] WHERE LevelName = 'N3');

DECLARE @M1Id INT = (SELECT TOP 1 MentorId FROM [Mentors] WHERE [FullName] LIKE N'%Nguyễn Thị A%');
DECLARE @M2Id INT = (SELECT TOP 1 MentorId FROM [Mentors] WHERE [FullName] LIKE N'%Trần Văn B%');
DECLARE @M3Id INT = (SELECT TOP 1 MentorId FROM [Mentors] WHERE [FullName] LIKE N'%Lê Thị C%');

DECLARE @AdminId INT = (SELECT TOP 1 AdminId FROM [Admins]);

IF NOT EXISTS (SELECT * FROM Mentors WHERE MentorId = @AdminId)
    INSERT INTO Mentors (MentorId, FullName, CreatedAt) VALUES (@AdminId, 'Admin Mentor', GETUTCDATE());

IF NOT EXISTS (SELECT * FROM [Courses] WHERE [Title] = N'Khóa học tiếng Nhật N5')
BEGIN
    INSERT INTO [Courses] ([Title], [Description], [LevelId], [MentorId], [IsFree], [IsPublished], [CreatedBy])
    VALUES (N'Khóa học tiếng Nhật N5', N'Khóa học nhập môn dành cho người mới bắt đầu. Nắm vững bảng chữ cái và ngữ pháp cơ bản.', @N5LevelId, @M1Id, 1, 1, @AdminId);
    
    DECLARE @Course1Id INT = SCOPE_IDENTITY();
    
    INSERT INTO [Lessons] ([CourseId], [Title], [Content], [VideoUrl], [LessonType], [SortOrder])
    VALUES 
    (@Course1Id, N'Bài 1: Bảng chữ cái Hiragana', N'Học bảng chữ cái mềm', 'https://www.youtube.com/embed/bOUqVC4XkOY', 'Video', 1),
    (@Course1Id, N'Bài 2: Bảng chữ cái Katakana', N'Học bảng chữ cái cứng', 'https://www.youtube.com/embed/s6AMBpsE1a0', 'Video', 2),
    (@Course1Id, N'Bài 3: Chào hỏi cơ bản', N'Các câu chào hỏi', 'https://www.youtube.com/embed/1j2vV3yQJ0E', 'Video', 3);
END

IF NOT EXISTS (SELECT * FROM [Courses] WHERE [Title] = N'Khóa học tiếng Nhật N4')
BEGIN
    INSERT INTO [Courses] ([Title], [Description], [LevelId], [MentorId], [IsFree], [IsPublished], [CreatedBy])
    VALUES (N'Khóa học tiếng Nhật N4', N'Nâng cao khả năng giao tiếp và vốn từ vựng. Chuẩn bị nền tảng vững chắc cho N3.', @N4LevelId, @M2Id, 0, 1, @AdminId);
    
    DECLARE @Course2Id INT = SCOPE_IDENTITY();
    
    INSERT INTO [Lessons] ([CourseId], [Title], [Content], [VideoUrl], [LessonType], [SortOrder])
    VALUES 
    (@Course2Id, N'Bài 1: Thể bị động', N'Cách dùng thể bị động', 'https://www.youtube.com/embed/example1', 'Video', 1),
    (@Course2Id, N'Bài 2: Thể sai khiến', N'Cách dùng thể sai khiến', 'https://www.youtube.com/embed/example2', 'Video', 2);
END

IF NOT EXISTS (SELECT * FROM [Courses] WHERE [Title] = N'Khóa học tiếng Nhật N3')
BEGIN
    INSERT INTO [Courses] ([Title], [Description], [LevelId], [MentorId], [IsFree], [IsPublished], [CreatedBy])
    VALUES (N'Khóa học tiếng Nhật N3', N'Khóa học trọng tâm giao tiếp kinh doanh và đọc hiểu báo chí Nhật Bản.', @N3LevelId, @M3Id, 0, 1, @AdminId);
END
GO
