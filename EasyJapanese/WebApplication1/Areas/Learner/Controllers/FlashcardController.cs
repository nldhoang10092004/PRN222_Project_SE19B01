using CoreLibrary.Authentication;
using CoreLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class FlashcardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IAuthenticationService _auth;

        public FlashcardController(AppDbContext db, IAuthenticationService auth)
        {
            _db = db;
            _auth = auth;
        }

        [HttpGet]
        public async Task<IActionResult> Review(CancellationToken cancellationToken)
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
