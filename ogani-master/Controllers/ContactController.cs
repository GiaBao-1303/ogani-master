using Microsoft.AspNetCore.Mvc;

namespace ogani_master.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
