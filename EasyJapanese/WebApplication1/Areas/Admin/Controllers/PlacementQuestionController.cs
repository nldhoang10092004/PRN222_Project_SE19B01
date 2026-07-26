using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;
using WebApplication1.Areas.Admin.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    [Route("admin/placement-questions")]
    public class PlacementQuestionController : Controller
    {
        private readonly AppDbContext _context;

        public PlacementQuestionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create(int testId)
        {
            var test = await _context.PlacementTests.FindAsync(testId);
            if (test == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Thêm Câu hỏi mới";
            ViewData["TestTitle"] = test.Title;

            var maxSortOrder = await _context.Questions
                .Where(q => q.TestId == testId)
                .MaxAsync(q => (int?)q.SortOrder) ?? 0;

            var model = new CreateQuestionViewModel
            {
                TestId = testId,
                SortOrder = maxSortOrder + 1,
                AnswerOptions = new List<AnswerOptionDto>
                {
                    new AnswerOptionDto(),
                    new AnswerOptionDto()
                }
            };

            return View(model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateQuestionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var test = await _context.PlacementTests.FindAsync(model.TestId);
                ViewData["Title"] = "Thêm Câu hỏi mới";
                ViewData["TestTitle"] = test?.Title;
                return View(model);
            }

            if (!model.AnswerOptions.Any(o => o.IsCorrect))
            {
                ModelState.AddModelError("", "Phải có ít nhất một đáp án đúng.");
                var test = await _context.PlacementTests.FindAsync(model.TestId);
                ViewData["Title"] = "Thêm Câu hỏi mới";
                ViewData["TestTitle"] = test?.Title;
                return View(model);
            }

            var question = new Question
            {
                TestId = model.TestId,
                QuestionText = model.QuestionText,
                QuestionType = model.QuestionType,
                Points = model.Points,
                SortOrder = model.SortOrder
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            foreach (var optionDto in model.AnswerOptions)
            {
                var option = new AnswerOption
                {
                    QuestionId = question.QuestionId,
                    AnswerText = optionDto.AnswerText,
                    IsCorrect = optionDto.IsCorrect
                };
                _context.AnswerOptions.Add(option);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm câu hỏi thành công.";
            return RedirectToAction("ManageQuestions", "PlacementTest", new { id = model.TestId });
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .Include(q => q.Test)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Chỉnh sửa Câu hỏi";
            ViewData["TestTitle"] = question.Test?.Title;

            var model = new EditQuestionViewModel
            {
                QuestionId = question.QuestionId,
                TestId = question.TestId ?? 0,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                Points = question.Points,
                SortOrder = question.SortOrder,
                AnswerOptions = question.AnswerOptions.Select(o => new AnswerOptionDto
                {
                    OptionId = o.OptionId,
                    AnswerText = o.AnswerText,
                    IsCorrect = o.IsCorrect
                }).ToList()
            };

            return View(model);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditQuestionViewModel model)
        {
            if (id != model.QuestionId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var test = await _context.PlacementTests.FindAsync(model.TestId);
                ViewData["Title"] = "Chỉnh sửa Câu hỏi";
                ViewData["TestTitle"] = test?.Title;
                return View(model);
            }

            if (!model.AnswerOptions.Any(o => o.IsCorrect))
            {
                ModelState.AddModelError("", "Phải có ít nhất một đáp án đúng.");
                var test = await _context.PlacementTests.FindAsync(model.TestId);
                ViewData["Title"] = "Chỉnh sửa Câu hỏi";
                ViewData["TestTitle"] = test?.Title;
                return View(model);
            }

            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            question.QuestionText = model.QuestionText;
            question.QuestionType = model.QuestionType;
            question.Points = model.Points;
            question.SortOrder = model.SortOrder;

            var existingOptionIds = model.AnswerOptions
                .Where(o => o.OptionId.HasValue)
                .Select(o => o.OptionId!.Value)
                .ToList();

            var optionsToDelete = question.AnswerOptions
                .Where(o => !existingOptionIds.Contains(o.OptionId))
                .ToList();

            _context.AnswerOptions.RemoveRange(optionsToDelete);

            foreach (var optionDto in model.AnswerOptions)
            {
                if (optionDto.OptionId.HasValue)
                {
                    var existingOption = question.AnswerOptions
                        .FirstOrDefault(o => o.OptionId == optionDto.OptionId.Value);
                    if (existingOption != null)
                    {
                        existingOption.AnswerText = optionDto.AnswerText;
                        existingOption.IsCorrect = optionDto.IsCorrect;
                    }
                }
                else
                {
                    var newOption = new AnswerOption
                    {
                        QuestionId = question.QuestionId,
                        AnswerText = optionDto.AnswerText,
                        IsCorrect = optionDto.IsCorrect
                    };
                    _context.AnswerOptions.Add(newOption);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật câu hỏi thành công.";
            return RedirectToAction("ManageQuestions", "PlacementTest", new { id = model.TestId });
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            var testId = question.TestId;

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa câu hỏi thành công.";
            return RedirectToAction("ManageQuestions", "PlacementTest", new { id = testId });
        }
    }
}
