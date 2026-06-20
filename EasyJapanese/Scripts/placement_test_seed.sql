-- =============================================================
-- Hi Japan! — Placement Test (Test trình độ đầu vào)
-- 40 câu, chia 4 band: N5 (1-10) / N4 (11-20) / N3 (21-30) / N2 (31-40)
--
-- Chấm điểm gợi ý (ghi trong PlacementTest.Description):
--   - Đếm số câu đúng theo band cao nhất user chạm tới
--   - Đúng >= 8/10 ở band N5  và < 5/10 ở N4   -> N5
--   - Đúng >= 8/10 ở band N4  và < 5/10 ở N3   -> N4
--   - Đúng >= 8/10 ở band N3  và < 5/10 ở N2   -> N3
--   - Đúng >= 8/10 ở band N2                       -> N2
--   - Còn lại (rải đều hoặc yếu đều)              -> N5 (mặc định an toàn)
--
-- Idempotent: chạy nhiều lần OK. Dùng MERGE / IF EXISTS.
-- DB target: EasyJapaneseDB (theo appsettings.Example.json)
-- =============================================================

SET XACT_ABORT ON;
BEGIN TRAN;

-- 0. Đảm bảo 5 level JLPT tồn tại (giữ nguyên nếu có)
IF NOT EXISTS (SELECT 1 FROM JlptLevels WHERE LevelName = N'N5')
    INSERT INTO JlptLevels (LevelName, Description, SortOrder)
    VALUES (N'N5', N'Sơ cấp — Bảng chữ cái, chào hỏi, ~800 từ vựng', 1);

IF NOT EXISTS (SELECT 1 FROM JlptLevels WHERE LevelName = N'N4')
    INSERT INTO JlptLevels (LevelName, Description, SortOrder)
    VALUES (N'N4', N'Sơ cấp — Ngữ pháp nền, ~1.500 từ vựng', 2);

IF NOT EXISTS (SELECT 1 FROM JlptLevels WHERE LevelName = N'N3')
    INSERT INTO JlptLevels (LevelName, Description, SortOrder)
    VALUES (N'N3', N'Trung cấp — Đọc hiểu, ~3.750 từ vựng', 3);

IF NOT EXISTS (SELECT 1 FROM JlptLevels WHERE LevelName = N'N2')
    INSERT INTO JlptLevels (LevelName, Description, SortOrder)
    VALUES (N'N2', N'Trung cấp nâng cao — ~6.000 từ vựng, đọc báo', 4);

IF NOT EXISTS (SELECT 1 FROM JlptLevels WHERE LevelName = N'N1')
    INSERT INTO JlptLevels (LevelName, Description, SortOrder)
    VALUES (N'N1', N'Cao cấp — Văn bản học thuật, ~10.000 từ vựng', 5);

-- Lấy LevelId để dùng sau
DECLARE @N5 INT = (SELECT LevelId FROM JlptLevels WHERE LevelName = N'N5');
DECLARE @N4 INT = (SELECT LevelId FROM JlptLevels WHERE LevelName = N'N4');
DECLARE @N3 INT = (SELECT LevelId FROM JlptLevels WHERE LevelName = N'N3');
DECLARE @N2 INT = (SELECT LevelId FROM JlptLevels WHERE LevelName = N'N2');
DECLARE @N1 INT = (SELECT LevelId FROM JlptLevels WHERE LevelName = N'N1');

-- 1. Tạo / cập nhật PlacementTest "Kiểm tra năng lực đầu vào"
IF EXISTS (SELECT 1 FROM PlacementTests WHERE Title = N'Kiểm tra năng lực đầu vào - Hi Japan!')
BEGIN
    UPDATE PlacementTests
    SET Description = N'Bài trắc nghiệm 40 câu, ~20 phút. Chia 4 band N5-N4-N3-N2. Chấm theo band cao nhất đạt >= 8/10 để gợi ý level.',
        Duration = 20,
        PassScore = 24,
        IsActive = 1,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Title = N'Kiểm tra năng lực đầu vào - Hi Japan!';
END
ELSE
BEGIN
    INSERT INTO PlacementTests (Title, Description, Duration, PassScore, IsActive, CreatedBy, CreatedAt, UpdatedAt)
    VALUES (
        N'Kiểm tra năng lực đầu vào - Hi Japan!',
        N'Bài trắc nghiệm 40 câu, ~20 phút. Chia 4 band N5-N4-N3-N2. Chấm theo band cao nhất đạt >= 8/10 để gợi ý level.',
        20, 24, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME()
    );
END

DECLARE @TestId INT = (SELECT TestId FROM PlacementTests WHERE Title = N'Kiểm tra năng lực đầu vào - Hi Japan!');

