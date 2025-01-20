    using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using ogani_master.Models;
using System.Threading.Tasks;
using System.Linq;

namespace ogani_master.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContactAdController : Controller
    {
        private readonly OganiMaterContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ContactAdController(OganiMaterContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Hàm lấy thông tin người dùng hiện tại
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

        // GET: Admin/ContactAd/Index
        public async Task<IActionResult> Index(string searchQuery)
        {
            // Truy vấn cơ sở dữ liệu để lấy các contact
            var query = _context.Contacts.AsQueryable();

            // Tìm kiếm nếu có searchQuery
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(c =>
                    c.Name.Contains(searchQuery) ||  // Tìm kiếm theo tên
                    c.Email.Contains(searchQuery)    // Tìm kiếm theo email
                );
            }

            // Lưu giá trị tìm kiếm vào ViewBag để hiển thị lại trên giao diện
            ViewBag.SearchQuery = searchQuery;

            // Lấy thông tin người dùng hiện tại
            ViewBag.CurrentUser = await GetCurrentUser();

            // Lấy tất cả các contact đã lọc theo tiêu chí tìm kiếm
            var contacts = await query.OrderBy(c => c.ContactId).ToListAsync();

            return View(contacts);
        }

        // GET: Admin/ContactAd/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            // Nếu trạng thái là "Unseen", thì chuyển thành "Seen" khi vào trang
            if (contact.Status == "Unseen")
            {
                contact.Status = "Seen";
                _context.Update(contact);
                await _context.SaveChangesAsync();
            }

            ViewBag.CurrentUser = await GetCurrentUser();
            return View(contact);
        }

        // POST: Admin/ContactAd/ChangeStatus
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return Json(new { success = false });
            }

            // Chuyển trạng thái từ "Unseen" -> "Seen" hoặc ngược lại
            if (contact.Status == "Unseen")
            {
                contact.Status = "Seen";  // Đã xem
            }
            else
            {
                contact.Status = "Unseen";  // Chưa xem
            }

            _context.Update(contact);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        // GET: Admin/ContactAd/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            // Lấy thông tin người dùng hiện tại
            ViewBag.CurrentUser = await GetCurrentUser();

            return View(contact);
        }

        // POST: Admin/ContactAd/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        
     
    }
}
