using Microsoft.AspNetCore.Mvc;
using CoreLibrary.Data;
using CoreLibrary.Data.Entities;
using CoreLibrary.Const;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CoreLibrary.Filters.AppAuthorize(RoleConst.ADMIN)]
    [Route("admin/transactions")]
    public class TransactionsController : Controller
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            ViewData["Title"] = "Quản lý Giao dịch";

            var query = _context.Transactions
                .Include(t => t.Student)
                .Include(t => t.Plan)
                .Include(t => t.Voucher)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(t => t.Student.FullName.ToLower().Contains(searchString) || 
                                         t.Student.StudentNavigation.Email.ToLower().Contains(searchString) ||
                                         t.PaymentRef.ToLower().Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Paid" || statusFilter == "Completed")
                {
                    query = query.Where(t => t.PaymentStatus == "Paid" || t.PaymentStatus == "Completed");
                }
                else
                {
                    query = query.Where(t => t.PaymentStatus == statusFilter);
                }
            }

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewData["SearchString"] = searchString;
            ViewData["StatusFilter"] = statusFilter;

            return View(transactions);
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Student)
                .ThenInclude(s => s.StudentNavigation)
                .Include(t => t.Plan)
                .Include(t => t.Voucher)
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            if (transaction == null) return NotFound();

            return PartialView("_TransactionDetails", transaction);
        }
    }
}
