using Microsoft.AspNetCore.Mvc;

namespace ogani_master.Controllers
{
    public class ShopingCartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
