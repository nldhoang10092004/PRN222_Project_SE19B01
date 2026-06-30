using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using CoreLibrary.Const;

namespace WebApplication1.Areas.Admin.Controllers
{
    /// <summary>
    /// Trang chủ khu vực quản trị.
    /// URL: /admin
    /// </summary>
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard quản trị";
            
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            
            // Total Revenue this month
            var monthlyRevenue = await _context.Transactions
                .Where(t => t.PaymentStatus == "Completed" && t.CreatedAt >= currentMonthStart)
                .SumAsync(t => t.FinalAmount);
                
            // New Students this month
            var newStudents = await _context.Accounts
                .Where(a => a.Role == RoleConst.STUDENT && a.CreatedAt >= currentMonthStart)
                .CountAsync();
                
            // Active Courses
            var activeCourses = await _context.Courses
                .Where(c => c.IsPublished)
                .CountAsync();
                
            // Recent Transactions
            var recentTransactions = await _context.Transactions
                .Include(t => t.Student)
                .Include(t => t.Plan)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewData["MonthlyRevenue"] = monthlyRevenue;
            ViewData["NewStudents"] = newStudents;
            ViewData["ActiveCourses"] = activeCourses;

            return View(recentTransactions);
        }
    }
}
