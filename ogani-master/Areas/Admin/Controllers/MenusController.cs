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
    public class MenusController : Controller
    {
        private readonly OganiMaterContext _context;

        public MenusController(OganiMaterContext context)
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

        // GET: Admin/Menus
        public async Task<IActionResult> Index()
        {
            var menuList = await _context.Menus.ToListAsync(); // Lấy tất cả menu từ cơ sở dữ liệu

            // Tạo một Dictionary để ánh xạ MEN_ID -> Title của menu
            var menuTitles = menuList.ToDictionary(m => m.MEN_ID, m => m.Title);

            // Tạo ViewBag để ánh xạ PARENT_ID sang Title
            var parentTitles = menuList.ToDictionary(
                m => m.MEN_ID,
                m =>
                {
                    if (m.PARENT_ID == null)
                    {
                        return "No Parent"; // Không có parent
                    }
                    var parentMenu = menuList.FirstOrDefault(p => p.MEN_ID == m.PARENT_ID);
                    return parentMenu?.Title ?? "No Parent"; // Nếu không tìm thấy parent, trả về "No Parent"
                });

            ViewBag.MenuTitles = parentTitles; // Truyền ánh xạ PARENT_ID -> Title vào ViewBag
            ViewBag.CurrentUser = await GetCurrentUser(); // Lấy thông tin người dùng hiện tại
            return View(menuList);
        }
        

        // GET: Admin/Menus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menu = await _context.Menus
                .FirstOrDefaultAsync(m => m.MEN_ID == id);
            if (menu == null)
            {
                return NotFound();
            }
            ViewBag.CurrentUser = await GetCurrentUser();

            return View(menu);
        }


        private List<SelectListItem> BuildMenuSelectList(List<Menu> menus, int? parentId = null, string prefix = "", HashSet<int> visited = null)
        {
            if (visited == null) visited = new HashSet<int>();

            List<SelectListItem> items = new List<SelectListItem>();

            var filteredMenus = menus.Where(m => m.PARENT_ID == parentId).ToList();

            foreach (var menu in filteredMenus)
            {
                // Tránh đệ quy vô hạn
                if (visited.Contains(menu.MEN_ID)) continue;

                visited.Add(menu.MEN_ID);

                items.Add(new SelectListItem
                {
                    Value = menu.MEN_ID.ToString(),
                    Text = prefix + menu.Title
                });

                items.AddRange(BuildMenuSelectList(menus, menu.MEN_ID, prefix + "--", visited));
            }

            return items;
        }



        // GET: Admin/Menus/Create
        public async Task<IActionResult> Create()
        {
            var allmenu = await _context.Menus.ToListAsync();
            ViewBag.PARENT_ID = BuildMenuSelectList(allmenu);

            ViewBag.CurrentUser = await GetCurrentUser();
            return View();
        }

        // POST: Admin/Menus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MEN_ID,PARENT_ID,Title,Url,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy")] Menu menu)
        {
            if (ModelState.IsValid)
            {
                menu.CreatedDate = DateTime.Now;
                var currentUser = await GetCurrentUser();
                menu.CreatedBy = currentUser?.UserName;
                // Kiểm tra và đảm bảo PARENT_ID đã được gán
                if (menu.PARENT_ID == 0)
                {
                    menu.PARENT_ID = null;  // Nếu không có menu cha, gán PARENT_ID = null
                }
                _context.Add(menu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var allMenus = await _context.Menus.ToListAsync();
            ViewBag.PARENT_ID = BuildMenuSelectList(allMenus);
            ViewBag.CurrentUser = await GetCurrentUser();
            return View(menu);
        }

        // GET: Admin/Menus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound();
            }
            var allMenus = await _context.Menus.ToListAsync();
            ViewBag.PARENT_ID = BuildMenuSelectList(allMenus);
            ViewBag.CurrentUser = await GetCurrentUser();

            return View(menu);
        }

        // POST: Admin/Menus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MEN_ID,PARENT_ID,Title,Url,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy")] Menu menu)
        {

            if (id != menu.MEN_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await GetCurrentUser();

                    // Lấy menu cũ từ DbContext để giữ lại CreatedBy và CreatedDate
                    var existingMenu = await _context.Menus.AsNoTracking().FirstOrDefaultAsync(m => m.MEN_ID == id);

                    if (existingMenu == null)
                    {
                        return NotFound();
                    }

                    // Giữ nguyên CreatedBy và CreatedDate, chỉ cập nhật UpdatedBy và UpdatedDate
                    menu.CreatedBy = existingMenu.CreatedBy;  // Giữ lại người tạo cũ
                    menu.CreatedDate = existingMenu.CreatedDate;  // Giữ lại thời gian tạo cũ

                    menu.UpdatedBy = currentUser?.UserName;  // Cập nhật người chỉnh sửa
                    menu.UpdatedDate = DateTime.Now;  // Cập nhật thời gian chỉnh sửa

                    // Đảm bảo không có nhiều đối tượng có cùng MEN_ID đang được theo dõi
                    _context.Entry(menu).State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuExists(menu.MEN_ID))
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

            var allMenus = await _context.Menus.ToListAsync();
            ViewBag.PARENT_ID = BuildMenuSelectList(allMenus);
            ViewBag.CurrentUser = await GetCurrentUser();

            return View(menu);
        }

        // GET: Admin/Menus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menu = await _context.Menus
                .FirstOrDefaultAsync(m => m.MEN_ID == id);
            if (menu == null)
            {
                return NotFound();
            }

            // Kiểm tra menu có phải là parent của các menu khác không
            var childMenus = _context.Menus.Where(m => m.PARENT_ID == id).ToList();
            if (childMenus.Any())
            {
                // Hiển thị thông báo, không ngăn cấm xóa nữa vì bạn sẽ giữ các menu con
                ViewBag.ErrorMessage = "This menu has child menus, but they will be kept when you delete the parent menu.";
            }

            ViewBag.CurrentUser = await GetCurrentUser();
            return View(menu);
        }

        // POST: Admin/Menus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu != null)
            {
                // Lấy tất cả các menu con
                var childMenus = _context.Menus.Where(m => m.PARENT_ID == id).ToList();

                // Cập nhật các menu con để không còn liên kết với menu cha (set PARENT_ID = null)
                foreach (var childMenu in childMenus)
                {
                    childMenu.PARENT_ID = null;
                    _context.Menus.Update(childMenu);  // Cập nhật lại các menu con
                }

                // Xóa menu cha
                _context.Menus.Remove(menu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MenuExists(int id)
        {
            return _context.Menus.Any(e => e.MEN_ID == id);
        }
    }
}
