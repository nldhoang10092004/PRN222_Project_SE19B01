using System.Threading;
using System.Threading.Tasks;
using CoreLibrary.Authentication;
using CoreLibrary.Const;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.Authentication;

namespace WebApplication1.Areas.Admin.Controllers
{
    /// <summary>
    /// Cổng đăng nhập riêng cho Admin.
    /// URL: /admin/login
    /// </summary>
    [Area("Admin")]
    [Route("admin")]
    public class AdminLoginController : Controller
    {
        private readonly IAuthenticationService _auth;

        public AdminLoginController(IAuthenticationService auth)
        {
            _auth = auth;
        }

        [HttpGet]
        [Route("login")]
        public IActionResult Index(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Areas/Admin/Views/AdminLogin/Index.cshtml", new LoginViewModel());
        }

        [HttpPost]
        [Route("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model, string? returnUrl, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View("~/Areas/Admin/Views/AdminLogin/Index.cshtml", model);
            }

            var result = await _auth.LoginAsync(model.Email, model.Password, HttpContext, cancellationToken);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Đăng nhập thất bại.");
                ViewData["ReturnUrl"] = returnUrl;
                return View("~/Areas/Admin/Views/AdminLogin/Index.cshtml", model);
            }

            if (result.User?.Role != RoleConst.ADMIN)
            {
                await _auth.LogoutAsync(HttpContext);
                ModelState.AddModelError(string.Empty, "Tài khoản này không có quyền truy cập khu vực quản trị.");
                return View("~/Areas/Admin/Views/AdminLogin/Index.cshtml", model);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            TempData["FlashMessage"] = result.Message;
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }
    }
}
