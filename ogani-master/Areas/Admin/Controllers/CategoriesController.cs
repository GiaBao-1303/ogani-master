using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;

namespace ogani_master.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly OganiMaterContext _context;

        public CategoriesController(OganiMaterContext context)
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

        // GET: Admin/Categories
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            pageSize = pageSize > 100 ? 100 : pageSize;

            ViewBag.CurrentUser = await GetCurrentUser();

            var categories = await _context.Categories
                                           .OrderBy(c => c.CAT_ID)
                                           .Skip((page - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToListAsync();

            return View(categories);
        }

        // GET: Admin/Categories/Details 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CAT_ID == id);
            if (category == null)
            {
                return NotFound();
            }

            ViewBag.CurrentUser = await GetCurrentUser();
            return View(category);
        }

        // GET: Admin/Categories/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.CurrentUser = await GetCurrentUser();
            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Category category)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await GetCurrentUser();
                if (currentUser != null) {
                    category.CreatedBy = currentUser.UserName;
                }
                category.CreatedDate = DateTime.Now;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewBag.CurrentUser = await GetCurrentUser();
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] Category category)
        {
            if (id != category.CAT_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CAT_ID == id);
                    if (existingCategory == null)
                    {
                        return NotFound();
                    }
                    category.CreatedBy = existingCategory.CreatedBy;
                    category.CreatedDate = existingCategory.CreatedDate;
                    var currentUser = await GetCurrentUser();
                    category.UpdatedBy = currentUser?.UserName;
                    category.UpdatedDate = DateTime.Now;
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                { 
                    if (!CategoryExists(category.CAT_ID ?? 0))  
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
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CAT_ID == id);
            if (category == null)
            {
                return NotFound();
            }
            ViewBag.CurrentUser = await GetCurrentUser();

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.CAT_ID == id);
            if (category != null)
            {
             
                foreach (var product in category.Products)
                {
                    product.CAT_ID = null;
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CAT_ID == id);
        }
    }
}