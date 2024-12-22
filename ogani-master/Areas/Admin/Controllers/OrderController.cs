using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;

namespace ogani_master.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController(OganiMaterContext _context) : Controller
    {
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

        public IActionResult Index()
        {
            ViewBag.CurrentUser = GetCurrentUser();
            return View();
        }
    }
}
