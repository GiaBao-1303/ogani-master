using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ogani_master.Areas.Admin.DTO;
using ogani_master.Models;

namespace ogani_master.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly OganiMaterContext _context;
        private readonly IWebHostEnvironment _hostEnv;
        public ProductsController(OganiMaterContext context, IWebHostEnvironment hostEnv)
        {
            _context = context;
            _hostEnv = hostEnv;
        }

        protected async Task<User?> GetCurrentUser()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            return userId.HasValue
                ? await _context.users.FirstOrDefaultAsync(u => u.UserId == userId)
                : null;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            ViewBag.CurrentUser = await GetCurrentUser();
            return View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.Include(p => p.Category)
                                                 .FirstOrDefaultAsync(p => p.PRO_ID == id);
            if (product == null) return NotFound();

            ViewBag.CurrentUser = await GetCurrentUser();
            return View(product);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["CAT_ID"] = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "CAT_ID", "Name");
            ViewBag.CurrentUser = await GetCurrentUser();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProductDTO request)
        {
            if (!ModelState.IsValid)
            {
                ViewData["CAT_ID"] = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "CAT_ID", "Name");
                return View(request);
            }

            var currentUser = await GetCurrentUser();
            var newProduct = new Product
            {
                CAT_ID = request.CAT_ID,
                Name = request.Name,
                Intro = request.Intro,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                Unit = request.Unit == "Other" && !string.IsNullOrEmpty(Request.Form["OtherUnit"])
                        ? Request.Form["OtherUnit"]
                        : request.Unit,
                Rate = request.Rate,
                Description = request.Description,
                Details = request.Details,
                quantity = request.quantity,
                CreatedBy = currentUser?.UserName,
                CreatedDate = DateTime.Now
            };

            if (request.Avatar != null)
            {
                string extension = Path.GetExtension(request.Avatar.FileName);
                string newFileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(_hostEnv.WebRootPath, "data", "product", newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Avatar.CopyToAsync(stream);
                }

                newProduct.Avatar = newFileName;
            }

            _context.Add(newProduct);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["CAT_ID"] = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "CAT_ID", "Name", product.CAT_ID);
            ViewBag.CurrentUser = await GetCurrentUser();
            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] ProductDTO productdto)
        {
            if (id != productdto.PRO_ID) return NotFound();

            if (!ModelState.IsValid)
            {
                // Trả về View với dữ liệu ProductDTO được chuyển đổi sang Product
                var productInvalid = new Product
                {
                    PRO_ID = productdto.PRO_ID,
                    CAT_ID = productdto.CAT_ID,
                    Name = productdto.Name,
                    Intro = productdto.Intro,
                    Price = productdto.Price,
                    DiscountPrice = productdto.DiscountPrice,
                    Unit = productdto.Unit,
                    Rate = productdto.Rate,
                    quantity = productdto.quantity,
                    Description = productdto.Description,
                    Details = productdto.Details
                };

                ViewData["CAT_ID"] = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "CAT_ID", "Name", productdto.CAT_ID);
                ViewBag.CurrentUser = await GetCurrentUser();

                return View(productInvalid);
            }

            // Lấy sản phẩm hiện tại từ database
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return NotFound();

            try
            {
                // Cập nhật các trường dữ liệu
                existingProduct.Name = productdto.Name;
                existingProduct.Intro = productdto.Intro;
                existingProduct.Price = productdto.Price;
                existingProduct.DiscountPrice = productdto.DiscountPrice;
                existingProduct.Unit = productdto.Unit == "Other" && !string.IsNullOrEmpty(Request.Form["OtherUnit"])
                                        ? Request.Form["OtherUnit"]
                                        : productdto.Unit;
                existingProduct.Rate = productdto.Rate;
                existingProduct.quantity = productdto.quantity;
                existingProduct.Description = productdto.Description;
                existingProduct.Details = productdto.Details;

                // Xử lý avatar mới (nếu có)
                if (productdto.Avatar != null)
                {
                    string extension = Path.GetExtension(productdto.Avatar.FileName);
                    string newFileName = $"{Guid.NewGuid()}{extension}";
                    string filePath = Path.Combine(_hostEnv.WebRootPath, "data", "product", newFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await productdto.Avatar.CopyToAsync(stream);
                    }

                    // Xóa file ảnh cũ (nếu tồn tại)
                    if (!string.IsNullOrEmpty(existingProduct.Avatar))
                    {
                        string oldFilePath = Path.Combine(_hostEnv.WebRootPath, "data", "product", existingProduct.Avatar);
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                    }

                    existingProduct.Avatar = newFileName;
                }

                // Cập nhật thông tin người chỉnh sửa
                existingProduct.UpdatedBy = (await GetCurrentUser())?.UserName;
                existingProduct.UpdatedDate = DateTime.Now;

                // Cập nhật sản phẩm vào database
                _context.Update(existingProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(productdto.PRO_ID)) return NotFound();
                throw;
            }
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.Include(p => p.Category)
                                                 .FirstOrDefaultAsync(p => p.PRO_ID == id);
            if (product == null) return NotFound();

            ViewBag.CurrentUser = await GetCurrentUser();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.PRO_ID == id);
        }
    }
}
