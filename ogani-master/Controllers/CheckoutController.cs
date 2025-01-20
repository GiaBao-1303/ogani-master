using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;

namespace ogani_master.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly OganiMaterContext _context;
        public CheckoutController(OganiMaterContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            ViewBag.Settings = _context.Settings.ToList();
            return View();
        }
    }
}
