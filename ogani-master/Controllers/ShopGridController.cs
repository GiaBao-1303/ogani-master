using Microsoft.AspNetCore.Mvc;

namespace ogani_master.Controllers
{
    public class ShopGridController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
