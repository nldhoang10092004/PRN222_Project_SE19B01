using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using CoreLibrary.Utility;
using OfficeOpenXml;

namespace WebApplication1.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.MENTOR)]
    [Route("teacher/quizzes")]
    public class QuizzesController : Controller
    {
        private readonly AppDbContext _context;

        static QuizzesController()
        {
            ExcelPackage.License.SetNonCommercialPersonal("EasyJapanese");
        }

        public QuizzesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? courseId)
        {
            ViewData["Title"] = "Quản lý Quizzes";
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            // Load courses taught by this mentor
            var courses = await _context.Courses
                .Where(c => c.CreatedBy == mentorId)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;

            // Query quizzes
            var query = _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                .Where(q => q.Course.CreatedBy == mentorId);

            if (courseId.HasValue)
            {
                query = query.Where(q => q.CourseId == courseId.Value);
            }

            var quizzes = await query
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(quizzes);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Quiz model)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == model.CourseId && c.CreatedBy == mentorId);
            if (!courseExists) return Forbid();

            model.CreatedAt = DateTime.UtcNow;
            model.SortOrder = 0;

            _context.Quizzes.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo Quiz mới thành công.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit(Quiz model)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.QuizId == model.QuizId && q.Course.CreatedBy == mentorId);

            if (quiz == null) return NotFound();

            quiz.Title = model.Title;
            quiz.Duration = model.Duration;
            quiz.PassScore = model.PassScore;
            quiz.CourseId = model.CourseId;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật Quiz thành công.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int quizId)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuizId == quizId && q.Course.CreatedBy == mentorId);

            if (quiz == null) return NotFound();

            var courseId = quiz.CourseId;
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa Quiz thành công.";
            return RedirectToAction(nameof(Index), new { courseId });
        }

        [HttpGet("questions/{quizId}")]
        public async Task<IActionResult> Questions(int quizId)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuizId == quizId && q.Course.CreatedBy == mentorId);

            if (quiz == null) return NotFound();

            ViewData["Title"] = $"Câu hỏi Quiz: {quiz.Title}";
            return View(quiz);
        }

        [HttpPost("questions/{quizId}/create")]
        public async Task<IActionResult> CreateQuestion(int quizId, string questionText, int points, List<string> options, int correctIndex)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.QuizId == quizId && q.Course.CreatedBy == mentorId);

            if (quiz == null) return NotFound();

            var question = new Question
            {
                QuizId = quizId,
                QuestionText = questionText,
                QuestionType = "MultipleChoice",
                Points = points,
                SortOrder = 0
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            for (int i = 0; i < options.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(options[i])) continue;
                var opt = new AnswerOption
                {
                    QuestionId = question.QuestionId,
                    AnswerText = options[i],
                    IsCorrect = (i == correctIndex)
                };
                _context.AnswerOptions.Add(opt);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm câu hỏi mới thành công.";
            return RedirectToAction(nameof(Questions), new { quizId });
        }

        [HttpPost("questions/{quizId}/delete")]
        public async Task<IActionResult> DeleteQuestion(int quizId, int questionId)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.QuizId == quizId && q.Course.CreatedBy == mentorId);

            if (quiz == null) return NotFound();

            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.QuizId == quizId);

            if (question == null) return NotFound();

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa câu hỏi thành công.";
            return RedirectToAction(nameof(Questions), new { quizId });
        }

        [HttpPost("questions/{quizId}/import-excel")]
        public async Task<IActionResult> ImportExcel(int quizId, IFormFile excelFile)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.QuizId == quizId && q.Course.CreatedBy == mentorId);

            if (quiz == null) return NotFound();

            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một file Excel hợp lệ.";
                return RedirectToAction(nameof(Questions), new { quizId });
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            TempData["ErrorMessage"] = "File Excel trống.";
                            return RedirectToAction(nameof(Questions), new { quizId });
                        }

                        var rowCount = worksheet.Dimension.Rows;
                        int importedCount = 0;

                        // Start from row 2 (skipping header)
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var qText = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                            var pointsStr = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                            var opt1 = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                            var opt2 = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                            var opt3 = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                            var opt4 = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                            var correctIdxStr = worksheet.Cells[row, 7].Value?.ToString()?.Trim();

                            if (string.IsNullOrEmpty(qText)) continue;

                            int points = 10;
                            int.TryParse(pointsStr, out points);

                            int correctIdx = 0;
                            if (int.TryParse(correctIdxStr, out int parsedIdx))
                            {
                                correctIdx = parsedIdx - 1; // 1-indexed to 0-indexed
                            }

                            var question = new Question
                            {
                                QuizId = quizId,
                                QuestionText = qText,
                                QuestionType = "MultipleChoice",
                                Points = points,
                                SortOrder = 0
                            };

                            _context.Questions.Add(question);
                            await _context.SaveChangesAsync();

                            var options = new string[] { opt1, opt2, opt3, opt4 };
                            for (int i = 0; i < options.Length; i++)
                            {
                                if (string.IsNullOrEmpty(options[i])) continue;
                                var opt = new AnswerOption
                                {
                                    QuestionId = question.QuestionId,
                                    AnswerText = options[i],
                                    IsCorrect = (i == correctIdx)
                                };
                                _context.AnswerOptions.Add(opt);
                            }

                            importedCount++;
                        }

                        if (importedCount > 0)
                        {
                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = $"Đã nhập thành công {importedCount} câu hỏi từ file Excel.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Không tìm thấy dữ liệu hợp lệ trong file Excel.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xử lý file Excel: {ex.Message}";
            }

            return RedirectToAction(nameof(Questions), new { quizId });
        }

        [HttpPost("import-quiz-set")]
        public async Task<IActionResult> ImportQuizSet(int courseId, string title, int? duration, int passScore, IFormFile excelFile)
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            var mentorId = user?.AccountId ?? 0;

            // Verify course belongs to this mentor
            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == courseId && c.CreatedBy == mentorId);
            if (!courseExists) return Forbid();

            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một file Excel hợp lệ.";
                return RedirectToAction(nameof(Index), new { courseId });
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            TempData["ErrorMessage"] = "File Excel trống.";
                            return RedirectToAction(nameof(Index), new { courseId });
                        }

                        // Create the Quiz first
                        var quiz = new Quiz
                        {
                            CourseId = courseId,
                            Title = title,
                            Duration = duration,
                            PassScore = passScore,
                            CreatedAt = DateTime.UtcNow,
                            SortOrder = 0
                        };

                        _context.Quizzes.Add(quiz);
                        await _context.SaveChangesAsync();

                        var rowCount = worksheet.Dimension.Rows;
                        int importedCount = 0;

                        // Start from row 2 (skipping header)
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var qText = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                            var pointsStr = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                            var opt1 = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                            var opt2 = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                            var opt3 = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                            var opt4 = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                            var correctIdxStr = worksheet.Cells[row, 7].Value?.ToString()?.Trim();

                            if (string.IsNullOrEmpty(qText)) continue;

                            int points = 10;
                            int.TryParse(pointsStr, out points);

                            int correctIdx = 0;
                            if (int.TryParse(correctIdxStr, out int parsedIdx))
                            {
                                correctIdx = parsedIdx - 1; // 1-indexed to 0-indexed
                            }

                            var question = new Question
                            {
                                QuizId = quiz.QuizId,
                                QuestionText = qText,
                                QuestionType = "MultipleChoice",
                                Points = points,
                                SortOrder = 0
                            };

                            _context.Questions.Add(question);
                            await _context.SaveChangesAsync();

                            var options = new string[] { opt1, opt2, opt3, opt4 };
                            for (int i = 0; i < options.Length; i++)
                            {
                                if (string.IsNullOrEmpty(options[i])) continue;
                                var opt = new AnswerOption
                                {
                                    QuestionId = question.QuestionId,
                                    AnswerText = options[i],
                                    IsCorrect = (i == correctIdx)
                                };
                                _context.AnswerOptions.Add(opt);
                            }

                            importedCount++;
                        }

                        if (importedCount > 0)
                        {
                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = $"Đã khởi tạo Quiz '{title}' và nhập thành công {importedCount} câu hỏi từ Excel.";
                        }
                        else
                        {
                            // Rollback empty quiz
                            _context.Quizzes.Remove(quiz);
                            await _context.SaveChangesAsync();
                            TempData["ErrorMessage"] = "Không tìm thấy dữ liệu câu hỏi hợp lệ trong file Excel.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xử lý file Excel: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { courseId });
        }
    }
}
