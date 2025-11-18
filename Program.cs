using Microsoft.EntityFrameworkCore;
using Ticket_Booking.Data;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Repositories;

namespace Ticket_Booking
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options =>
                   options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


            builder.Services.AddScoped<IRepository<User>, UserRepository>();
            builder.Services.AddScoped<IRepository<Company>, CompanyRepository>();
            builder.Services.AddScoped<IRepository<TransportType>, TransportTypeRepository>();
            builder.Services.AddScoped<IRepository<Models.DomainModels.Route>, RouteRepository>();
            builder.Services.AddScoped<IRepository<Vehicle>, VehicleRepository>();
            builder.Services.AddScoped<IRepository<Trip>, TripRepository>();
            builder.Services.AddScoped<IRepository<Ticket>, TicketRepository>();
            builder.Services.AddScoped<IRepository<Payment>, PaymentRepository>();
            builder.Services.AddScoped<IRepository<Review>, ReviewRepository>();



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(100);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            app.UseSession();

            app.UseHttpsRedirection();
            app.UseStaticFiles();


            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}");

            AppDbContext appDbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            appDbContext.Database.EnsureCreated();

            app.Run();
        }
    }
}
