using CoreLibrary.Const;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Storage;
using CoreLibrary.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.MENTOR)]
    [Route("teacher/exercises")]
    public class ExercisesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IStorageService _storage;

        static ExercisesController()
        {
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("EasyJapanese");
            }
            catch
            {
                // License may already be set by another controller (e.g. QuizzesController)
            }
        }

        public ExercisesController(AppDbContext context, IStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        private int GetCurrentMentorId()
        {
            var user = HttpContext.Session.GetObject<CoreLibrary.Authentication.CurrentUser>(CoreLibrary.Authentication.IAuthenticationService.SessionKeyCurrentUser);
            return user?.AccountId ?? 0;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? courseId)
        {
            ViewData["Title"] = "Quản lý Bài tập (Exercise)";
            var mentorId = GetCurrentMentorId();

            var courses = await _context.Courses
                .Where(c => c.CreatedBy == mentorId)
                .ToListAsync();

            var courseIds = courses.Select(c => c.CourseId).ToList();
            var lessonsByCourse = await _context.Lessons
                .Where(l => courseIds.Contains(l.CourseId))
                .OrderBy(l => l.CourseId).ThenBy(l => l.SortOrder)
                .Select(l => new { l.LessonId, l.CourseId, l.Title })
                .ToListAsync();

            ViewBag.LessonsByCourse = lessonsByCourse
                .GroupBy(l => l.CourseId)
                .ToDictionary(g => g.Key, g => g.Select(l => new { l.LessonId, l.Title }).ToList());

            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;

            var query = _context.Exercises
                .Include(e => e.Course)
                .Include(e => e.Lesson)
                .Include(e => e.Questions)
                .Where(e => e.Course.CreatedBy == mentorId);

            if (courseId.HasValue)
            {
                query = query.Where(e => e.CourseId == courseId.Value);
            }

            var exercises = await query
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(exercises);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Exercise model, int? lessonId)
        {
            var mentorId = GetCurrentMentorId();

            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == model.CourseId && c.CreatedBy == mentorId);
            if (!courseExists) return Forbid();

            if (lessonId.HasValue)
            {
                var lessonValid = await _context.Lessons.AnyAsync(l => l.LessonId == lessonId.Value && l.CourseId == model.CourseId);
                if (!lessonValid) return BadRequest("Bài học không thuộc khóa học đã chọn.");
                model.LessonId = lessonId;
            }

            model.CreatedAt = DateTime.UtcNow;
            model.SortOrder = 0;

            _context.Exercises.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo Bài tập mới thành công.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit(Exercise model, int? lessonId)
        {
            var mentorId = GetCurrentMentorId();

            var exercise = await _context.Exercises
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.ExerciseId == model.ExerciseId && e.Course.CreatedBy == mentorId);

            if (exercise == null) return NotFound();

            if (lessonId.HasValue)
            {
                var lessonValid = await _context.Lessons.AnyAsync(l => l.LessonId == lessonId.Value && l.CourseId == model.CourseId);
                if (!lessonValid) return BadRequest("Bài học không thuộc khóa học đã chọn.");
            }

            exercise.Title = model.Title;
            exercise.ExerciseType = model.ExerciseType;
            exercise.Content = model.Content;
            exercise.AudioUrl = model.AudioUrl;
            exercise.StrokeOrderUrl = model.StrokeOrderUrl;
            exercise.CourseId = model.CourseId;
            exercise.LessonId = lessonId;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật Bài tập thành công.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        [HttpPost("update-assets")]
        public async Task<IActionResult> UpdateAssets(int exerciseId, string? content, IFormFile? audioFile, IFormFile? strokeOrderFile, CancellationToken cancellationToken)
        {
            var mentorId = GetCurrentMentorId();

            var exercise = await _context.Exercises
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.Course.CreatedBy == mentorId, cancellationToken);

            if (exercise == null) return NotFound();

            exercise.Content = content;

            if (audioFile != null && audioFile.Length > 0)
            {
                var ext = Path.GetExtension(audioFile.FileName);
                var key = $"exercises/{exerciseId}/audio-{Guid.NewGuid()}{ext}";
                using var stream = audioFile.OpenReadStream();
                exercise.AudioUrl = await _storage.UploadAsync(key, stream, audioFile.ContentType, cancellationToken);
            }

            if (strokeOrderFile != null && strokeOrderFile.Length > 0)
            {
                var ext = Path.GetExtension(strokeOrderFile.FileName);
                var key = $"exercises/{exerciseId}/stroke-{Guid.NewGuid()}{ext}";
                using var stream = strokeOrderFile.OpenReadStream();
                exercise.StrokeOrderUrl = await _storage.UploadAsync(key, stream, strokeOrderFile.ContentType, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Cập nhật tài liệu đính kèm thành công.";
            return RedirectToAction(nameof(Questions), new { exerciseId });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int exerciseId)
        {
            var mentorId = GetCurrentMentorId();

            var exercise = await _context.Exercises
                .Include(e => e.Course)
                .Include(e => e.Questions)
                .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.Course.CreatedBy == mentorId);

            if (exercise == null) return NotFound();

            var courseId = exercise.CourseId;
            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa Bài tập thành công.";
            return RedirectToAction(nameof(Index), new { courseId });
        }

        [HttpGet("questions/{exerciseId}")]
        public async Task<IActionResult> Questions(int exerciseId)
        {
            var mentorId = GetCurrentMentorId();

            var exercise = await _context.Exercises
                .Include(e => e.Course)
                .Include(e => e.Questions)
                .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.Course.CreatedBy == mentorId);

            if (exercise == null) return NotFound();

            ViewData["Title"] = $"Câu hỏi Bài tập: {exercise.Title}";
            return View(exercise);
        }

        [HttpPost("questions/{exerciseId}/create")]
        public async Task<IActionResult> CreateQuestion(int exerciseId, string questionText, int points, List<string> options, int correctIndex)
        {
            var mentorId = GetCurrentMentorId();

            var exercise = await _context.Exercises
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.Course.CreatedBy == mentorId);

            if (exercise == null) return NotFound();

            var question = new Question
            {
                ExerciseId = exerciseId,
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
            return RedirectToAction(nameof(Questions), new { exerciseId });
        }

        [HttpPost("questions/{exerciseId}/delete")]
        public async Task<IActionResult> DeleteQuestion(int exerciseId, int questionId)
        {
            var mentorId = GetCurrentMentorId();

            var exercise = await _context.Exercises
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.Course.CreatedBy == mentorId);

            if (exercise == null) return NotFound();

            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.ExerciseId == exerciseId);

            if (question == null) return NotFound();

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa câu hỏi thành công.";
            return RedirectToAction(nameof(Questions), new { exerciseId });
        }

        [HttpPost("questions/{exerciseId}/import-excel")]
        public async Task<IActionResult> ImportExcel(int exerciseId, IFormFile excelFile)
        {
            var mentorId = GetCurrentMentorId();

            var exercise = await _context.Exercises
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.Course.CreatedBy == mentorId);

            if (exercise == null) return NotFound();

            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một file Excel hợp lệ.";
                return RedirectToAction(nameof(Questions), new { exerciseId });
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
                            return RedirectToAction(nameof(Questions), new { exerciseId });
                        }

                        var rowCount = worksheet.Dimension.Rows;
                        int importedCount = 0;

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
                                correctIdx = parsedIdx - 1;
                            }

                            var question = new Question
                            {
                                ExerciseId = exerciseId,
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

            return RedirectToAction(nameof(Questions), new { exerciseId });
        }

        [HttpPost("import-exercise-set")]
        public async Task<IActionResult> ImportExerciseSet(int courseId, string title, string exerciseType, string? content, string? audioUrl, string? strokeOrderUrl, IFormFile excelFile)
        {
            var mentorId = GetCurrentMentorId();

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

                        var exercise = new Exercise
                        {
                            CourseId = courseId,
                            Title = title,
                            ExerciseType = exerciseType,
                            Content = content,
                            AudioUrl = audioUrl,
                            StrokeOrderUrl = strokeOrderUrl,
                            CreatedAt = DateTime.UtcNow,
                            SortOrder = 0
                        };

                        _context.Exercises.Add(exercise);
                        await _context.SaveChangesAsync();

                        var rowCount = worksheet.Dimension.Rows;
                        int importedCount = 0;

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
                                correctIdx = parsedIdx - 1;
                            }

                            var question = new Question
                            {
                                ExerciseId = exercise.ExerciseId,
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
                            TempData["SuccessMessage"] = $"Đã khởi tạo Bài tập '{title}' và nhập thành công {importedCount} câu hỏi từ Excel.";
                        }
                        else
                        {
                            _context.Exercises.Remove(exercise);
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

        [HttpPost("questions/{exerciseId}/edit")]
        public async Task<IActionResult> EditQuestion(int exerciseId, int questionId, string questionText, int points, List<string> options, int correctIndex)
        {
            var mentorId = GetCurrentMentorId();

            var exercise = await _context.Exercises
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.Course.CreatedBy == mentorId);

            if (exercise == null) return NotFound();

            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.ExerciseId == exerciseId);

            if (question == null) return NotFound();

            question.QuestionText = questionText;
            question.Points = points;

            _context.AnswerOptions.RemoveRange(question.AnswerOptions);

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
            TempData["SuccessMessage"] = "Cập nhật câu hỏi thành công.";
            return RedirectToAction(nameof(Questions), new { exerciseId });
        }
    }
}