using CoreLibrary.Authentication;
using CoreWeb.Areas.Learner.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class BlogController : Controller
    {
        private readonly IAuthenticationService _auth;

        public BlogController(IAuthenticationService auth)
        {
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

            var vm = new CommunityIndexViewModel
            {
                FullName = currentUser.FullName 
            };

            return View(vm);
        }
    }
}