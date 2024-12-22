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
    public class SettingsController : Controller
    {
        private readonly OganiMaterContext _context;

        public SettingsController(OganiMaterContext context)
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

        // GET: Admin/Settings
        public async Task<IActionResult> Index()
        {
            ViewBag.CurrentUser = await GetCurrentUser();
            return View(await _context.Settings.ToListAsync());
        }

        // GET: Admin/Settings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var setting = await _context.Settings
                .FirstOrDefaultAsync(m => m.SET_ID == id);
            if (setting == null)
            {
                return NotFound();
            }

            return View(setting);
        }

        // GET: Admin/Settings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Settings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SET_ID,Name,Value,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy")] Setting setting)
        {
            if (ModelState.IsValid)
            {
                _context.Add(setting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(setting);
        }

        // GET: Admin/Settings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var setting = await _context.Settings.FindAsync(id);
            if (setting == null)
            {
                return NotFound();
            }
            return View(setting);
        }

        // POST: Admin/Settings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SET_ID,Name,Value,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy")] Setting setting)
        {
            if (id != setting.SET_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
            return View(setting);
        }

        // GET: Admin/Settings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var setting = await _context.Settings
                .FirstOrDefaultAsync(m => m.SET_ID == id);
            if (setting == null)
            {
                return NotFound();
            }

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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SettingExists(int id)
        {
            return _context.Settings.Any(e => e.SET_ID == id);
        }
    }
}
