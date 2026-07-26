using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLibrary.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreLibrary.Utility
{
    public static class EncodingFixer
    {
        static EncodingFixer()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static string FixMojibake(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input ?? "";

            // Check if string contains typical Mojibake patterns
            if (!input.Contains("Ã") && !input.Contains("á»") && !input.Contains("áº") &&
                !input.Contains("Ä") && !input.Contains("Æ") && !input.Contains("Â") &&
                !input.Contains("áº«") && !input.Contains("cáº"))
            {
                return input;
            }

            try
            {
                var encoding1252 = Encoding.GetEncoding(1252);
                byte[] bytes = encoding1252.GetBytes(input);
                string decoded = Encoding.UTF8.GetString(bytes);

                if (!string.IsNullOrEmpty(decoded) && !decoded.Contains('\uFFFD'))
                {
                    return decoded;
                }
            }
            catch
            {
                // Fallback to manual dictionary replacement below if CP1252 decode fails
            }

            return ApplyManualReplacements(input);
        }

        private static string ApplyManualReplacements(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var map = new Dictionary<string, string>
            {
                {"BÃ i", "Bài"},
                {"bÃ i", "bài"},
                {"Giá»oi", "Giới"},
                {"Giá»o", "Giới"},
                {"giá»oi", "giới"},
                {"giá»o", "giới"},
                {"thiá»u", "thiệu"},
                {"thiá»‡u", "thiệu"},
                {"máº«u", "mẫu"},
                {"cáºu", "câu"},
                {"trá»ng", "trọng"},
                {"tÃâm", "tâm"},
                {"TÃong", "Tổng"},
                {"tÃong", "tổng"},
                {"cÃ¡c", "các"},
                {"cÃ¡ch", "cách"},
                {"CÃ¡ch", "Cách"},
                {"ngÃº", "ngữ"},
                {"phÃ¡p", "pháp"},
                {"sáº½", "sẽ"},
                {"há»oc", "học"},
                {"HÃ¡oc", "Học"},
                {"khÃ³a", "khóa"},
                {"Ä‘Ãdoc", "đọc"},
                {"Ä‘Ãoc", "đọc"},
                {"vÃ ", "và"},
                {"viáº¿t", "viết"},
                {"chá»¯", "chữ"},
                {"HÃ¡n", "Hán"},
                {"trÃºc", "trúc"},
                {"ÄÃokng", "động"},
                {"Ä‘á»™ng", "động"},
                {"tÃº", "từ"},
                {"Luyá»n", "Luyện"},
                {"tÃºp", "tập"},
                {"vÃoi", "với"},
                {"hÃ»", "hội"},
                {"káºot", "kết"}
            };

            foreach (var kv in map)
            {
                text = text.Replace(kv.Key, kv.Value);
            }

            return text;
        }

        public static void FixDatabase(AppDbContext db)
        {
            try
            {
                // 1. Fix Courses
                var courses = db.Courses.ToList();
                bool coursesChanged = false;
                foreach (var c in courses)
                {
                    var newTitle = FixMojibake(c.Title);
                    var newDesc = FixMojibake(c.Description);
                    if (newTitle != c.Title || newDesc != c.Description)
                    {
                        c.Title = newTitle;
                        c.Description = newDesc;
                        coursesChanged = true;
                    }
                }
                if (coursesChanged) db.SaveChanges();

                // 2. Fix Lessons
                var lessons = db.Lessons.ToList();
                bool lessonsChanged = false;
                foreach (var l in lessons)
                {
                    var newTitle = FixMojibake(l.Title);
                    var newContent = FixMojibake(l.Content);
                    if (newTitle != l.Title || newContent != l.Content)
                    {
                        l.Title = newTitle;
                        l.Content = newContent;
                        lessonsChanged = true;
                    }
                }
                if (lessonsChanged) db.SaveChanges();

                // 3. Fix LessonMaterials
                var materials = db.LessonMaterials.ToList();
                bool matChanged = false;
                foreach (var m in materials)
                {
                    var newTitle = FixMojibake(m.Title);
                    if (newTitle != m.Title)
                    {
                        m.Title = newTitle;
                        matChanged = true;
                    }
                }
                if (matChanged) db.SaveChanges();

                // 4. Fix Questions
                var questions = db.Questions.ToList();
                bool qChanged = false;
                foreach (var q in questions)
                {
                    var newText = FixMojibake(q.QuestionText);
                    if (newText != q.QuestionText)
                    {
                        q.QuestionText = newText;
                        qChanged = true;
                    }
                }
                if (qChanged) db.SaveChanges();

                // 5. Fix AnswerOptions
                var options = db.AnswerOptions.ToList();
                bool optChanged = false;
                foreach (var o in options)
                {
                    var newText = FixMojibake(o.AnswerText);
                    if (newText != o.AnswerText)
                    {
                        o.AnswerText = newText;
                        optChanged = true;
                    }
                }
                if (optChanged) db.SaveChanges();

                // 6. Fix PlacementTests
                var tests = db.PlacementTests.ToList();
                bool testChanged = false;
                foreach (var t in tests)
                {
                    var newTitle = FixMojibake(t.Title);
                    var newDesc = FixMojibake(t.Description);
                    if (newTitle != t.Title || newDesc != t.Description)
                    {
                        t.Title = newTitle;
                        t.Description = newDesc;
                        testChanged = true;
                    }
                }
                if (testChanged) db.SaveChanges();

                // 7. Fix Exercises
                var exercises = db.Exercises.ToList();
                bool exChanged = false;
                foreach (var ex in exercises)
                {
                    var newTitle = FixMojibake(ex.Title);
                    var newContent = FixMojibake(ex.Content);
                    if (newTitle != ex.Title || newContent != ex.Content)
                    {
                        ex.Title = newTitle;
                        ex.Content = newContent;
                        exChanged = true;
                    }
                }
                if (exChanged) db.SaveChanges();

                // 8. Fix Quizzes
                var quizzes = db.Quizzes.ToList();
                bool quizChanged = false;
                foreach (var qz in quizzes)
                {
                    var newTitle = FixMojibake(qz.Title);
                    if (newTitle != qz.Title)
                    {
                        qz.Title = newTitle;
                        quizChanged = true;
                    }
                }
                if (quizChanged) db.SaveChanges();

                // 9. Fix Flashcards & Sets
                var sets = db.FlashcardSets.ToList();
                bool setChanged = false;
                foreach (var s in sets)
                {
                    var newTitle = FixMojibake(s.Title);
                    var newDesc = FixMojibake(s.Description);
                    if (newTitle != s.Title || newDesc != s.Description)
                    {
                        s.Title = newTitle;
                        s.Description = newDesc;
                        setChanged = true;
                    }
                }
                if (setChanged) db.SaveChanges();

                var flashcards = db.Flashcards.ToList();
                bool fcChanged = false;
                foreach (var fc in flashcards)
                {
                    var newFront = FixMojibake(fc.FrontText);
                    var newBack = FixMojibake(fc.BackText);
                    if (newFront != fc.FrontText || newBack != fc.BackText)
                    {
                        fc.FrontText = newFront;
                        fc.BackText = newBack;
                        fcChanged = true;
                    }
                }
                if (fcChanged) db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EncodingFixer.FixDatabase warning: {ex.Message}");
            }
        }
    }
}
