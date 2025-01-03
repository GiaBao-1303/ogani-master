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

        public async Task<IActionResult> Index(int? uid)
        {

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
            ViewBag.Product = product;
            ViewBag.ListNewProducts = listNewProducts;

            return View();
        }
    }
}
