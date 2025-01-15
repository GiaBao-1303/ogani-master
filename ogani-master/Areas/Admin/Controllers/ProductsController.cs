using Azure.Core;
using X.PagedList;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ogani_master.Areas.Admin.DTO;
using ogani_master.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using X.PagedList.Extensions;

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

        public async Task<IActionResult> Index(string searchQuery, int? page)
        {
           
            int pageSize = 12; // Số sản phẩm trên mỗi trang
            int pageNumber = page ?? 1;
            var query = _context.Products.Include(p => p.Category).AsQueryable();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p =>
                    (!string.IsNullOrEmpty(p.Name) && p.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                );
            }
            ViewBag.SearchQuery = searchQuery;
            ViewBag.CurrentUser = await GetCurrentUser();
            var pagedProducts = query.OrderBy(p => p.PRO_ID).ToPagedList(pageNumber, pageSize);
            return View(pagedProducts); 
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
        /// Post creates
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProductDTO request)
        {
            Console.WriteLine($"Description: {request.Description}");

            if (request.quantity < 0)
            {
                ModelState.AddModelError("quantity", "The quantity of products cannot be negative.");
                ViewData["CAT_ID"] = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "CAT_ID", "Name");
                return View(request);
            }

            if (request.DiscountPrice > request.Price)
            {
                TempData["ErrorMessage-discountPrice"] = "Discount Price cannot be greater than the original Price. Please enter a valid Discount Price.";
                ViewData["CAT_ID"] = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "CAT_ID", "Name", request.CAT_ID);
                return View(request);
            }

            // Kiểm tra nếu không có ảnh được upload
            if (request.Avatar == null || request.Avatar.Length == 0)
            {
                ModelState.AddModelError("Avatar", "Please upload an image for the product.");
                ViewData["CAT_ID"] = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "CAT_ID", "Name", request.CAT_ID);
                TempData["ErrorMessage"] = "Product must have an image. Please upload a photo.";
                return View(request);
            }

            if (!ModelState.IsValid)
            {
                ViewData["CAT_ID"] = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "CAT_ID", "Name", request.CAT_ID);
                TempData["ErrorMessage"] = "Please check again for promotional prices, quantities, and product image.";
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

            // Xử lý upload ảnh
            string extension = Path.GetExtension(request.Avatar.FileName);
            string newFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(_hostEnv.WebRootPath, "data", "product", newFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Avatar.CopyToAsync(stream);
            }

            newProduct.Avatar = newFileName;

            _context.Add(newProduct);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product created successfully!";
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

            if (productdto.DiscountPrice > productdto.Price)
            {
                ModelState.AddModelError("DiscountPrice", "Discount Price cannot be greater than the original Price.");
            }
            if (productdto.quantity < 0)
            {
                ModelState.AddModelError("quantity", "The quantity of products cannot be negative.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["CAT_ID"] = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "CAT_ID", "Name", productdto.CAT_ID);
                ViewBag.CurrentUser = await GetCurrentUser();
                TempData["ErrorMessage"] = "Please check the input fields and try again.";
                return View(productdto);
            }

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return NotFound();

            try
            {
                existingProduct.Name = productdto.Name;
                existingProduct.Intro = productdto.Intro;
                existingProduct.Price = productdto.Price;
                existingProduct.CAT_ID = productdto.CAT_ID;
                existingProduct.DiscountPrice = productdto.DiscountPrice;
                existingProduct.Unit = productdto.Unit == "Other" && !string.IsNullOrEmpty(Request.Form["OtherUnit"])
                    ? Request.Form["OtherUnit"]
                    : productdto.Unit;
                existingProduct.quantity = productdto.quantity;
                existingProduct.Description = productdto.Description ?? "No description provided.";
                existingProduct.Details = productdto.Details ?? "No details provided.";
                existingProduct.UpdatedBy = (await GetCurrentUser())?.UserName;
                existingProduct.UpdatedDate = DateTime.Now;
                // Nếu người dùng upload ảnh mới thì thay ảnh, nếu không thì giữ nguyên ảnh cũ
                if (productdto.Avatar != null && productdto.Avatar.Length > 0)
                {
                    string extension = Path.GetExtension(productdto.Avatar.FileName);
                    string newFileName = $"{Guid.NewGuid()}{extension}";
                    string filePath = Path.Combine(_hostEnv.WebRootPath, "data", "product", newFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await productdto.Avatar.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(existingProduct.Avatar))
                    {
                        string oldFilePath = Path.Combine(_hostEnv.WebRootPath, "data", "product", existingProduct.Avatar);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    existingProduct.Avatar = newFileName;
                }
                else
                {
                    existingProduct.Avatar = existingProduct.Avatar;
                }
                _context.Update(existingProduct);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(productdto.PRO_ID))
                    return NotFound();

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
            /// Xoá ảnh 
            if (!string.IsNullOrEmpty(product.Avatar))
            {
                string filePath = Path.Combine(_hostEnv.WebRootPath, "data", "product", product.Avatar);
                if (System.IO.File.Exists(filePath))
                { 
                System.IO.File.Delete(filePath);
                }
            }
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
