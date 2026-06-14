using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        [Route("Login")]
        [Route("Login/Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
