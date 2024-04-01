using GameVerse_recommendation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GameVerse_recommendation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages(options =>
            {

            });

            builder.Services.AddControllers(options =>
            {

            });

            builder.Services.AddDbContext<VideogameStoreContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.UseNpgsql(builder.Configuration.GetConnectionString("postgresql"));
            });

            builder.Services.AddIdentityCore<Player>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters += " ";
                })
                .AddSignInManager()
                .AddEntityFrameworkStores<VideogameStoreContext>();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            }).AddIdentityCookies(options =>
            {
                // Cookie settings
                options.ApplicationCookie?.Configure(cookie =>
                {
                    cookie.Cookie.HttpOnly = true;
                    cookie.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                    cookie.LoginPath = "/Register";
                    cookie.SlidingExpiration = true;
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseStaticFiles();

            app.MapRazorPages();
            app.MapControllers();

            app.Run();
        }
    }
}