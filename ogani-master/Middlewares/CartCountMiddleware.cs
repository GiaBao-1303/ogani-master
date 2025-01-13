using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;
using System.Threading.Tasks;

namespace ogani_master.Middlewares
{
	public class CartCountMiddleware
	{
		private readonly RequestDelegate _next;

		public CartCountMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context, OganiMaterContext dbContext)
		{

			int? userId = context.Session.GetInt32("UserID");

			int cartCount = await dbContext.Carts.Where(c => c.UserId == userId).CountAsync();
			List<Cart> carts = await dbContext.Carts.Where(c => c.UserId == userId).ToListAsync();

			decimal total = carts.Sum(c => (c.DiscountPrice ?? c.Price) * c.Quantity);

			context.Items["CartCount"] = cartCount;
			context.Items["CartTotal"] = total ;

			await _next(context);
		}
	}
}
