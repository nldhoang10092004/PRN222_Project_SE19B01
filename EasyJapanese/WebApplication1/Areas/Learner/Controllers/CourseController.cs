using CoreLibrary.Authentication;
using CoreLibrary.Data;
using CoreLibrary.Const;
using CoreWeb.Areas.Learner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class CourseController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IAuthenticationService _auth;

        public CourseController(AppDbContext db, IAuthenticationService auth)
        {
            _db = db;
            _auth = auth;
        }

        // GET: /learn/Course
        public async Task<IActionResult> Index()
        {
            int? studentId = await GetCurrentStudentIdAsync();

            var hasMembership = studentId.HasValue && await _db.StudentMemberships
                .AnyAsync(m => m.StudentId == studentId.Value
                            && m.IsActive
                            && m.EndDate > DateTime.UtcNow);

            // ── Logic hiển thị ──
            // Không có membership  → chỉ lấy IsFree = true
            // Có membership + đã làm placement test → hiện TẤT CẢ, ViewBag.RecommendedLevelId để highlight
            // Có membership + chưa làm test          → hiện TẤT CẢ
            var query = _db.Courses
                .Where(c => c.IsPublished)
                .Include(c => c.Level)
                .Include(c => c.Mentor)
                .AsQueryable();

            if (!hasMembership)
                query = query.Where(c => c.IsFree);

            var courses = await query
                .OrderBy(c => c.Level.SortOrder)
                .ThenBy(c => c.Title)
                .ToListAsync();

            // Placement result (chỉ dùng để highlight, không filter bỏ level)
            int? recommendedLevelId = null;
            string? recommendedLevelName = null;

            if (studentId.HasValue)
            {
                var placement = await _db.StudentPlacementResults
                    .Where(p => p.StudentId == studentId.Value && p.CompletedAt != null)
                    .OrderByDescending(p => p.CompletedAt)
                    .Select(p => new
                    {
                        p.RecommendedLevelId,
                        LevelName = p.RecommendedLevel != null ? p.RecommendedLevel.LevelName : null
                    })
                    .FirstOrDefaultAsync();

                recommendedLevelId = placement?.RecommendedLevelId;
                recommendedLevelName = placement?.LevelName;
            }

            // Enrollments của student hiện tại (để đánh dấu "Tiếp tục học")
            var enrolledIds = new HashSet<int>();
            if (studentId.HasValue)
            {
                var ids = await _db.Enrollments
                    .Where(e => e.StudentId == studentId.Value)
                    .Select(e => e.CourseId)
                    .ToListAsync();
                enrolledIds = new HashSet<int>(ids);
            }

            ViewBag.HasMembership = hasMembership;
            ViewBag.RecommendedLevelId = recommendedLevelId;
            ViewBag.RecommendedLevelName = recommendedLevelName;
            ViewBag.EnrolledCourseIds = enrolledIds;

            return View(courses);
        }

        // GET: /learn/Course/Detail/1
        public async Task<IActionResult> Detail(int id = 1)
        {
            var course = await _db.Courses
                .Include(c => c.Level)
                .Include(c => c.Mentor)
                .Include(c => c.Lessons)
                .Include(c => c.Enrollments)
                .Include(c => c.CourseReviews)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();

            if (!course.IsFree && !await HasAccessAsync())
            {
                TempData["LockedMessage"] = "Bạn cần đăng ký Membership để truy cập khóa học này.";
                return RedirectToAction("Index", "Membership");
            }

            return View(course);
        }

        // GET: /learn/Course/Lesson/1
        [HttpGet]
        public async Task<IActionResult> Lesson(int id)
        {
            var lesson = await _db.Lessons
                .Include(l => l.Course)
                    .ThenInclude(c => c.Level)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null) return NotFound();

            if (lesson.Course != null && !lesson.Course.IsFree && !await HasAccessAsync())
            {
                TempData["LockedMessage"] = "Bạn cần đăng ký Membership để truy cập bài học này.";
                return RedirectToAction("Index", "Membership");
            }

            // Chỉ lấy bài tập thuộc đúng lesson đang mở
            var exercises = await _db.Exercises
                .Where(e => e.LessonId == lesson.LessonId)
                .OrderBy(e => e.SortOrder)
                .ToListAsync();

            var vm = new LessonViewModel
            {
                LessonId = lesson.LessonId,
                CourseId = lesson.CourseId,
                LessonTitle = lesson.Title,
                CourseTitle = lesson.Course?.Title ?? "",
                LevelName = lesson.Course?.Level?.LevelName ?? "",
                Content = lesson.Content,
                VideoUrl = lesson.VideoUrl,

                VocabularyItems = MapExercises(exercises, "Vocabulary"),
                KanjiItems = MapExercises(exercises, "Kanji"),
                GrammarItems = MapExercises(exercises, "Grammar"),
                ReadingItems = MapExercises(exercises, "Reading"),
                ListeningItems = MapExercises(exercises, "Listening")
            };

            return View(vm);
        }

        // GET: /learn/Course/StartBasic
        public async Task<IActionResult> StartBasic()
        {
            var basicCourse = await _db.Courses
                .Include(c => c.Level)
                .OrderBy(c => c.CourseId)
                .FirstOrDefaultAsync(c => c.Level != null && c.Level.LevelName == "N5");

            return basicCourse != null
                ? RedirectToAction("Detail", new { id = basicCourse.CourseId })
                : RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CheckExercise([FromBody] ExerciseAnswerRequest request)
        {
            var questionIds = request.Answers
                .Select(x => x.QuestionId)
                .ToList();

            var questions = await _db.Questions
                .Where(q => q.ExerciseId == request.ExerciseId &&
                            questionIds.Contains(q.QuestionId))
                .Include(q => q.AnswerOptions)
                .ToListAsync();

            var result = questions.Select(question =>
            {
                var selected = request.Answers
                    .FirstOrDefault(a => a.QuestionId == question.QuestionId);

                var correctOption = question.AnswerOptions
                    .FirstOrDefault(o => o.IsCorrect);

                bool isCorrect = selected != null &&
                                 correctOption != null &&
                                 selected.OptionId == correctOption.OptionId;

                return new
                {
                    questionId = question.QuestionId,
                    isCorrect,
                    correctOptionId = correctOption?.OptionId
                };
            });

            return Json(new { answers = result });
        }

        [HttpGet]
        public async Task<IActionResult> ExercisePractice(int exerciseId)
        {
            var questions = await _db.Questions
                .Where(q => q.ExerciseId == exerciseId)
                .OrderBy(q => q.SortOrder)
                .Select(q => new
                {
                    questionId = q.QuestionId,
                    questionText = q.QuestionText,
                    questionType = q.QuestionType,
                    options = q.AnswerOptions.Select(o => new
                    {
                        optionId = o.OptionId,
                        answerText = o.AnswerText
                    }).ToList()
                })
                .ToListAsync();

            return Json(new { questions });
        }

        // ── Helpers ──

        private static List<LessonExerciseItemViewModel> MapExercises( List<CoreLibrary.Data.Entities.Exercise> exercises,string exerciseType)
        {
            return exercises
                .Where(e => e.ExerciseType == exerciseType)
                .OrderBy(e => e.SortOrder)
                .Select(e => new LessonExerciseItemViewModel
                {
                    ExerciseId = e.ExerciseId,
                    Title = e.Title,
                    Content = e.Content,
                    AudioUrl = e.AudioUrl,
                    StrokeOrderUrl = e.StrokeOrderUrl,
                    SortOrder = e.SortOrder,

                    Questions = e.Questions
                        .OrderBy(q => q.SortOrder)
                        .Select(q => new ExerciseQuestionViewModel
                        {
                            QuestionId = q.QuestionId,
                            QuestionText = q.QuestionText,
                            QuestionType = q.QuestionType,
                            SortOrder = q.SortOrder,

                            Options = q.AnswerOptions
                                .Select(a => new ExerciseAnswerOptionViewModel
                                {
                                    OptionId = a.OptionId,
                                    AnswerText = a.AnswerText,
                                    IsCorrect = a.IsCorrect
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();
        }

        private async Task<int?> GetCurrentStudentIdAsync()
        {
            var user = await _auth.GetCurrentUserAsync(HttpContext);
            if (user == null || user.Role != RoleConst.STUDENT)
                return null;
            return user.AccountId;
        }

        private async Task<bool> HasAccessAsync()
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (!studentId.HasValue) return false;

            return await _db.StudentMemberships
                .AnyAsync(m => m.StudentId == studentId.Value
                            && m.IsActive
                            && m.EndDate > DateTime.UtcNow);
        }
    }
}