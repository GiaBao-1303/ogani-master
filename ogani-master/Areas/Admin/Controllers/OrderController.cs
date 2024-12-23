using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.dto;
using ogani_master.enums;
using ogani_master.Models;
using System.ComponentModel.DataAnnotations;

namespace ogani_master.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController(OganiMaterContext _context) : Controller
    {
        private OganiMaterContext context = _context;
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

        public async Task<IActionResult> Index()
        {
            ViewBag.CurrentUser = await GetCurrentUser();

            List<Order> orders = await this.context.Orders
                .Include(o => o.Product)
                .ToListAsync();

            ViewBag.Orders = orders;

            return View();
        }

        public async Task<IActionResult> DetailsPage(int? uid)
        {
            ViewBag.CurrentUser = await GetCurrentUser();

            Order? order = await this.context.Orders
                .Include(o => o.User)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.ORD_ID == uid);

            if (order == null) return NotFound();

            var displayNames = typeof(OrderStatus)
                   .GetFields()
                   .Where(f => f.IsStatic && f.Name != nameof(OrderStatus.Pending))
                   .Select(f => f.GetCustomAttributes(typeof(DisplayAttribute), false)
                   .Cast<DisplayAttribute>()
                   .FirstOrDefault()?.Name ?? f.Name)
                   .ToList();


            ViewBag.Order = order;
            ViewBag.DisplayNames = displayNames;

            return View("~/Areas/Admin/Views/Order/Details.cshtml");
        }

        public async Task<IActionResult> DeletePage(int? uid)
        {

            ViewBag.CurrentUser = await GetCurrentUser();

            Order? order = await this.context.Orders.FirstOrDefaultAsync(o => o.ORD_ID == uid);

            if (order != null) return NotFound();

            ViewBag.Order = order;

            return View("~/Areas/Admin/Views/Order/Delete.cshtml");
        }

        [HttpPost]
        [Route("/VerifyOrder")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> VerifyOrderStatus(VerifyOrderStatusDto verifyOrderStatusDto)
        {

            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            Order? existingOrder = await this.context.Orders.FirstOrDefaultAsync(o => o.ORD_ID == verifyOrderStatusDto.OrderId);

            if(existingOrder == null)
            {
                return NotFound();
            }

            if (!Enum.IsDefined(typeof(OrderStatus), verifyOrderStatusDto.typeStatus))
            {
                return BadRequest();
            }



            existingOrder.Status = verifyOrderStatusDto.typeStatus;
            existingOrder.UpdatedDate = DateTime.UtcNow;
            existingOrder.UpdatedBy = "Admin";

            await this.context.SaveChangesAsync();


            return RedirectToAction("Index", "Order");
        }
    }
}
