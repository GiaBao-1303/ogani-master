using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;

namespace ogani_master.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        protected readonly OganiMaterContext _context;
        public BaseController(OganiMaterContext context)
        { 
        _context = context;
        
        }
        // Lấy thông tin người dùng hiện tại
        protected async Task<User?> GetCurrentUser()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId.HasValue)
            {
                return await _context.users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
            }
            return null;
        
        
        }
    }
}
