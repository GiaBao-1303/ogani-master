using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;

namespace ogani_master.Controllers
{
    public class ContactController : Controller
    {
        private readonly OganiMaterContext _context;
        public ContactController(OganiMaterContext context)
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
        public async Task<List<Category>> getCategories()
        {
            return await _context.Categories.ToListAsync();
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.Settings = _context.Settings.ToList();
            ViewBag.Favorites = await this.getFavorites() ?? new List<FavoritesModel>();
            ViewBag.CurrentUser = await this.GetCurrentUser();
            ViewBag.Categories = await getCategories();
            return View();
        }
    }
}
