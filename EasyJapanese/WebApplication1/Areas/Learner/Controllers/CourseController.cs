using CoreLibrary.Authentication;
using CoreLibrary.Const;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreWeb.Areas.Learner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public async Task<IActionResult> Index(
            string? level,
            string? price,
            string? sort,
            string? q,
            CancellationToken cancellationToken)
        {
            int? studentId = await GetCurrentStudentIdAsync();

            var hasMembership = studentId.HasValue && await _db.StudentMemberships
                .AnyAsync(m => m.StudentId == studentId.Value
                            && m.IsActive
                            && m.EndDate > DateTime.UtcNow);

            var query = _db.Courses
                .Where(c => c.IsPublished)
                .Include(c => c.Level)
                .Include(c => c.Mentor)
                .AsQueryable();

            if (!hasMembership)
                query = query.Where(c => c.IsFree);

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

            var enrolledIds = new HashSet<int>();
            if (studentId.HasValue)
            {
                var ids = await _db.Enrollments
                   .Where(e => e.StudentId == studentId.Value)
                   .Select(e => e.CourseId)
                   .ToListAsync();
                enrolledIds = new HashSet<int>(ids);
            }

                if (!string.IsNullOrWhiteSpace(level))
            {
                query = query.Where(c => c.Level != null && c.Level.LevelName == level);
            }

            if (string.Equals(price, "free", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => c.IsFree);
            }
            else if (string.Equals(price, "paid", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => !c.IsFree);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(c =>
                    c.Title.Contains(term) ||
                    (c.Description != null && c.Description.Contains(term)));
            }

            query = sort?.ToLowerInvariant() switch
            {
                "name" => query.OrderBy(c => c.Title),
                "name-desc" => query.OrderByDescending(c => c.Title),
                _ => query.OrderBy(c => c.Level!.SortOrder)
                         .ThenByDescending(c => c.CreatedAt)
            };

            var courses = await query.ToListAsync(cancellationToken);

            ViewBag.HasMembership = hasMembership;
            ViewBag.RecommendedLevelId = recommendedLevelId;
            ViewBag.RecommendedLevelName = recommendedLevelName;
            ViewBag.EnrolledCourseIds = enrolledIds;
            ViewBag.CurrentLevel = level ?? "";
            ViewBag.CurrentPrice = price ?? "";
            ViewBag.CurrentSort = string.IsNullOrWhiteSpace(sort) ? "level" : sort.ToLowerInvariant();
            ViewBag.CurrentQuery = q ?? "";

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

            var studentId = await GetCurrentStudentIdAsync();
            var completedLessonIds = new HashSet<int>();
            var progressPercent = 0;

            if (studentId.HasValue)
            {
                var lessonIds = course.Lessons.Select(l => l.LessonId).ToList();
                if (lessonIds.Count > 0)
                {
                    var completed = await _db.LessonProgresses
                        .Where(lp => lp.StudentId == studentId.Value
                                  && lp.IsCompleted
                                  && lessonIds.Contains(lp.LessonId))
                        .Select(lp => lp.LessonId)
                        .ToListAsync();
                    completedLessonIds = new HashSet<int>(completed);
                    progressPercent = (int)Math.Round(100.0 * completed.Count / lessonIds.Count);
                }
            }

            ViewBag.CompletedLessonIds = completedLessonIds;
            ViewBag.ProgressPercent = progressPercent;
            ViewBag.IsEnrolled = studentId.HasValue
                && course.Enrollments.Any(e => e.StudentId == studentId.Value);

            // Query FlashcardSets for this course
            var flashcardSets = await _db.FlashcardSets
                .Include(fs => fs.Flashcards)
                .Where(fs => fs.CourseId == id && fs.Flashcards.Any())
                .ToListAsync();
            ViewBag.FlashcardSets = flashcardSets;

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

            // Danh sách bài học trong khóa để hiển thị sidebar
            var courseLessons = await _db.Lessons
                .Where(l => l.CourseId == lesson.CourseId)
                .OrderBy(l => l.SortOrder)
                .Select(l => new { l.LessonId, l.Title, l.SortOrder })
                .ToListAsync();

            if (lesson.Course != null && !lesson.Course.IsFree && !await HasAccessAsync())
            {
                TempData["LockedMessage"] = "Bạn cần đăng ký Membership để truy cập bài học này.";
                return RedirectToAction("Index", "Membership");
            }

            // Mở bài học = ghi danh khóa + đánh dấu đã truy cập bài
            bool isCompleted = false;
            var studentId = await GetCurrentStudentIdAsync();
            var completedLessonIds = new HashSet<int>();
            if (studentId.HasValue)
            {
                EnsureEnrolled(studentId.Value, lesson.CourseId);
                var progress = await TrackLessonAccessAsync(studentId.Value, lesson.LessonId);
                await _db.SaveChangesAsync();
                isCompleted = progress.IsCompleted;

                var courseLessonIds = courseLessons.Select(l => l.LessonId).ToList();
                var completedIds = await _db.LessonProgresses
                    .Where(lp => lp.StudentId == studentId.Value
                              && lp.IsCompleted
                              && courseLessonIds.Contains(lp.LessonId))
                    .Select(lp => lp.LessonId)
                    .ToListAsync();
                completedLessonIds = new HashSet<int>(completedIds);
            }

            // Chỉ lấy bài tập thuộc đúng lesson đang mở
            var exercises = await _db.Exercises
                .Where(e => e.LessonId == lesson.LessonId)
                .OrderBy(e => e.SortOrder)
                .ToListAsync();

            var materials = await _db.LessonMaterials
                .Where(m => m.LessonId == lesson.LessonId)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();

            // Quiz gắn với lesson này (nếu có) + attempt tốt nhất của student hiện tại
            var lessonQuizzes = await _db.Quizzes
                .Where(q => q.LessonId == lesson.LessonId)
                .Include(q => q.Questions)
                .OrderBy(q => q.SortOrder).ThenBy(q => q.CreatedAt)
                .ToListAsync();

            Dictionary<int, QuizAttempt> bestAttempts = new();
            if (studentId.HasValue && lessonQuizzes.Any())
            {
                var quizIds = lessonQuizzes.Select(q => q.QuizId).ToList();
                bestAttempts = await _db.QuizAttempts
                    .Where(a => a.StudentId == studentId.Value && quizIds.Contains(a.QuizId))
                    .GroupBy(a => a.QuizId)
                    .Select(g => g.OrderByDescending(a => a.Score).First())
                    .ToDictionaryAsync(a => a.QuizId);
            }

            var vm = new LessonViewModel
            {
                LessonId = lesson.LessonId,
                CourseId = lesson.CourseId,
                LessonTitle = CoreLibrary.Utility.EncodingFixer.FixMojibake(lesson.Title),
                CourseTitle = CoreLibrary.Utility.EncodingFixer.FixMojibake(lesson.Course?.Title ?? ""),
                LevelName = lesson.Course?.Level?.LevelName ?? "",
                Content = CoreLibrary.Utility.EncodingFixer.FixMojibake(lesson.Content),
                VideoUrl = lesson.VideoUrl,
                IsCompleted = isCompleted,

                AllLessons = courseLessons.Select(l => new SidebarLessonItem
                {
                    LessonId = l.LessonId,
                    Title = CoreLibrary.Utility.EncodingFixer.FixMojibake(l.Title),
                    SortOrder = l.SortOrder,
                    IsCurrent = l.LessonId == lesson.LessonId,
                    IsCompleted = completedLessonIds.Contains(l.LessonId)
                }).ToList(),

                MaterialItems = materials.Select(m => new LessonMaterialViewModel
                {
                    MaterialId = m.MaterialId,
                    Title = CoreLibrary.Utility.EncodingFixer.FixMojibake(m.Title),
                    Url = m.Url,
                    FileType = m.FileType ?? "link"
                }).ToList(),

                KanjiItems = MapExercises(exercises, ExerciseTypeConst.KANJI),
                GrammarItems = MapExercises(exercises, ExerciseTypeConst.GRAMMAR),
                ReadingItems = MapExercises(exercises, ExerciseTypeConst.READING),
                ListeningItems = MapExercises(exercises, ExerciseTypeConst.LISTENING),

                QuizItems = lessonQuizzes.Select(q => new LessonQuizItemViewModel
                {
                    QuizId = q.QuizId,
                    Title = q.Title,
                    Duration = q.Duration,
                    PassScore = q.PassScore,
                    QuestionCount = q.Questions?.Count ?? 0,
                    BestScore = bestAttempts.TryGetValue(q.QuizId, out var a) ? a.Score : (int?)null,
                    HasPassed = bestAttempts.TryGetValue(q.QuizId, out var p) && p.IsPassed
                }).ToList()
            };

            return View(vm);
        }

        // POST: /learn/Course/UpdateProgress
        [HttpPost]
        public async Task<IActionResult> UpdateProgress([FromBody] LessonProgressRequest request)
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (!studentId.HasValue) return Unauthorized();

            var lesson = await _db.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == request.LessonId);
            if (lesson == null) return NotFound();

            if (lesson.Course != null && !lesson.Course.IsFree && !await HasAccessAsync())
                return Forbid();

            var progress = await _db.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.StudentId == studentId.Value
                                        && lp.LessonId == request.LessonId);
            if (progress == null)
            {
                progress = new CoreLibrary.Data.Entities.LessonProgress
                {
                    StudentId = studentId.Value,
                    LessonId = request.LessonId
                };
                _db.LessonProgresses.Add(progress);
            }

            // WatchedSeconds chỉ tăng, không lùi
            if (request.WatchedSeconds > progress.WatchedSeconds)
                progress.WatchedSeconds = request.WatchedSeconds;
            if (request.IsCompleted)
                progress.IsCompleted = true;
            progress.LastAccessedAt = DateTime.UtcNow;

            EnsureEnrolled(studentId.Value, lesson.CourseId);
            await _db.SaveChangesAsync();

            var coursePercent = await UpdateCourseCompletionAsync(studentId.Value, lesson.CourseId);

            return Json(new
            {
                success = true,
                isCompleted = progress.IsCompleted,
                coursePercent
            });
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

        // Ghi danh nếu chưa có (chưa gọi SaveChanges — caller tự save)
        private void EnsureEnrolled(int studentId, int courseId)
        {
            var exists = _db.Enrollments.Local
                    .Any(e => e.StudentId == studentId && e.CourseId == courseId)
                || _db.Enrollments
                    .Any(e => e.StudentId == studentId && e.CourseId == courseId);
            if (exists) return;

            _db.Enrollments.Add(new CoreLibrary.Data.Entities.Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow
            });
        }

        // Tạo/cập nhật LessonProgress khi mở bài (chưa SaveChanges)
        private async Task<CoreLibrary.Data.Entities.LessonProgress> TrackLessonAccessAsync(int studentId, int lessonId)
        {
            var progress = await _db.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LessonId == lessonId);
            if (progress == null)
            {
                progress = new CoreLibrary.Data.Entities.LessonProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    LastAccessedAt = DateTime.UtcNow
                };
                _db.LessonProgresses.Add(progress);
            }
            else
            {
                progress.LastAccessedAt = DateTime.UtcNow;
            }
            return progress;
        }

        // Đánh dấu Enrollment.CompletedAt khi học hết bài, trả về % hoàn thành
        private async Task<int> UpdateCourseCompletionAsync(int studentId, int courseId)
        {
            var lessonIds = await _db.Lessons
                .Where(l => l.CourseId == courseId)
                .Select(l => l.LessonId)
                .ToListAsync();
            if (lessonIds.Count == 0) return 0;

            var completedCount = await _db.LessonProgresses
                .CountAsync(lp => lp.StudentId == studentId
                               && lp.IsCompleted
                               && lessonIds.Contains(lp.LessonId));

            var enrollment = await _db.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
            if (enrollment != null)
            {
                bool allDone = completedCount >= lessonIds.Count;
                if (allDone && enrollment.CompletedAt == null)
                {
                    enrollment.CompletedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
                else if (!allDone && enrollment.CompletedAt != null)
                {
                    enrollment.CompletedAt = null;
                    await _db.SaveChangesAsync();
                }
            }

            return (int)Math.Round(100.0 * completedCount / lessonIds.Count);
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

        // GET: /learn/Course/Flashcards/{setId}
        [HttpGet("~/learn/Course/Flashcards/{setId}")]
        public async Task<IActionResult> GetFlashcardsJson(int setId)
        {
            var flashcards = await _db.Flashcards
                .Where(f => f.FlashcardSetId == setId)
                .OrderBy(f => f.FlashcardId)
                .Select(f => new
                {
                    id = f.FlashcardId,
                    front = f.FrontText,
                    back = f.BackText
                })
                .ToListAsync();

            return Json(flashcards);
        }
    }
}