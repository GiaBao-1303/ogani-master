using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ogani_master.Middlewares
{
    public class AdminAccessControlMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminAccessControlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Admin"))
            {
                var userRole = context.Session.GetString("role");



                if (userRole != "Admin")
                {
                    context.Response.Redirect("/");
                    return;
                }
            }

            await _next(context);
        }
    }
}
