using DemoApp.Controllers;
using DemoApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DemoApp
{
    public class Program
    {
        public static void Main(string[] args)
        {   

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContext") ?? throw new InvalidOperationException("Connection string 'AppDbContext' not found.")));


            // Thêm dòng này
            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = false;
            });
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            // =========  2 DÒNG NÀY ĐỂ DÙNG SESSION =========
            builder.Services.AddDistributedMemoryCache();   // bộ nhớ tạm cho session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // hết hạn sau 30 phút
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            });
           
            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/Account/Login"))
                {
                    foreach (var cookie in context.Request.Cookies.Keys)
                    {
                        if (cookie.Contains("Antiforgery"))
                            context.Response.Cookies.Delete(cookie);
                    }
                }

                await next();
            });
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            // Thêm route cụ thể cho Account
            app.MapControllerRoute(
                name: "account",
                pattern: "account/{action=Login}",
                defaults: new { controller = "Account" });
            app.MapControllerRoute(
                name: "admin",
                pattern: "admin/{action=Dashboard}",
                defaults: new { controller = "Admin" });

            app.Run();
        }
    }
}