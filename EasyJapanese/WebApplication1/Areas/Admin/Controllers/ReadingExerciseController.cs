using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    [Route("admin/reading-exercises")]
    public class ReadingExerciseController : Controller
    {
        private readonly AppDbContext _db;

        public ReadingExerciseController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý Bài Đọc";
            var exercises = await _db.Exercises
                .Include(e => e.Questions)
                .Where(e => e.ExerciseType == "Reading" && e.CourseId == null)
                .OrderBy(e => e.SortOrder)
                .ToListAsync();
            return View(exercises);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(string title, string content, int sortOrder)
        {
            var exercise = new Exercise
            {
                Title = title,
                Content = content,
                ExerciseType = "Reading",
                CourseId = null,
                SortOrder = sortOrder,
                CreatedAt = DateTime.UtcNow
            };

            _db.Exercises.Add(exercise);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo bài đọc mới thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit(int exerciseId, string title, string content, int sortOrder)
        {
            var exercise = await _db.Exercises
                .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.CourseId == null);
            if (exercise == null) return NotFound();

            exercise.Title = title;
            exercise.Content = content;
            exercise.SortOrder = sortOrder;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật bài đọc thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int exerciseId)
        {
            var exercise = await _db.Exercises
                .Include(e => e.Questions).ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.CourseId == null);
            if (exercise == null) return NotFound();

            _db.Exercises.Remove(exercise);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa bài đọc thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("questions/{exerciseId}")]
        public async Task<IActionResult> Questions(int exerciseId)
        {
            var exercise = await _db.Exercises
                .Include(e => e.Questions).ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.CourseId == null);
            if (exercise == null) return NotFound();

            ViewData["Title"] = $"Câu hỏi: {exercise.Title}";
            return View(exercise);
        }

        [HttpPost("questions/{exerciseId}/create")]
        public async Task<IActionResult> CreateQuestion(int exerciseId, string questionText, List<string> options, int correctIndex)
        {
            var exerciseExists = await _db.Exercises.AnyAsync(e => e.ExerciseId == exerciseId && e.CourseId == null);
            if (!exerciseExists) return NotFound();

            var maxSort = await _db.Questions
                .Where(q => q.ExerciseId == exerciseId)
                .Select(q => (int?)q.SortOrder)
                .MaxAsync() ?? 0;

            var question = new Question
            {
                ExerciseId = exerciseId,
                QuestionText = questionText,
                QuestionType = "MultipleChoice",
                Points = 1,
                SortOrder = maxSort + 1
            };

            _db.Questions.Add(question);
            await _db.SaveChangesAsync();

            for (int i = 0; i < options.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(options[i])) continue;
                _db.AnswerOptions.Add(new AnswerOption
                {
                    QuestionId = question.QuestionId,
                    AnswerText = options[i],
                    IsCorrect = (i == correctIndex)
                });
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm câu hỏi mới thành công.";
            return RedirectToAction(nameof(Questions), new { exerciseId });
        }

        [HttpPost("questions/{exerciseId}/edit")]
        public async Task<IActionResult> EditQuestion(int exerciseId, int questionId, string questionText, List<string> options, int correctIndex)
        {
            var question = await _db.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.ExerciseId == exerciseId);
            if (question == null) return NotFound();

            question.QuestionText = questionText;

            _db.AnswerOptions.RemoveRange(question.AnswerOptions);

            for (int i = 0; i < options.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(options[i])) continue;
                _db.AnswerOptions.Add(new AnswerOption
                {
                    QuestionId = question.QuestionId,
                    AnswerText = options[i],
                    IsCorrect = (i == correctIndex)
                });
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật câu hỏi thành công.";
            return RedirectToAction(nameof(Questions), new { exerciseId });
        }

        [HttpPost("questions/{exerciseId}/delete")]
        public async Task<IActionResult> DeleteQuestion(int exerciseId, int questionId)
        {
            var question = await _db.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.ExerciseId == exerciseId);
            if (question == null) return NotFound();

            _db.Questions.Remove(question);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa câu hỏi thành công.";
            return RedirectToAction(nameof(Questions), new { exerciseId });
        }
    }
}
