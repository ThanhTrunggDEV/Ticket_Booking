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
            builder.Services.AddTransient<IRepository<User>, UserRepository>();
            builder.Services.AddTransient<IRepository<Company>, CompanyRepository>();
            builder.Services.AddTransient<IRepository<TransportType>, TransportTypeRepository>();
            builder.Services.AddTransient<IRepository<Models.DomainModels.Route>, RouteRepository>();
            builder.Services.AddTransient<IRepository<Vehicle>, VehicleRepository>();
            builder.Services.AddTransient<IRepository<Trip>, TripRepository>();
            builder.Services.AddTransient<IRepository<Ticket>, TicketRepository>();
            builder.Services.AddTransient<IRepository<Payment>, PaymentRepository>();
            builder.Services.AddTransient<IRepository<Review>, ReviewRepository>();



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
