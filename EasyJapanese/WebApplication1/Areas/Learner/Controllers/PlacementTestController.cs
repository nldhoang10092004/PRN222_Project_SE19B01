using CoreLibrary.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class PlacementTestController : Controller
    {
        private readonly IAuthenticationService _auth;

        public PlacementTestController(IAuthenticationService auth)
        {
            _auth = auth;
        }

        [HttpGet]
        public async Task<IActionResult> Start(CancellationToken cancellationToken)
        {
            var currentUser = await _auth.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            return View();
        }
    }
}
