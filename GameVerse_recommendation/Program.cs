using GameVerse_recommendation.Models;
using GameVerse_recommendation.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using RabbitMQ.Client;

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

            // Configure Database
            builder.Services.AddDbContext<VideogameStoreContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.UseNpgsql(builder.Configuration.GetConnectionString("postgresql"));
            });

            builder.Services.AddSingleton<RecommendationsCache>(provider => new RecommendationsCache
                (
                    new MemoryCacheOptions()
                    {
                        Clock = new SystemClock(),
                        ExpirationScanFrequency = TimeSpan.FromDays(1),
                        SizeLimit = 2048,
                        CompactionPercentage = .25
                    }
                ));

            // Configure Security
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

            builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = builder.Configuration.GetValue<string>("Credentials:Rabbit:Username"),
                Password = builder.Configuration.GetValue<string>("Credentials:Rabbit:Password"),
                DispatchConsumersAsync = true
            });
            builder.Services.AddHostedService<RecommendationConsumerService>();



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Configure middleware
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