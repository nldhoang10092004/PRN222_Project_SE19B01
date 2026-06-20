using CoreLibrary.Authentication;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreWeb.Areas.Learner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IAuthenticationService _auth;

        public DashboardController(AppDbContext db, IAuthenticationService auth)
        {
            _db = db;
            _auth = auth;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var studentId = currentUser.AccountId;

            var student = await _db.Students
                .Where(s => s.StudentId == studentId)
                .Select(s => new { s.FullName })
                .FirstOrDefaultAsync(cancellationToken);

            if (student == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var now = DateTime.UtcNow;

            var hasActiveMembership = await _db.StudentMemberships
                .AnyAsync(m => m.StudentId == studentId && m.IsActive && m.EndDate > now, cancellationToken);

            var placement = await _db.StudentPlacementResults
                .Where(r => r.StudentId == studentId && r.CompletedAt != null)
                .OrderByDescending(r => r.CompletedAt)
                .Select(r => new JlptGoalViewModel
                {
                    RecommendedLevelName = r.RecommendedLevel != null ? r.RecommendedLevel.LevelName : "Cơ bản",
                    Score = r.Score,
                    TotalPoints = r.TotalPoints,
                    CompletedAt = r.CompletedAt!.Value
                })
                .FirstOrDefaultAsync(cancellationToken);

            var hasCompletedPlacementTest = placement != null;

            var vm = new LearnerDashboardViewModel
            {
                FullName = student.FullName,
                HasActiveMembership = hasActiveMembership,
                HasCompletedPlacementTest = hasCompletedPlacementTest,
                JlptGoal = placement
            };

            // ── Trạng thái 1: chưa có membership ──
            if (!hasActiveMembership)
            {
                vm.AvailablePlans = await _db.SubscriptionPlans
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.DurationDays)
                    .Select(p => new PlanPreviewViewModel
                    {
                        PlanId = p.PlanId,
                        PlanName = p.PlanName,
                        Price = p.Price,
                        DurationDays = p.DurationDays
                    })
                    .ToListAsync(cancellationToken);

                return View(vm);
            }

            // ── Trạng thái 2: có membership, chưa placement test ──
            if (!hasCompletedPlacementTest)
            {
                return View(vm);
            }

            // ── Trạng thái 3/4: có membership + đã test → full dashboard ──
            vm.ContinueLesson = await _db.LessonProgresses
                .Where(lp => lp.StudentId == studentId && !lp.IsCompleted)
                .OrderByDescending(lp => lp.LastAccessedAt)
                .Select(lp => new ContinueLessonViewModel
                {
                    LessonId = lp.LessonId,
                    CourseId = lp.Lesson.CourseId,
                    LevelName = lp.Lesson.Course.Level.LevelName,
                    LessonTitle = lp.Lesson.Title,
                    CourseTitle = lp.Lesson.Course.Title,
                    WatchedSeconds = lp.WatchedSeconds,
                    PercentComplete = lp.Lesson.Duration.HasValue && lp.Lesson.Duration > 0
                        ? (int)Math.Min(100, 100.0 * lp.WatchedSeconds / lp.Lesson.Duration.Value)
                        : 0
                })
                .FirstOrDefaultAsync(cancellationToken);

            vm.Stats = new LearnerStatsViewModel
            {
                CoursesEnrolled = await _db.Enrollments.CountAsync(e => e.StudentId == studentId, cancellationToken),
                LessonsCompleted = await _db.LessonProgresses.CountAsync(lp => lp.StudentId == studentId && lp.IsCompleted, cancellationToken),
                FlashcardsLearned = await _db.Flashcards.CountAsync(f => f.StudentId == studentId, cancellationToken)
            };

            vm.FlashcardsDue = await _db.Flashcards
                .Where(f => f.StudentId == studentId && (f.NextReviewAt == null || f.NextReviewAt <= now))
                .OrderBy(f => f.NextReviewAt)
                .Take(4)
                .Select(f => new FlashcardReviewItemViewModel
                {
                    FlashcardId = f.FlashcardId,
                    FrontText = f.FrontText,
                    BackText = f.BackText,
                    CourseTitle = f.Course != null ? f.Course.Title : null
                })
                .ToListAsync(cancellationToken);

            vm.StreakDays = await CalculateStreakAsync(studentId, cancellationToken);

            return View(vm);
        }

        private async Task<int> CalculateStreakAsync(int studentId, CancellationToken cancellationToken)
        {
            var accessDates = await _db.LessonProgresses
                .Where(lp => lp.StudentId == studentId)
                .Select(lp => lp.LastAccessedAt.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync(cancellationToken);

            if (accessDates.Count == 0) return 0;

            var streak = 0;
            var expected = DateTime.UtcNow.Date;

            foreach (var date in accessDates)
            {
                if (date == expected)
                {
                    streak++;
                    expected = expected.AddDays(-1);
                }
                else if (date < expected)
                {
                    break;
                }
            }

            return streak;
        }

        
    }
}