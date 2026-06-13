using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Admin.Controllers
{
    /// <summary>
    /// Trang chủ khu vực quản trị.
    /// URL: /admin
    /// </summary>
    [Area("Admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard quản trị";
            return View();
        }
    }
}
