using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class RegisterController : Controller
    {
        [Route("Register")]
        [Route("Register/Init")]
        public IActionResult Init()
        {
            return View();
        }
    }
}
