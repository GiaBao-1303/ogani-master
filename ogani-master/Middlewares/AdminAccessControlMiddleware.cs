using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ogani_master.Middlewares
{
    public class AdminAccessControlMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly Dictionary<string, string[]> _routeRoles = new Dictionary<string, string[]>
        {
            { "/Admin/Products", new[] { "Admin" } }, 
            { "/Admin/Orders", new[] { "Admin", "Moderator" } },
            { "/Admin/Users", new[] { "Admin" } },
            { "/Admin", new[] { "Admin", "Moderator" } },
            { "/Admin/Categories", new[] { "Admin", "Moderator" } },
            { "/Admin/BlogAd", new[] { "Admin", "Moderator" } },
            { "/Admin/Banners", new[] { "Admin", "Moderator" } },
            { "/Admin/Reviews", new[] { "Admin", "Moderator" } },
            { "/Admin/Menus", new[] { "Admin", "Moderator" } },
            { "/Admin/Settings", new[] { "Admin", "Moderator" } },
            { "/Admin/ManagementUsers", new[] { "Admin", "Moderator" } },

        };

        public AdminAccessControlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path.ToString().ToLower();

            var routeEntry = _routeRoles.FirstOrDefault(r => path.StartsWith(r.Key.ToLower()));

            if (routeEntry.Key != null)
            {
                var userRole = context.Session.GetString("role");

                if (string.IsNullOrEmpty(userRole) || !routeEntry.Value.Contains(userRole))
                {
                    context.Response.Redirect("/AccessDenied");
                    return;
                }
            }

            await _next(context);
        }
    }
}
