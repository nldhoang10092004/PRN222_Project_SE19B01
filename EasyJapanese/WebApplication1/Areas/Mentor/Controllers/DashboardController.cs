using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Mentor.Controllers
{
    /// <summary>
    /// Trang chủ khu vực giảng viên.
    /// URL: /mentor
    /// </summary>
    [Area("Mentor")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Trang chủ giảng viên";
            return View();
        }
    }
}
