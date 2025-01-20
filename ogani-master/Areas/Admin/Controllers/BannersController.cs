using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ogani_master.Areas.Admin.DTO;
using ogani_master.Models;
using System.IO;

namespace ogani_master.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BannersController : Controller
    {
        private readonly IWebHostEnvironment _hostEnv;

        private readonly OganiMaterContext _context;

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

        public BannersController(OganiMaterContext context, IWebHostEnvironment hostEnv)
        {
            _context = context;
            _hostEnv = hostEnv;

        }

        // GET: Admin/Banners
        public async Task<IActionResult> Index()
        {
            ViewBag.CurrentUser = await this.GetCurrentUser();

            return View(await _context.Banners.ToListAsync());
        }

        // GET: Admin/Banners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banners
                .FirstOrDefaultAsync(m => m.BAN_ID == id);
            if (banner == null)
            {
                return NotFound();
            }
            ViewBag.CurrentUser = await this.GetCurrentUser();
            return View(banner);
        }

        // GET: Admin/Banners/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.CurrentUser = await this.GetCurrentUser();
            return View();
        }

        // POST: Admin/Banners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] BannerDTO _banner)
        {
            var currentUser = await GetCurrentUser();
            var banner = new Banner
            {
                BAN_ID = _banner.BAN_ID,
                Title = _banner.Title,
                DisplayOrder = _banner.DisplayOrder,
                Url = _banner.Url,
                CreatedBy = currentUser?.UserName ?? "System", // gán createdBy
                
            };
            if (ModelState.IsValid)
            {
                string? newImageFileName = null;
                if (_banner.Image != null)
                {
                    var extension = Path.GetExtension(_banner.Image.FileName);
                    newImageFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(_hostEnv.WebRootPath, "data", "banner", newImageFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await _banner.Image.CopyToAsync(stream);
                    }

                    banner.Image = newImageFileName;
                }
                _context.Add(banner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(banner);
        }
        

        // GET: Admin/Banners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
               return NotFound();
            }

            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
               return NotFound();
            }

            ViewBag.CurrentUser = await GetCurrentUser(); ;
            return View(banner);
        }

        // POST: Admin/Banners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] BannerDTO bannerDto)
        {
            if (id != bannerDto.BAN_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBanner = await _context.Banners.FindAsync(id);
                    if (existingBanner == null)
                    {
                        return NotFound();
                    }

                    existingBanner.Title = bannerDto.Title;
                    existingBanner.Url = bannerDto.Url;
                    existingBanner.DisplayOrder = bannerDto.DisplayOrder;

                    if (bannerDto.Image != null && bannerDto.Image is { Length: > 0 })
                    {
                        var extension = Path.GetExtension(bannerDto.Image.FileName);
                        var newImageFileName = $"{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(_hostEnv.WebRootPath, "data", "banner", newImageFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await bannerDto.Image.CopyToAsync(stream);
                        }

                        if (!string.IsNullOrEmpty(existingBanner.Image))
                        {
                            var oldFilePath = Path.Combine(_hostEnv.WebRootPath, "data", "banner", existingBanner.Image);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        existingBanner.Image = newImageFileName;
                    }

                    var currentUser = await GetCurrentUser();
                    existingBanner.UpdatedBy = currentUser?.UserName ?? "System";
                    existingBanner.UpdatedDate = DateTime.Now;

                    _context.Update(existingBanner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BannerExists(bannerDto.BAN_ID))
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
            ViewBag.CurrentUser = await this.GetCurrentUser();
            return View(bannerDto);
        }

        // GET: Admin/Banners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banners
                .FirstOrDefaultAsync(m => m.BAN_ID == id);
            if (banner == null)
            {
                return NotFound();
            }
            ViewBag.CurrentUser = await this.GetCurrentUser();

            return View(banner);
        }

        // POST: Admin/Banners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                _context.Banners.Remove(banner);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BannerExists(int id)
        {

            return _context.Banners.Any(e => e.BAN_ID == id);
        }
    }
}
