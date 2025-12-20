using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Ticket_Booking.Data;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Models.CurrencyModels;
using Ticket_Booking.Repositories;
using Ticket_Booking.Resources;
using Ticket_Booking.Services;
using Ticket_Booking.Helpers;
using VNPAY.Extensions;

namespace Ticket_Booking
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddLocalization(options => 
            {
                options.ResourcesPath = "Resources";
            });
            builder.Services.AddControllersWithViews()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResource));
                });

            var vnpayConfig = builder.Configuration.GetSection("VNPAY");

            builder.Services.AddVnpayClient(config =>
            {
                config.TmnCode = vnpayConfig["TmnCode"]!;
                config.HashSecret = vnpayConfig["HashSecret"]!;
                config.CallbackUrl = vnpayConfig["CallbackUrl"]!;
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
                   options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


            builder.Services.AddScoped<IRepository<User>, UserRepository>();
            builder.Services.AddScoped<IRepository<Company>, CompanyRepository>();

            builder.Services.AddScoped<IRepository<Trip>, TripRepository>();
            builder.Services.AddScoped<IRepository<Ticket>, TicketRepository>();
            builder.Services.AddScoped<IRepository<Payment>, PaymentRepository>();
            builder.Services.AddScoped<IRepository<Review>, ReviewRepository>();
            builder.Services.AddScoped<Services.MailService>();

            // Register PNR Helper
            builder.Services.AddScoped<IPNRHelper, PNRHelper>();

            // Add memory cache for currency exchange rates
            builder.Services.AddMemoryCache();

            // Add HTTP context accessor for currency service
            builder.Services.AddHttpContextAccessor();

            // Configure currency options
            builder.Services.Configure<CurrencyOptions>(
                builder.Configuration.GetSection(CurrencyOptions.SectionName));

            // Register HttpClient for Exchange Rate API
            // AddHttpClient already registers the service, so no need for separate AddScoped
            builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
            {
                var currencyOptions = builder.Configuration.GetSection(CurrencyOptions.SectionName).Get<CurrencyOptions>();
                if (currencyOptions != null && !string.IsNullOrEmpty(currencyOptions.ApiEndpoint))
                {
                    client.BaseAddress = new Uri(currencyOptions.ApiEndpoint);
                }
                client.Timeout = TimeSpan.FromSeconds(5);
            });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Configure localization
            var supportedCultures = new[] { "vi", "en" };
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("vi");
                options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
                options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
                
                // Add cookie provider to read culture from cookie
                options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseSession();

            // Add localization middleware
            app.UseRequestLocalization();

            app.UseHttpsRedirection();
            app.UseStaticFiles();


            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}");

            AppDbContext appDbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            appDbContext.Database.EnsureCreated();
            appDbContext.Seed();

            app.Run();
        }
    }
}
