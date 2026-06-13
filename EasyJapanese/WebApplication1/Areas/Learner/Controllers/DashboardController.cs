using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Learner.Controllers
{
    /// <summary>
    /// Trang chủ khu vực học viên.
    /// URL: /learn/dashboard
    /// </summary>
    [Area("Learner")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Trang chủ học viên";
            return View();
        }
    }
}
