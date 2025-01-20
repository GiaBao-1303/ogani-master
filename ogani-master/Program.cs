using Microsoft.EntityFrameworkCore;
using ogani_master.Middlewares;
using ogani_master.Models;
using DotNetEnv;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using ogani_master.configs;
using ogani_master.FIlters;

namespace ogani_master
{
    public class Program
    {
        public static void Main(string[] args)
        {
			Env.Load();
			var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddUserSecrets<Program>();
			// Setting OganiMaterContext use the SQL Server
			builder.Services.AddDbContext<OganiMaterContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
					sqlOptions => sqlOptions.EnableRetryOnFailure()));



			builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;                
                options.Cookie.IsEssential = true;             
            });

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<CartCountFilter>();
                options.Filters.Add<Authenticate>();
            });

            var loggerFactory = LoggerFactory.Create(
                 builder => builder
                     .AddConsole()
                     .AddDebug()
                     .SetMinimumLevel(LogLevel.Debug));

            GlobalLogger.LoggerFactory = loggerFactory;

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            app.UseSession();

            //app.UseMiddleware<UserBehaviorLoggingMiddleware>();
            app.UseMiddleware<AdminAccessControlMiddleware>();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

			app.MapControllers();

			app.UseStaticFiles();

			app.MapControllerRoute(
               name: "areas",
               pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
