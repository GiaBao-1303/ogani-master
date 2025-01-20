using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ogani_master.Controllers
{
    public class BlogController : Controller
    {
        private readonly OganiMaterContext _context;

        public BlogController(OganiMaterContext context)
        {
            _context = context;
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

        public async Task<IActionResult> Index(string query = "")
        {
            var allBlogs = await _context.Blogs
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            int? userId = HttpContext.Session.GetInt32("UserID");
            ViewBag.Settings = _context.Settings.ToList();
            ViewBag.CurrentUser = await this._context.users.FirstOrDefaultAsync(u => u.UserId == userId);

            var mainBlogs = allBlogs.Take(15).ToList();
            var recentBlogs = allBlogs.Take(5).ToList();

            // Tìm kiếm nếu có từ khóa
            if (!string.IsNullOrEmpty(query))
            {
                mainBlogs = allBlogs
                    .Where(b => b.Title.Contains(query) || b.Content.Contains(query))
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(15)
                    .ToList();

                recentBlogs = allBlogs
                    .Where(b => b.Title.Contains(query) || b.Content.Contains(query))
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToList();

                // Lưu trữ kết quả tìm kiếm vào ViewData
                ViewData["SearchQuery"] = query;

                // Thông báo nếu không tìm thấy kết quả
                if (!mainBlogs.Any())
                {
                    ViewData["NoResults"] = "Không có kết quả tìm kiếm nào!";
                }
            }

            // Trả về view với dữ liệu từ ViewData
            ViewData["MainBlogs"] = mainBlogs;
            ViewData["RecentBlogs"] = recentBlogs;
            ViewBag.Favorites = await this.getFavorites();
            ViewBag.Categories = await getCategories();
            return View(allBlogs); // Trả về tất cả bài blog để sử dụng nếu cần trong view
        }

        // Action để hiển thị chi tiết một bài blog và xử lý tìm kiếm trong chi tiết
        [HttpGet]
        public async Task<IActionResult> Detail(int id, string query = "")
        {
            // Lấy bài viết chi tiết
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.BlogId == id);
            if (blog == null)
            {
                return NotFound();
            }
            var relatedBlogs = new List<Blog>();
            // Nếu có tìm kiếm từ trang chi tiết
            if (!string.IsNullOrEmpty(query))
            {
                ViewData["SearchQuery"] = query;

                // Tìm kiếm các blog có liên quan đến từ khóa
                 relatedBlogs = await _context.Blogs
                    .Where(b => b.Title.Contains(query) || b.Content.Contains(query))
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(3)
                    .ToListAsync();

                ViewData["RelatedBlogs"] = relatedBlogs;

                if (!relatedBlogs.Any())
                {
                    ViewData["NoResults"] = "Không có kết quả tìm kiếm nào!";
                }
            }
            else
            {
                // Nếu không tìm kiếm, lấy 3 bài viết gợi ý (không bao gồm bài viết hiện tại)
                 relatedBlogs = await _context.Blogs
                    .Where(b => b.BlogId != id)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(3)
                    .ToListAsync();

                ViewData["RelatedBlogs"] = relatedBlogs;
            }
            // Lấy 3 bài viết gợi ý (không bao gồm bài viết hiện tại)
             relatedBlogs = await _context.Blogs
                .Where(b => b.BlogId != id)
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .ToListAsync();
            int? userId = HttpContext.Session.GetInt32("UserID");
            ViewBag.Settings = _context.Settings.ToList();
            ViewBag.CurrentUser = await this._context.users.FirstOrDefaultAsync(u => u.UserId == userId);
            ViewBag.Favorites = await this.getFavorites();
            // Truyền dữ liệu sang view
            ViewData["RelatedBlogs"] = relatedBlogs;
            return View(blog);
        }
    }
}
