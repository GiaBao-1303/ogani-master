
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;
using System.Threading.Tasks;

namespace ogani_master.FIlters
{
    public class Authenticate : ActionFilterAttribute
    {
        private readonly OganiMaterContext _dbContext;

        public Authenticate(OganiMaterContext dbContext)
        {
            _dbContext = dbContext;
        }


        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            HttpContext httpContext = context.HttpContext;

            int? userId = httpContext.Session.GetInt32("UserID");

            List<FavoritesModel> favorites = await this._dbContext.Favorites.Include(f => f.Product).Where(f => f.UserID == userId).ToListAsync();

            User? currentUser = await this._dbContext.users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (context.Controller is Controller controller)
            {
                controller.ViewBag.CurrentUser = currentUser;
                controller.ViewBag.Favorites = favorites;
            }

            await next();
        }
    }
}
