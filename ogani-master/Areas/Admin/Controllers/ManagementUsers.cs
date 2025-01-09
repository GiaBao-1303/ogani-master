using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ogani_master.Areas.Admin.dto;
using ogani_master.enums;
using ogani_master.Models;

namespace ogani_master.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ManagementUsers : Controller
    {


        private readonly OganiMaterContext context;
        private readonly IWebHostEnvironment _hostEnv;
        public ManagementUsers(OganiMaterContext _context, IWebHostEnvironment hostEnv)
        {
            context = _context;
            _hostEnv = hostEnv;
        }

        protected async Task<User?> GetCurrentUser()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId != null)
            {
                var user = await context.users.FirstOrDefaultAsync(u => u.UserId == userId);
                return user;
            }
            return null;
        }

        public async Task<IActionResult> Index()
        {
            User? currentUser = await GetCurrentUser();

            if (currentUser == null || (currentUser.Role > (int)UserRole.Moderator))
            {
                return RedirectToAction("SignInPage", "Auth");
            }

            List<User> listUsers = await this.context.users.Where(u => u.Role > currentUser.Role).ToListAsync();

            ViewBag.ListUsers = listUsers;
            ViewBag.CurrentUser = await GetCurrentUser();
            return View();
        }

        [HttpPost]
        [Route("/Management/ChangeRole")]
        public async Task<IActionResult> ChangeRole(ChangeRoleDto changeRoleDto)
        {
            if (!ModelState.IsValid) {
                return View(changeRoleDto);
            }

            User? currentUser = await GetCurrentUser();

            if (currentUser == null || (currentUser.Role > (int)UserRole.Moderator))
            {
                return RedirectToAction("SignInPage", "Auth");
            }

            if (changeRoleDto.selectedRole <= currentUser.Role) { 
                return BadRequest("Invalid role");
            }

            User? user = await this.context.users.FirstOrDefaultAsync(u => u.UserId == changeRoleDto.UserId);

            if(user == null) return RedirectToAction("SignInPage", "Auth");

            if (!Enum.IsDefined(typeof(UserRole), changeRoleDto.selectedRole)) return BadRequest("Invalid role.");

            user.Role = changeRoleDto.selectedRole;

            await this.context.SaveChangesAsync();

            return RedirectToAction("Index", "ManagementUsers");
        }

        [HttpPost]
        [Route("/Management/ChangeStatus")]
        public async Task<IActionResult> ChangeStatus(ChangeStatus changeStatus)
        {
            if (!ModelState.IsValid)
            {
                return View(changeStatus);
            }

            User? currentUser = await GetCurrentUser();

            if (currentUser == null || (currentUser.Role > (int)UserRole.Moderator))
            {
                return RedirectToAction("SignInPage", "Auth");
            }

            User? user = await this.context.users.FirstOrDefaultAsync(u => u.UserId == changeStatus.UserId);

            if(user == null) return NotFound();

            if (currentUser.Role >= user.Role) return BadRequest();

            user.Status = changeStatus.selectedStatus;

            await this.context.SaveChangesAsync();

            return RedirectToAction("Index", "ManagementUsers");
        }

        public async Task<IActionResult> Detail(int? uid)
        {
            User? currentUser = await GetCurrentUser();

            if(currentUser == null) return RedirectToAction("SignInPage", "Auth");

            User? userInfo = await this.context.users.FirstOrDefaultAsync(u => u.UserId == uid);

            if(userInfo == null) return NotFound();


            ViewBag.CurrentUser = currentUser;
            ViewBag.User = userInfo;
            return View();
        }
    }
}
