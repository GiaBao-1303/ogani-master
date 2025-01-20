using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;

namespace ogani_master.Controllers
{
    public class ContactController : Controller
    {
        private readonly OganiMaterContext _context;

        // Inject DbContext vào constructor
        public ContactController(OganiMaterContext context)
        {
            _context = context;
        }

        // GET: /Contact
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Contact/SendMessage
        [HttpPost]
        public IActionResult SendMessage(string name, string email, string phoneNumber, string message)
        {
            if (ModelState.IsValid)
            {
                // Tạo đối tượng ContactAd mới
                var contact = new ContactAd
                {
                    Name = name,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Message = message,
                    Status = "Unseen",  // Trạng thái mặc định là "Unread"
                    SentAt = DateTime.UtcNow
                };

                // Lưu vào cơ sở dữ liệu
                _context.Contacts.Add(contact);
                _context.SaveChanges();

                // Thêm thông báo thành công và chuyển hướng về trang Index
                TempData["SuccessMessage"] = "Bạn đã gửi thành công";
                return RedirectToAction("Index");
            }

            // Nếu form không hợp lệ, trả lại form với thông báo lỗi
            return View("Index");
        }
    }
}
