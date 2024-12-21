using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            var oganiMaterContext = _context.Products.Include(p => p.Category);
            return View(await oganiMaterContext.ToListAsync());
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.PRO_ID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public IActionResult Create()
        {
            ViewData["CAT_ID"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "CAT_ID", "CAT_ID");
            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProductDTO request)
        {
            var product = new Product
            {
                PRO_ID = request.PRO_ID,
                CAT_ID = request.CAT_ID,
                Name = request.Name,
                Intro = request.Intro,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                Unit = request.Unit,
                Rate = request.Rate ,
                Description = request.Description,
                Details = request.Details,
                CreatedBy = request.CreatedBy,
                

            };
            if (ModelState.IsValid)
            {
                string? newImageFileName = null;
                if (request.Avatar != null)
                {
                    var extension = Path.GetExtension(request.Avatar.FileName);
                    newImageFileName = $"{Guid.NewGuid().ToString()}{extension}";
                    var filePath = Path.Combine(_hostEnv.WebRootPath, "data", "product", newImageFileName);
                    request.Avatar.CopyTo(new FileStream(filePath, FileMode.Create));
                }

                if (newImageFileName != null) product.Avatar = newImageFileName;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CAT_ID"] = new SelectList(_context.Categories, "CAT_ID", "CAT_ID");

            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CAT_ID"] = new SelectList(_context.Categories, "CAT_ID", "CAT_ID", product.CAT_ID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] Product product)
        {
            if (id != product.PRO_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.PRO_ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CAT_ID"] = new SelectList(_context.Categories, "CAT_ID", "CAT_ID", product.CAT_ID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.PRO_ID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.PRO_ID == id);
        }
    }
}
