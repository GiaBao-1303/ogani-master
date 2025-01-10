using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;

namespace ogani_master.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : Controller
    {
        private readonly OganiMaterContext _context;

        public SettingsController(OganiMaterContext context)
        {
            _context = context;
        }

        // Lấy thông tin người dùng từ session
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

        // GET: Admin/Settings
        public async Task<IActionResult> Index()
        {
            ViewBag.CurrentUser = await GetCurrentUser();  // Lấy thông tin người dùng hiện tại
            var settings = await _context.Settings.ToListAsync();  // Lấy danh sách setting
            return View(settings);
        }

        // GET: Admin/Settings/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.CurrentUser = await GetCurrentUser();
            return View();
        }

        // POST: Admin/Settings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Value")] Setting setting)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await GetCurrentUser();
                setting.CreatedDate = DateTime.Now;
                setting.CreatedBy = currentUser?.UserName ?? "System";  // Lấy tên người dùng hoặc mặc định là "System"
                _context.Add(setting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CurrentUser = await GetCurrentUser();
            return View(setting);
        }

        // GET: Admin/Settings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.SET_ID == id);
            if (setting == null)
            {
                return NotFound();
            }

            ViewBag.CurrentUser = await GetCurrentUser();
            return View(setting);
        }

        // POST: Admin/Settings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SET_ID,Name,Value")] Setting setting)
        {
            if (id != setting.SET_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var currentUser = await GetCurrentUser();
                var existingSetting = await _context.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.SET_ID == id);

                if (existingSetting == null)
                {
                    return NotFound();
                }

                // Giữ lại thông tin người tạo và ngày tạo
                setting.CreatedBy = existingSetting.CreatedBy;
                setting.CreatedDate = existingSetting.CreatedDate;
                setting.UpdatedBy = currentUser?.UserName ?? "System";  // Cập nhật người chỉnh sửa
                setting.UpdatedDate = DateTime.Now;  // Cập nhật thời gian chỉnh sửa

                try
                {
                    _context.Update(setting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SettingExists(setting.SET_ID))
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
            ViewBag.CurrentUser = await GetCurrentUser();
            return View(setting);
        }

        // GET: Admin/Settings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.SET_ID == id);
            if (setting == null)
            {
                return NotFound();
            }

            ViewBag.CurrentUser = await GetCurrentUser();
            return View(setting);
        }

        // POST: Admin/Settings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var setting = await _context.Settings.FindAsync(id);
            if (setting != null)
            {
                _context.Settings.Remove(setting);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SettingExists(int id)
        {
            return _context.Settings.Any(e => e.SET_ID == id);
        }
    }
}
