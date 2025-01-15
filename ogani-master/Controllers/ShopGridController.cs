using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;

namespace ogani_master.Controllers
{
    public class ShopGridController : Controller
    {
        private readonly OganiMaterContext _context;
        public ShopGridController(OganiMaterContext context)
        {
            _context = context;
        }
        protected async Task<User?> GetCurrentUser()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId != null)
            {
                var user = await _context.users.FirstOrDefaultAsync(u => u.UserId == userId);
                return user;
            }
            return null;
        }
        public async Task<List<FavoritesModel>> getFavorites()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");

            List<FavoritesModel> favorites = await this._context.Favorites.Include(f => f.Product).Where(f => f.UserID == userId).ToListAsync();

            return favorites;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Favorites = await this.getFavorites() ?? new List<FavoritesModel>();
            ViewBag.CurrentUser = await this.GetCurrentUser();

            return View();
        }
    }
}
