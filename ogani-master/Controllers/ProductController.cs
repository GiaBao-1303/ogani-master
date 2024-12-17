using Microsoft.AspNetCore.Mvc;

namespace ogani_master.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