-- 2. Xóa Question + AnswerOption cũ của test này (re-seed an toàn)
DELETE FROM AnswerOptions
WHERE QuestionId IN (SELECT QuestionId FROM Questions WHERE TestId = @TestId);

DELETE FROM Questions WHERE TestId = @TestId;

-- 3. Insert 40 câu hỏi
-- Cấu trúc 1 lệnh INSERT: cột Question (số), Text, SortOrder, 4 đáp án (A B C D), đáp án đúng (1-4)

-- ============== BAND N5 (câu 1-10): hiragana + chào hỏi + số đếm ==============

-- Câu 1
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Hiragana nào tương ứng với âm "ka"?', N'MultipleChoice', 1, 1);
DECLARE @Q INT = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'か', 1), (@Q, N'さ', 0), (@Q, N'た', 0), (@Q, N'な', 0);

-- Câu 2
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Câu nào nghĩa là "Xin chào"?', N'MultipleChoice', 1, 2);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'こんにちは', 1), (@Q, N'さようなら', 0), (@Q, N'ありがとう', 0), (@Q, N'すみません', 0);

-- Câu 3
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Số "5" trong tiếng Nhật đọc là gì?', N'MultipleChoice', 1, 3);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'ご (go)', 1), (@Q, N'よん (yon)', 0), (@Q, N'なな (nana)', 0), (@Q, N'いち (ichi)', 0);

-- Câu 4
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'"ありがとう" nghĩa là gì?', N'MultipleChoice', 1, 4);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'Cảm ơn', 1), (@Q, N'Xin lỗi', 0), (@Q, N'Vâng', 0), (@Q, N'Tạm biệt', 0);

-- Câu 5
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Chọn katakana đúng của từ "コーヒー" (coffee):', N'MultipleChoice', 1, 5);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'コーヒー', 1), (@Q, N'コオヒー', 0), (@Q, N'コーヒイ', 0), (@Q, N'コオヒイ', 0);

-- Câu 6
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'"私" (watashi) nghĩa là gì?', N'MultipleChoice', 1, 6);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'Tôi', 1), (@Q, N'Bạn', 0), (@Q, N'Anh ấy', 0), (@Q, N'Cô ấy', 0);

-- Câu 7
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Thứ tự đúng của câu "Tôi là sinh viên" trong tiếng Nhật là?', N'MultipleChoice', 1, 7);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'私は学生です', 1), (@Q, N'学生は私です', 0), (@Q, N'私はです学生', 0), (@Q, N'です私は学生', 0);

-- Câu 8
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Ngày trong tuần "thứ Hai" đọc là gì?', N'MultipleChoice', 1, 8);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'げつようび (getsuyoubi)', 1), (@Q, N'かようび (kayoubi)', 0), (@Q, N'もくようび (mokuyoubi)', 0), (@Q, N'きんようび (kinyoubi)', 0);

-- Câu 9
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Màu "đỏ" trong tiếng Nhật là?', N'MultipleChoice', 1, 9);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'あか (aka)', 1), (@Q, N'あお (ao)', 0), (@Q, N'きいろ (kiiro)', 0), (@Q, N'しろ (shiro)', 0);

-- Câu 10
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'"りんご" nghĩa là gì?', N'MultipleChoice', 1, 10);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'Táo', 1), (@Q, N'Chuối', 0), (@Q, N'Nho', 0), (@Q, N'Dưa hấu', 0);

-- ============== BAND N4 (câu 11-20): ngữ pháp cơ bản, kanji N4, đếm ==============

-- Câu 11
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Chọn trợ từ đúng: "Tôi ___ Nhật Bản đi." (Tôi đi đến Nhật Bản)', N'MultipleChoice', 1, 11);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'日本へ行きます', 1), (@Q, N'日本に来ます', 0), (@Q, N'日本を来ます', 0), (@Q, N'日本が行きます', 0);

-- Câu 12
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'"~たい" diễn tả điều gì?', N'MultipleChoice', 1, 12);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'Muốn làm gì đó', 1), (@Q, N'Phải làm gì đó', 0), (@Q, N'Có thể làm gì đó', 0), (@Q, N'Đang làm gì đó', 0);

-- Câu 13
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Điền vào chỗ trống: "コーヒー___ ください" (Cho tôi cà phê)', N'MultipleChoice', 1, 13);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'を', 1), (@Q, N'は', 0), (@Q, N'が', 0), (@Q, N'に', 0);

-- Câu 14
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Kanji "食べる" đọc là gì?', N'MultipleChoice', 1, 14);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'tabe-ru', 1), (@Q, N'no-be-ru', 0), (@Q, N'mi-ru', 0), (@Q, N'ka-u', 0);

-- Câu 15
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Điền vào chỗ trống: "毎日 勉強____ ます" (mỗi ngày học)', N'MultipleChoice', 1, 15);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'し', 1), (@Q, N'す', 0), (@Q, N'する', 0), (@Q, N'して', 0);

