using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.configs;
using ogani_master.dto;
using ogani_master.enums;
using ogani_master.Models;
using ogani_master.Payments.dto;
using ogani_master.Payments.momo;
using ogani_master.utils;
using System.Globalization;

namespace ogani_master.Controllers
{
    public class ShopingCartController : Controller
    {
        private readonly ILogger<ShopingCartController> logger;
        private OganiMaterContext context;

        public ShopingCartController(OganiMaterContext _context)
        {
            this.context = _context;
            var logger = GlobalLogger.CreateLogger<ShopingCartController>();
            this.logger = logger;
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


            ViewBag.Settings = context.Settings.ToList();
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

            if(!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = string.Join("<br>", ModelState.Values
               .SelectMany(v => v.Errors)
               .Select(e => e.ErrorMessage));

                return RedirectToAction("Index", "Product", new { uid = addToCartDto.ProdID });
            }

            User? user = await GetCurrentUser();

            if (user == null) return RedirectToAction("SignInPage", "Auth");

            Product? product = await context.Products.FirstOrDefaultAsync(p => p.PRO_ID == addToCartDto.ProdID);

            if (product == null) return NotFound();

            ViewBag.CurrentUser = user;

            if(addToCartDto.amount > product.quantity)
            {
                TempData["ErrorMessage"] = $"Not enough stock for product {product.Name}. Available: {product.quantity}.";
                return RedirectToAction("Index", "Product", new { uid = addToCartDto.ProdID });
            }

            Cart? existingProductInCart = await this.context.Carts.FirstOrDefaultAsync(c => c.PRO_ID == addToCartDto.ProdID && c.UserId == user.UserId);

            if (existingProductInCart != null)
            {
                existingProductInCart.Quantity += addToCartDto.amount;
                await this.context.SaveChangesAsync();
                return RedirectToAction("Index", "ShopingCart");
            }

            Cart cart = new Cart
            {
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                PRO_ID = product.PRO_ID,
                Quantity = addToCartDto.amount,
                UserId = user.UserId,
                CreatedBy = Enum.GetName(typeof(UserRole), user.Role),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                UpdatedBy = Enum.GetName(typeof(UserRole), user.Role),
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

            if (orderDtos == null || !orderDtos.Any())
            {
                TempData["ErrorMessage"] = "No orders provided.";
                return RedirectToAction("Index", "ShopingCart");
            }

            var user = await GetCurrentUser();
            var role = HttpContext.Session.GetString("role");
            if (user == null)
            {
                return RedirectToAction("SignInPage", "Auth");
            }

            var cartItems = await this.context.Carts
                .Where(c => c.UserId == user.UserId)
                .ToListAsync();

            var productIds = orderDtos.Select(o => o.ProdId).ToList();
            var products = await this.context.Products
                .Where(p => productIds.Contains(p.PRO_ID))
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Cart empty";
                return RedirectToAction("Index", "ShopingCart");
            }

            foreach (var orderDto in orderDtos)
            {
                if (orderDto.amount == 0)
                {
                    TempData["ErrorMessage"] = "One or more products in your cart have an invalid quantity (0). Please update your cart and try again.";
                    return RedirectToAction("Index", "ShopingCart");
                }

                var product = products.FirstOrDefault(p => p.PRO_ID == orderDto.ProdId);
                if (product == null)
                {

                    TempData["ErrorMessage"] = $"Product with ID {orderDto.ProdId} does not exist.";
                    return RedirectToAction("Index", "ShopingCart");
                }


                if (orderDto.amount > product.quantity)
                {
                    TempData["ErrorMessage"] = $"Not enough stock for product {product.Name}. Available: {product.quantity}.";
                    return RedirectToAction("Index", "ShopingCart");
                }

                var cartItem = cartItems.FirstOrDefault(c => c.PRO_ID == orderDto.ProdId);
                if (cartItem != null)
                {
                    cartItem.Quantity = orderDto.amount;
                }

                product.quantity = product.quantity - orderDto.amount;
            }

            await this.context.SaveChangesAsync();

            List<MessageMailDto> messageMailDtos = new List<MessageMailDto>();

            foreach (OrderDto o in orderDtos)
            {
                Cart? existingProductInCart = await this.context.Carts.Include(c => c.Product).FirstOrDefaultAsync(c => c.PRO_ID == o.ProdId);

                if (existingProductInCart == null)
                {
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
                    TotalPrice = (existingProductInCart.Product?.DiscountPrice ?? existingProductInCart.Product.Price) * o.amount,
                    UpdatedDate = DateTime.Now,
                };

                decimal originPrice = existingProductInCart.Product?.DiscountPrice ?? existingProductInCart.Product.Price;

                MessageMailDto dataMail = new MessageMailDto
                {
                    companyName = "Ogani-master",
                    customerName = user.LastName + " " + user.FirstName,
                    Email = user.Email,
                    price = (int)originPrice,
                    prodName = existingProductInCart.Product.Name,
                    quantity = o.amount,
                    totalPrice = o.amount * (int)existingProductInCart.Product.Price,
                };

                messageMailDtos.Add(dataMail);
                this.context.Add(order);
                this.context.Remove(existingProductInCart);
            }

            await this.context.SaveChangesAsync();



            bool isSendMain = await MailUtils.SendMailGoogleSmtpAsync(user.Email, "Ogani-master", "", messageMailDtos);

            if (!isSendMain)
            {
                throw new Exception("Send mail failed");
            }

            return RedirectToAction("Index", "ShopingCart");
        }

