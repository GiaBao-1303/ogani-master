using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using ogani_master.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection.Metadata;
using X.PagedList.Extensions;

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
        public async Task<IActionResult> Index(string searchQuery)
        {
            // Truy vấn cơ sở dữ liệu để lấy các bài blog
            var query = _context.Blogs.AsQueryable();

            // Tìm kiếm nếu có searchQuery
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(b =>
                    b.Title.Contains(searchQuery) ||  // Tìm kiếm theo tiêu đề
                    b.Content.Contains(searchQuery)   // Tìm kiếm theo nội dung
                );
            }

            // Lưu giá trị tìm kiếm vào ViewBag để hiển thị lại trên giao diện
            ViewBag.SearchQuery = searchQuery;

            // Lấy thông tin người dùng hiện tại
            ViewBag.CurrentUser = await GetCurrentUser();

            // Lấy tất cả các bài blog đã lọc theo tiêu chí tìm kiếm
            var blogs = await query.OrderBy(b => b.BlogId).ToListAsync(); // Lấy dữ liệu và sắp xếp theo BlogId

            return View(blogs); // Trả về view và truyền vào danh sách blog đã lọc
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog, IFormFile image)
        {
            // Kiểm tra tính hợp lệ của mô hình
            if (ModelState.IsValid)
            {
                // Xử lý tải lên hình ảnh
                if (image != null && image.Length > 0)
                {
                    // Lấy tên file và phần mở rộng
                    string fileName = Path.GetFileNameWithoutExtension(image.FileName);
                    string extension = Path.GetExtension(image.FileName);
                    string uniqueFileName = fileName + DateTime.Now.Ticks.ToString() + extension;

                    // Đảm bảo thư mục lưu hình ảnh tồn tại
                    string directoryPath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "blog");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Lưu hình ảnh vào thư mục
                    string filePath = Path.Combine(directoryPath, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    // Gán tên file của hình ảnh vào thuộc tính Image của blog
                    blog.Image = uniqueFileName;
                }
                else
                {
                    // Nếu không có ảnh được upload, bạn có thể hiển thị lỗi
                    ModelState.AddModelError("Image", "Image is required");
                    return View(blog);  // Trả lại view cùng với lỗi
                }

                // Đặt thời gian tạo và cập nhật cho blog
                blog.CreatedAt = DateTime.Now;
                blog.UpdatedAt = DateTime.Now;

                // Thêm bài viết vào cơ sở dữ liệu
                _context.Add(blog);
                await _context.SaveChangesAsync();

                // Thêm thông báo thành công
                TempData["SuccessMessage"] = "Bài viết đã được tạo thành công!";

                // Chuyển hướng đến trang danh sách bài viết
                return RedirectToAction(nameof(Index));
            }

            // Nếu không hợp lệ, trả lại view cùng với dữ liệu bài viết
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
                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "blog", blog.Image);

                // Xóa ảnh cũ nếu tồn tại
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

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