-- Câu 16
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Câu nào đúng với thì quá khứ lịch sự?', N'MultipleChoice', 1, 16);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'昨日映画を見ました', 1), (@Q, N'昨日映画を見ます', 0), (@Q, N'昨日映画を見る', 0), (@Q, N'昨日映画を見ない', 0);

-- Câu 17
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Từ nào có nghĩa "đẹp, đẹp đẽ"?', N'MultipleChoice', 1, 17);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'美しい (うつくしい)', 1), (@Q, N'新しい (あたらしい)', 0), (@Q, N'忙しい (いそがしい)', 0), (@Q, N'面白い (おもしろい)', 0);

-- Câu 18
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Đếm "2 quyển sách" dùng trợ số nào?', N'MultipleChoice', 1, 18);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'本 (hon) → 2さつ (ni-satsu)', 1), (@Q, N'2ほん (ni-hon)', 0), (@Q, N'2まい (ni-mai)', 0), (@Q, N'2かい (ni-kai)', 0);

-- Câu 19
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Chọn câu phủ định đúng của "今日は暑いです":', N'MultipleChoice', 1, 19);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'今日は暑くないです', 1), (@Q, N'今日は暑いですない', 0), (@Q, N'今日は暑いくないです', 0), (@Q, N'今日は暑くないます', 0);

-- Câu 20
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'"一緒に" nghĩa là gì?', N'MultipleChoice', 1, 20);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'Cùng nhau', 1), (@Q, N'Một mình', 0), (@Q, N'Từ từ', 0), (@Q, N'Ngay lập tức', 0);

-- ============== BAND N3 (câu 21-30): ngữ pháp nâng cao, kanji N3, đọc hiểu ==============

-- Câu 21
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Mẫu câu "~てみる" diễn tả điều gì?', N'MultipleChoice', 1, 21);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'Thử làm gì đó', 1), (@Q, N'Làm xong rồi', 0), (@Q, N'Đang làm', 0), (@Q, N'Phải làm', 0);

-- Câu 22
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Điền vào chỗ trống: "雨が降った____、出かけました" (Mặc dù trời mưa, tôi vẫn đi ra ngoài)', N'MultipleChoice', 1, 22);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'のに', 1), (@Q, N'から', 0), (@Q, N'ので', 0), (@Q, N'ながら', 0);

-- Câu 23
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Chọn cách đọc đúng của "経済":', N'MultipleChoice', 1, 23);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'けいざい (keizai)', 1), (@Q, N'けいさい (keisai)', 0), (@Q, N'けいざい (keizaii)', 0), (@Q, N'けんざい (kenzai)', 0);

-- Câu 24
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Câu nào có nghĩa "Anh ấy nói rằng sẽ đến vào ngày mai"?', N'MultipleChoice', 1, 24);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'彼は明日来ると言いました', 1), (@Q, N'彼は明日来ますと言いました', 0), (@Q, N'彼は明日来ることを言います', 0), (@Q, N'彼は明日来るなら言います', 0);

-- Câu 25
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Từ "激しい" (hageshii) nghĩa là gì?', N'MultipleChoice', 1, 25);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'Mãnh liệt, dữ dội', 1), (@Q, N'Yên tĩnh', 0), (@Q, N'Bình thường', 0), (@Q, N'Nhẹ nhàng', 0);

-- Câu 26
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Điền vào chỗ trống: "もう____ ので、新しいのを買いたい" (Cái này đã cũ rồi, tôi muốn mua cái mới)', N'MultipleChoice', 1, 26);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'古くなった', 1), (@Q, N'新しい', 0), (@Q, N'高くなる', 0), (@Q, N'安くなる', 0);

-- Câu 27
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Câu nào dùng "のに" đúng cách?', N'MultipleChoice', 1, 27);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'こんなに勉強したのに、試験に落ちた', 1), (@Q, N'こんなに勉強したのに、試験に合格した', 0), (@Q, N'こんなに勉強したから、試験に落ちた', 0), (@Q, N'こんなに勉強したら、試験に落ちた', 0);

-- Câu 28
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'"~ようにする" diễn tả điều gì?', N'MultipleChoice', 1, 28);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'Cố gắng để làm gì đó', 1), (@Q, N'Quên làm gì đó', 0), (@Q, N'Từ chối làm gì đó', 0), (@Q, N'Hoàn thành xong rồi', 0);

-- Câu 29
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Kanji "想像" đọc là gì và nghĩa là gì?', N'MultipleChoice', 1, 29);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'そうぞう - tưởng tượng', 1), (@Q, N'しょうぞう - tăng cường', 0), (@Q, N'そうぞう - vận chuyển', 0), (@Q, N'ぞうそう - xây dựng', 0);

