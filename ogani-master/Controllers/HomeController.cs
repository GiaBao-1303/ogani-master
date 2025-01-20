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

        public async Task<List<FavoritesModel>> getFavorites()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");

            List<FavoritesModel> favorites = await this.context.Favorites.Include(f => f.Product).Where(f => f.UserID == userId).ToListAsync();

            return favorites;
        }


        public async Task<IActionResult> Index()
        {
            List<Product> products = context.Products.ToList();
            ViewBag.Products = products;

            var allBlogs = await context.Blogs
               .OrderByDescending(b => b.CreatedAt)
               .ToListAsync();
            var mainBlogs = allBlogs.Take(15).ToList();
            var recentBlogs = allBlogs.Take(5).ToList();
            ViewData["MainBlogs"] = mainBlogs;
            ViewData["RecentBlogs"] = recentBlogs;

            var categories = await context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();

            ViewBag.AllDepartmentCategory = categories.FirstOrDefault(c => c.DisplayOrder == 1);
            ViewBag.Categories = categories.Where(c => c.DisplayOrder > 1).ToList();

            var mainBanner = await context.Banners.OrderBy(b => b.DisplayOrder).FirstOrDefaultAsync();
            ViewBag.MainBanner = mainBanner;

            var sideBanners = await context.Banners.OrderBy(b => b.DisplayOrder).Skip(1).Take(2).ToListAsync();
            ViewBag.SideBanners = sideBanners;
            ViewBag.Settings = context.Settings.ToList();
            ViewBag.Favorites = await this.getFavorites();
            ViewBag.CurrentUser = await this.GetCurrentUser();

            var categoryWithProducts = categories.Select(cat => new
            {
                Category = cat,
                RepresentativeProduct = products.FirstOrDefault(p => p.CAT_ID == cat.CAT_ID)
            }).ToList();

            ViewBag.CategoryWithProducts = categoryWithProducts;

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
