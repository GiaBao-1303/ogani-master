using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.dto;
using ogani_master.enums;
using ogani_master.Models;

namespace ogani_master.Controllers
{
    public class ShopingCartController(OganiMaterContext _context) : Controller
    {

        private OganiMaterContext context = _context;
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
            var userId = HttpContext.Session.GetInt32("UserID");

            if(userId == null)
            {
                return RedirectToAction("SignInPage", "Auth");
            }

            User? user = await GetCurrentUser();

            if (user == null) return RedirectToAction("SignInPage", "Auth");

            List<Cart> carts = await this.context.Carts
              .Include(c => c.Product)
              .Where(c => c.UserId == user.UserId)
              .ToListAsync();
            List<Order> orders = await this.context.Orders
                .Include(o => o.Product)
                .Where(o => o.MEM_ID == user.UserId)
                .ToListAsync();

            ViewBag.CurrentUser = user;
            ViewBag.Carts = carts;
            ViewBag.Orders = orders;

            return View();
        }

        [HttpPost]
        [Route("/RemoveCart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCart(int cartId)
        {
            var user = await GetCurrentUser();

            if (user == null) return RedirectToAction("SignInPage", "Auth");

            Cart? cart = await this.context.Carts.FirstOrDefaultAsync(c => c.ORDD_ID == cartId);

            if (cart == null) return RedirectToAction("Index", "Home");

            this.context.Carts.Remove(cart);

            await this.context.SaveChangesAsync();

            return RedirectToAction("Index", "ShopingCart");
        }

        [HttpPost]
        [Route("/AddToCart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(AddToCartDto addToCartDto)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index", "Home");

            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("role");

            if (userId == null)
            {
                return RedirectToAction("SignInPage", "Auth");
            }

            User? user = await GetCurrentUser();

            if (user == null) return RedirectToAction("SignInPage", "Auth");

            Product? product = await context.Products.FirstOrDefaultAsync(p => p.PRO_ID == addToCartDto.ProdID);

            if (product == null) return NotFound();

            ViewBag.CurrentUser = user;



            Cart? existingProductInCart = await this.context.Carts.FirstOrDefaultAsync(c => c.Product.PRO_ID == addToCartDto.ProdID);

            if (existingProductInCart != null) {
                existingProductInCart.Quantity += addToCartDto.amount;
                await this.context.SaveChangesAsync();
                return RedirectToAction("Index", "ShopingCart");
            }

            Cart cart = new Cart
            {
                Price = product.Price,
                PRO_ID = product.PRO_ID,
                Quantity = addToCartDto.amount,
                UserId = user.UserId,
                CreatedBy = role,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                UpdatedBy = role,
            };

            this.context.Carts.Add(cart);
            await this.context.SaveChangesAsync();

            return RedirectToAction("Index", "ShopingCart");

        }

        [HttpPost]
        [Route("/Order")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Order(OrderDto[] orderDtos)
        {

            if(!ModelState.IsValid)
            {
                return RedirectToAction("Index", "ShopingCart");
            }

            var role = HttpContext.Session.GetString("role");

            User? user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("SignInPage", "Auth");
            }

            foreach (OrderDto o in orderDtos) {
                Cart? existingProductInCart = await this.context.Carts.Include(c => c.Product).FirstOrDefaultAsync(c => c.PRO_ID == o.ProdId);

                if (existingProductInCart == null) {
                    return RedirectToAction("Index", "Home");
                }

                Order order = new Order
                {
                    IsPaid = false,
                    PROD_ID = o.ProdId,
                    Phone = user.Phone,
                    OrderDate = DateTime.Now,
                    PaymentMethod = "COD",
                    MEM_ID = user.UserId,
                    Status = (int)OrderStatus.Pending,
                    Address = user.Address,
                    Quantity = o.amount,
                    CreatedBy = role,
                    CreatedDate = DateTime.Now,
                    TotalPrice = existingProductInCart.Product.Price * o.amount,
                    UpdatedDate = DateTime.Now,
                };

                this.context.Add(order);
                this.context.Remove(existingProductInCart);
            }

            await this.context.SaveChangesAsync();

            return RedirectToAction("Index", "ShopingCart");
        }
    }
}
