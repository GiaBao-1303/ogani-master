using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ogani_master.Middlewares
{
    public class AdminAccessControlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _allowedRoles = { "Admin", "Moderator" };

        public AdminAccessControlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Admin"))
            {
                var userRole = context.Session.GetString("role");

                if (!_allowedRoles.Contains(userRole))
                {
                    context.Response.Redirect("/");
                    return;
                }
            }

            await _next(context);
        }
    }
}
