using Microsoft.AspNetCore.Mvc;

namespace ogani_master.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
