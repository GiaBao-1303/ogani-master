using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using ogani_master.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection.Metadata;

namespace ogani_master.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogAdController : Controller
    {
        private readonly OganiMaterContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BlogAdController(OganiMaterContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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

        // GET: Admin/BlogAd/Index
        public async Task<IActionResult> Index()
        {
            var blogs = _context.Blogs.ToList();
            ViewBag.CurrentUser = await GetCurrentUser();
            return View(blogs);
        }

        // GET: Admin/BlogAd/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(m => m.BlogId == id);
            if (blog == null)
            {
                return NotFound();
            }
            ViewBag.CurrentUser = await GetCurrentUser();
            return View(blog);
        }

        // GET: Admin/BlogAd/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.CurrentUser = await GetCurrentUser();
            return View();
        }

        // POST: Admin/BlogAd/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                // Handle the image upload
                if (image != null && image.Length > 0)
                {
                    string fileName = Path.GetFileNameWithoutExtension(image.FileName);
                    string extension = Path.GetExtension(image.FileName);
                    string uniqueFileName = fileName + DateTime.Now.Ticks.ToString() + extension;
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "blog", uniqueFileName);

                    // Save the image to the folder
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    // Assign the image file name to the blog's Image property
                    blog.Image = uniqueFileName;
                }
                blog.CreatedAt = DateTime.Now;
                blog.UpdatedAt = DateTime.Now;  
                // Add the blog to the database
                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // GET: Admin/BlogAd/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Blog? blog = await _context.Blogs.FirstOrDefaultAsync(b => b.BlogId == id);

            if (blog == null)
            {
                return NotFound();
            }

            ViewBag.CurrentUser = await GetCurrentUser();

            return View(blog);
        }

        // POST: Admin/BlogAd/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog, IFormFile image)
        {
            if (id != blog.BlogId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle the image upload if a new image is provided
                    if (image != null && image.Length > 0)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(image.FileName);
                        string extension = Path.GetExtension(image.FileName);
                        string uniqueFileName = fileName + DateTime.Now.Ticks.ToString() + extension;
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "blog", uniqueFileName);

                        // Save the image to the folder
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        // Update the blog's Image property with the new image
                        blog.Image = uniqueFileName;
                    }

                    _context.Update(blog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.BlogId))
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
            return View(blog);
        }

        // GET: Admin/BlogAd/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(m => m.BlogId == id);
            if (blog == null)
            {
                return NotFound();
            }

            ViewBag.CurrentUser = await GetCurrentUser();

            return View(blog);
        }

        // POST: Admin/BlogAd/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.BlogId == id);
        }
    }
}
