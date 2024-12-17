using Microsoft.AspNetCore.Mvc;

namespace ogani_master.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class Home : Controller
    {        public IActionResult Index()
        {
            return View();
        }
    }
}
