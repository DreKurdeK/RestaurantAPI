using RestaurantAPI;
using RestaurantAPI.Entities;
using RestaurantAPI.Services;
using System.Reflection;
using NLog.Web;
using NLog;
using RestaurantAPI.Middleware;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Validators;
using FluentValidation.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

            var authenticationSettings = new AuthenticationSettings();


            // Add services to the container.
            b.Configuration.GetSection("Authentication").Bind(authenticationSettings);
            b.Services.AddSingleton(authenticationSettings);
            b.Services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = "Bearer";
                option.DefaultScheme = "Bearer";
                option.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = authenticationSettings.JwtIssuer,
                    ValidAudience = authenticationSettings.JwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey)),
                };
            });

            b.Services.AddControllers();
            b.Services.AddFluentValidationAutoValidation();
            b.Services.AddFluentValidationClientsideAdapters();
            b.Services.AddDbContext<RestaurantDbContext>();
            b.Services.AddScoped<RestaurantSeeder>();
            b.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
            b.Services.AddScoped<IRestaurantService, RestaurantService>();
            b.Services.AddScoped<IDishService, DishService>();
            b.Services.AddScoped<IAccountService, AccountService>();
            b.Services.AddScoped<ErrorHandlingMiddleware>();
            b.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            b.Services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();
            b.Services.AddScoped<RequestTimeMiddleware>();
            b.Services.AddSwaggerGen();

            var app = b.Build();

            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<RestaurantSeeder>();
                seeder.Seed();
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<RequestTimeMiddleware>();
            app.UseAuthentication();
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