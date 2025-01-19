using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.dto;

using ogani_master.Models;

namespace ogani_master.Controllers
{
    public class ShopGridController : Controller
    {
        
        private readonly int numberOfSaleOffs = 6;
        private readonly int numberOfNewProduct = 6;
        private readonly int numberOfProduct = 12;
        public OganiMaterContext context;

        public ShopGridController(OganiMaterContext _context) {
            this.context = _context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(QueryProduct? queryProduct)
        {

            List<Product> saleOffs = await this.context.Products
                .Take(this.numberOfSaleOffs)
                .Include(p => p.Category)
                .Where(p => p.DiscountPrice != null)
                .ToListAsync();

            List<Product> listProducts;

            var query = this.context.Products
                .Take(this.numberOfProduct)
                .Where(p => p.DiscountPrice == null);

            if (queryProduct != null)
            {
                if (queryProduct.unit != null)
                {
                    query = query.Where(p => p.Unit == queryProduct.unit);
                }

                if (queryProduct.category != null)
                {
                    query = query.Where(p => p.CAT_ID == queryProduct.category);
                }
            }

            listProducts = await query
                .Include(p => p.Category)
                .ToListAsync();

            List<Product> listNewProducts = await this.context.Products
                .OrderByDescending(p => p.CreatedDate)
                .Take(this.numberOfNewProduct)
                .ToListAsync();

            List<Category> categories = await this.context.Categories.ToListAsync();

            decimal minPriceDb = await this.context.Products.MinAsync(p => p.Price);
            decimal maxPriceDb = await this.context.Products.MaxAsync(p => p.Price);

            ViewBag.SaleOffs = saleOffs;
            ViewBag.ListProducts = listProducts;
            ViewBag.ListNewProduct = listNewProducts;
            ViewBag.Categories = categories;
            ViewBag.CategoryActive = queryProduct?.category;
            ViewBag.UnitActive = queryProduct?.unit;
            ViewBag.MinPrice = (int)minPriceDb;
            ViewBag.MaxPrice = (int)maxPriceDb;

            return View();
        }
    }
}
