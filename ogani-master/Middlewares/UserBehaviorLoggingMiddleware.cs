using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ogani_master.Models;
using Microsoft.AspNetCore.Authentication;

namespace ogani_master.Middlewares
{
    public class UserBehaviorLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public UserBehaviorLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, OganiMaterContext dbContext)
        {


            try
            {
                var userId = context.Session.GetInt32("UserID");
                string role = context.Session.GetString("role") ?? "User";

                if (userId.HasValue && !role.Equals("Admin"))
                {

                    var action = context.Request.Method;
                    var page = context.Request.Path;

                    var log = new UserBehaviorLog
                    {
                        UserId = userId.Value,
                        Action = action,
                        Timestamp = DateTime.UtcNow,
                        Page = page
                    };

                    dbContext.UserBehaviorLogs.Add(log);
                    await dbContext.SaveChangesAsync();
                }

                await _next(context);
            }
            catch (Exception ex) {
                var returnUrl = context.Request.Headers["Referer"].ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    returnUrl = "/"; 
                }
                context.Response.Redirect(returnUrl);
            }
        }
    }

}
