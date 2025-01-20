using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;

namespace ogani_master.Controllers
{
    public class ProductController(OganiMaterContext _context) : Controller
    {
        private OganiMaterContext context = _context;

        protected async Task<User?> GetCurrentUser()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId != null)
            {
                var user = await context.users.FirstOrDefaultAsync(u => u.UserId == userId);
                return user;
            }
            return null;
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            // Nếu từ khóa tìm kiếm rỗng, hiển thị toàn bộ sản phẩm
            if (string.IsNullOrWhiteSpace(query))
            {
                ViewBag.Message = "Please enter a keyword to search.";
                var allProducts = _context.Products.ToList();
                return View(allProducts);
            }

            // Tìm kiếm sản phẩm theo tên hoặc mô tả
            var products = _context.Products
                .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
                .ToList();

            // Trả về View hiển thị kết quả tìm kiếm
            ViewBag.Settings = context.Settings.ToList();
            ViewBag.Message = products.Any()
                ? $"Found {products.Count} result(s) for '{query}'"
                : $"No products found for '{query}'";

            return View(products);
        }

        public async Task<IActionResult> Index(int? uid, int? CateID)
        {
           

            int? userId = HttpContext.Session.GetInt32("UserID");

            if (!uid.HasValue)
            {
                return NotFound("Product ID is required.");
            }

            var product = await context.Products.FirstOrDefaultAsync(p => p.PRO_ID == uid);

            if (product == null)
            {
                return NotFound("Product not found.");
            }
          
            List<Product> listNewProducts = await this.context.Products
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(4)
                    .ToListAsync();

            FavoritesModel? isFavorite = await this.context.Favorites.FirstOrDefaultAsync(f => f.UserID == userId && f.ProductId == uid);
            ViewBag.Product = product;
            ViewBag.Settings = context.Settings.ToList();
            ViewBag.ListNewProducts = listNewProducts;
            ViewBag.CurrentUser = await GetCurrentUser();
            ViewBag.isFavorite = isFavorite != null ? true : false;

            return View();
        }
    }
}