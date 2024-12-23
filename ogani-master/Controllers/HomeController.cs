using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;

namespace ogani_master.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly OganiMaterContext context;

        public HomeController(ILogger<HomeController> logger, OganiMaterContext _context)
        {
            _logger = logger;
            context = _context;
        }

        protected async Task<User?> GetCurrentUser() 
        { 
            int? userId = HttpContext.Session.GetInt32("UserID"); 
            if (userId != null) { 
                var user = await context.users.FirstOrDefaultAsync(u => u.UserId == userId); 
                return user; 
            } 
            return null; 
        }

        public async Task<IActionResult> Index()
        {

            List<Product> products = context.Products.ToList();

            ViewBag.Products = products;
            ViewBag.CurrentUser = await this.GetCurrentUser();
            return View();

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
    }
}