        [HttpPost]
        [Route("/order-pay-via-momo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderWithMomo(OrderDto[] orderDtos)
        {
            if (orderDtos == null || !orderDtos.Any())
            {
                TempData["ErrorMessage"] = "No orders provided.";
                return RedirectToAction("Index", "ShopingCart");
            }

            var user = await GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("SignInPage", "Auth");
            }

            var cartItems = await this.context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == user.UserId)
                .ToListAsync();

            var productIds = orderDtos.Select(o => o.ProdId).ToList();
            var products = await this.context.Products
                .Where(p => productIds.Contains(p.PRO_ID))
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Cart empty";
                return RedirectToAction("Index", "ShopingCart");
            }

            foreach (var orderDto in orderDtos)
            {
                if(orderDto.amount == 0)
                {
                    TempData["ErrorMessage"] = "One or more products in your cart have an invalid quantity (0). Please update your cart and try again.";
                    return RedirectToAction("Index", "ShopingCart");
                }

                var product = products.FirstOrDefault(p => p.PRO_ID == orderDto.ProdId);
                if (product == null)
                {
                    
                    TempData["ErrorMessage"] = $"Product with ID {orderDto.ProdId} does not exist.";
                    return RedirectToAction("Index", "ShopingCart");
                }


                if (orderDto.amount > product.quantity)
                {
                    TempData["ErrorMessage"] = $"Not enough stock for product {product.Name}. Available: {product.quantity}.";
                    return RedirectToAction("Index", "ShopingCart");
                }

                var cartItem = cartItems.FirstOrDefault(c => c.PRO_ID == orderDto.ProdId);
                if (cartItem != null)
                {
                    cartItem.Quantity = orderDto.amount;
                }

				product.quantity = product.quantity - orderDto.amount;
			}

            await this.context.SaveChangesAsync();

            var momoService = new MomoService(this.context);

            var request = HttpContext.Request;

            string domain = $"{request.Scheme}://{request.Host}";

            var momoResponse = await momoService.CreatePayment(user.UserId, domain);

            if (momoResponse == null || momoResponse.resultCode != 0)
            {
                return RedirectToAction("Index", "ShopingCart", new { error = "Payment failed" });
            }

            return Redirect(momoResponse.payUrl);
        }

        [Route("/momo-redirect")]
        public async Task<IActionResult> MomoRedirect()
        {
            var queryString = Request.Query;
            string resultCode = queryString["resultCode"];
            string orderId = queryString["orderId"];
            string partnerCode = queryString["partnerCode"];
            string requestId = queryString["requestId"];

            User? user = await this.GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("SignInPage", "Auth");
            }

            var momoService = new MomoService(this.context);

            MomoConfirmDto momoConfirmDto = new MomoConfirmDto
            {
                lang = "vi",
                orderId = orderId,
                partnerCode = partnerCode,
                requestId = requestId,
            };

            if (resultCode == "0")
            {
                ConfirmPaymentResultDto paymentConfirmed = await momoService.ConfirmPaymentFromMomo(user.UserId, momoConfirmDto, user);

                if (paymentConfirmed.IsSuccess)
                {
                   
                    string subject = "Bạn đã thanh toán thành công với đơn hàng tại - Ogani-master";
                    string customerName = $"{user.FirstName} {user.LastName}";
                    string companyName = "Ogani-master";
                    DateTime nowUtc = DateTime.UtcNow;
                    TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, vietnamTimeZone);
                    string amount = paymentConfirmed.Amount.ToString("C0", new CultureInfo("en-US"));

                    await MailUtils.SendMailGoogleSmtpPaymentSuccessAsync(user.Email, subject, customerName, companyName, momoConfirmDto.orderId, "MoMo", amount, vietnamTime);

                    return Redirect("/ShopingCart");
                }
                else
                {
                    TempData["PaymentConfirmMessage"] = "Payment confirmation failed.";
                    return RedirectToAction("Index", "ShopingCart");
                }
            }
            else
            {
                TempData["PaymentConfirmMessage"] = "Payment confirmation failed.";
                return RedirectToAction("Index", "ShopingCart");
            }
        }

    }
}
