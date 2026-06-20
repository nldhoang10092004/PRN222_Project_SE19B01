using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class CourseController : Controller
    {
        // GET: /learn/Course
        public IActionResult Index()
        {
            return View();
        }

        // GET: /learn/Course/Detail/1
        public IActionResult Detail(int id = 1)
        {
            return View();
        }

        // GET: /learn/Course/Lesson/1
        public IActionResult Lesson(int id = 1)
        {
            return View();
        }
    }
}
