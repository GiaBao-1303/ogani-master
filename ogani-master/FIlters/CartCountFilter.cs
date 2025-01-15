using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;
using System.Threading.Tasks;

public class CartCountFilter : ActionFilterAttribute
{
    private readonly OganiMaterContext _dbContext;

    public CartCountFilter(OganiMaterContext dbContext)
    {
        _dbContext = dbContext;
    }


    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{

		HttpContext httpContext = context.HttpContext;

        int? userId = httpContext.Session.GetInt32("UserID");

        int cartCount = await _dbContext.Carts.Where(c => c.UserId == userId).CountAsync();
        decimal cartTotal = await _dbContext.Carts
           .Where(c => c.UserId == userId)
           .SumAsync(c => (c.DiscountPrice ?? c.Price) * c.Quantity);

        List<FavoritesModel> favorites = await _dbContext.Favorites
            .Where(f => f.UserID == userId)
            .ToListAsync();

        if (favorites == null) throw new Exception("Favorites is null");

        User? currentUser = await this._dbContext.users.FirstOrDefaultAsync(u => u.UserId == userId);

        if (context.Controller is Controller controller)
        {
            controller.ViewBag.CartCount = cartCount;
            controller.ViewBag.CartTotal = cartTotal;

        }

        await next();
    }
}