-- Câu 30
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Điền vào chỗ trống: "彼は日本へ行った____ありません" (Anh ấy chưa bao giờ đi Nhật)', N'MultipleChoice', 1, 30);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'ことが', 1), (@Q, N'ために', 0), (@Q, N'のに', 0), (@Q, N'ように', 0);

-- ============== BAND N2 (câu 31-40): ngữ pháp nâng cao, kanji N2, đọc hiểu văn bản ==============

-- Câu 31
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Mẫu "~にもかかわらず" tương đương với?', N'MultipleChoice', 1, 31);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'Mặc dù... vẫn', 1), (@Q, N'Bởi vì... nên', 0), (@Q, N'Nếu... thì', 0), (@Q, N'Khi... thì', 0);

-- Câu 32
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Chọn cách đọc đúng: "心臓" (tim):', N'MultipleChoice', 1, 32);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'しんぞう', 1), (@Q, N'しんそう', 0), (@Q, N'じんぞう', 0), (@Q, N'しんつう', 0);

-- Câu 33
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Câu nào đúng nghĩa: "Càng làm việc, càng kiếm được nhiều tiền"?', N'MultipleChoice', 1, 33);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'働けば働くほど、お金が貯まる', 1), (@Q, N'働くから働くほど、お金が貯まる', 0), (@Q, N'働くほど働けば、お金が貯まる', 0), (@Q, N'働くのに働くほど、お金が貯まる', 0);

-- Câu 34
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'"~次第" (shidai) trong câu "連絡があり次第、お知らせください" nghĩa là gì?', N'MultipleChoice', 1, 34);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'Ngay khi có... thì', 1), (@Q, N'Trước khi... thì', 0), (@Q, N'Sau khi... một lúc', 0), (@Q, N'Thay vì... thì', 0);

-- Câu 35
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Kanji "複雑" đọc là gì và nghĩa là gì?', N'MultipleChoice', 1, 35);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'ふくざつ - phức tạp', 1), (@Q, N'ふくさつ - phức tạp', 0), (@Q, N'ふくざつ - đơn giản', 0), (@Q, N'ぷくざつ - rắc rối', 0);

-- Câu 36
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Câu nào dùng "~ものの" đúng cách?', N'MultipleChoice', 1, 36);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'勉強したものの、試験に落ちた', 1), (@Q, N'勉強するので、試験に落ちた', 0), (@Q, N'勉強したら、試験に落ちた', 0), (@Q, N'勉強するなら、試験に落ちた', 0);

-- Câu 37
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'"~ざるを得ない" nghĩa là gì?', N'MultipleChoice', 1, 37);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'Không thể không làm (bắt buộc)', 1), (@Q, N'Muốn làm nhưng không thể', 0), (@Q, N'Được phép không làm', 0), (@Q, N'Chưa bao giờ làm', 0);

-- Câu 38
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Điền vào chỗ trống: "政府の発表____、経済は回復傾向にある" (Theo thông báo của chính phủ, kinh tế đang phục hồi)', N'MultipleChoice', 1, 38);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'によると', 1), (@Q, N'のために', 0), (@Q, N'について', 0), (@Q, N'に対して', 0);

-- Câu 39
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Từ "従来" (jūrai) nghĩa là gì?', N'MultipleChoice', 1, 39);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'Theo truyền thống, cho đến nay', 1), (@Q, N'Trong tương lai', 0), (@Q, N'Đột nhiên', 0), (@Q, N'Trong khoảnh khắc', 0);

-- Câu 40
INSERT INTO Questions (TestId, QuestionText, QuestionType, Points, SortOrder)
VALUES (@TestId, N'Chọn câu dùng "~どころか" đúng cách:', N'MultipleChoice', 1, 40);
SET @Q = SCOPE_IDENTITY();
INSERT INTO AnswerOptions (QuestionId, AnswerText, IsCorrect) VALUES
    (@Q, N'彼は漢字どころか、ひらがなも書けない', 1), (@Q, N'彼は漢字どころか、ひらがなは書ける', 0), (@Q, N'彼は漢字だけでなく、ひらがなも書けない', 0), (@Q, N'彼は漢字かどうか、ひらがなを書けない', 0);

-- =============================================================
-- 4. (Tùy chọn) Update PassScore = 24 nếu cần thay đổi
-- Đã set PassScore = 24 ở bước 1. Tổng 40 points → pass = 60%.
-- =============================================================

COMMIT TRAN;
GO

-- Verify
SELECT 'PlacementTests' AS [Table], COUNT(*) AS [Count] FROM PlacementTests WHERE IsActive = 1
UNION ALL
SELECT 'Questions', COUNT(*) FROM Questions WHERE TestId = @TestId
UNION ALL
SELECT 'AnswerOptions', COUNT(*) FROM AnswerOptions
    WHERE QuestionId IN (SELECT QuestionId FROM Questions WHERE TestId = @TestId);
