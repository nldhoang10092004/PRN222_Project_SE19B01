using System.Threading;
using System.Threading.Tasks;
using CoreLibrary.Authentication;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.Authentication;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthenticationService _auth;

        public LoginController(IAuthenticationService auth)
        {
            _auth = auth;
        }

        [HttpGet]
        [Route("Login")]
        [Route("Login/Index")]
        public IActionResult Index()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [Route("Login")]
        [Route("Login/Index")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _auth.LoginAsync(model.Email, model.Password, HttpContext, cancellationToken);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Đăng nhập thất bại.");
                return View(model);
            }

            TempData["FlashMessage"] = result.Message;
            var role = result.User?.Role;
            if (role == CoreLibrary.Const.RoleConst.ADMIN)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            if (role == CoreLibrary.Const.RoleConst.MENTOR)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Learner" });
            }
            return RedirectToAction("Index", "Dashboard", new { area = "Learner" });
        }

        [HttpPost]
        [Route("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _auth.LogoutAsync(HttpContext);
            return RedirectToAction("Index", "Home");
        }
    }
}
