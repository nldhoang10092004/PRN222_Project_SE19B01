using System.Threading;
using System.Threading.Tasks;
using CoreLibrary.Authentication;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.Authentication;

namespace WebApplication1.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IAuthenticationService _auth;

        public RegisterController(IAuthenticationService auth)
        {
            _auth = auth;
        }

        [HttpGet]
        [Route("Register")]
        [Route("Register/Init")]
        public IActionResult Init()
        {
            return View(new RegisterStep1ViewModel());
        }

        [HttpPost]
        [Route("Register")]
        [Route("Register/Init")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Init(RegisterStep1ViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _auth.StartRegistrationAsync(
                new RegisterRequest
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Password = model.Password
                },
                HttpContext,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Đăng ký thất bại.");
                return View(model);
            }

            TempData["FlashMessage"] = result.Message;
            return RedirectToAction(nameof(Verify), new { email = model.Email });
        }

        [HttpGet]
        [Route("Register/Verify")]
        public IActionResult Verify(string? email)
        {
            var model = new RegisterStep2ViewModel { Email = email ?? string.Empty };
            return View(model);
        }

        [HttpPost]
        [Route("Register/Verify")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(RegisterStep2ViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _auth.VerifyRegistrationAsync(model.Email, model.Otp, HttpContext, cancellationToken);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Xác nhận OTP thất bại.");
                return View(model);
            }

            TempData["FlashMessage"] = result.Message;
            return RedirectToAction("Index", "Login");
        }
    }
}
