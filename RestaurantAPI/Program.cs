using RestaurantAPI;
using RestaurantAPI.Entities;
using RestaurantAPI.Services;
using System.Reflection;
using NLog.Web;
using NLog;
using RestaurantAPI.Middleware;

internal class Program
{
    private static void Main(string[] args)
    {
        var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

        try
        {
            logger.Info("Initializing application...");

            var b = WebApplication.CreateBuilder(args);

            // Integracja NLog z ASP.NET Core
            b.Logging.ClearProviders();
            b.Host.UseNLog();

            // Add services to the container.
            b.Services.AddControllers();
            b.Services.AddDbContext<RestaurantDbContext>();
            b.Services.AddScoped<RestaurantSeeder>();
            b.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
            b.Services.AddScoped<IRestaurantService, RestaurantService>();
            b.Services.AddScoped<IDishService, DishService>();
            b.Services.AddScoped<ErrorHandlingMiddleware>();
            b.Services.AddScoped<RequestTimeMiddleware>();
            b.Services.AddSwaggerGen();

            var app = b.Build();

            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<RequestTimeMiddleware>();

            // Resolve the RestaurantSeeder service and call the Seed method.
            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<RestaurantSeeder>();
                seeder.Seed();
            }

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API");
            });

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            // Zapis b³êdu przy uruchamianiu aplikacji
            logger.Error(ex, "Application stopped because of exception.");
            throw;
        }
        finally
        {
            // Gwarantowane wy³¹czenie logowania NLog
            NLog.LogManager.Shutdown();
        }
    }
}