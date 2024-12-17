using Microsoft.AspNetCore.Mvc;

namespace ogani_master.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
