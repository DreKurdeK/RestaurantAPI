using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using RestaurantAPI;
using RestaurantAPI.Authorization;
using RestaurantAPI.Entities;
using RestaurantAPI.Middleware;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Validators;
using RestaurantAPI.Services;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

internal class Program
{
    private static void Main(string[] args)
    {
        var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

        try
        {
            logger.Info("Initializing application...");

            var b = WebApplication.CreateBuilder(args);

            b.Logging.ClearProviders();
            b.Host.UseNLog();

            var authenticationSettings = new AuthenticationSettings();

            b.Configuration.GetSection("Authentication").Bind(authenticationSettings);
            if (string.IsNullOrEmpty(authenticationSettings.JwtKey))
            {
                throw new ArgumentNullException(nameof(authenticationSettings), "JWT Key cannot be null or empty.");
            }
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey))
                };
            });

            b.Services.AddAuthorizationBuilder()
                .AddPolicy("HasNationality", builder => builder.RequireClaim("Nationality", "German", "Polish"))
                .AddPolicy("AtLeast20", builder => builder.AddRequirements(new MinimumAgeRequirement(20)))
                .AddPolicy("CreatedAtLeast2Restaurants", builder => builder.Requirements.Add(new CreatedMultipleRestaurantsRequirement(2)));
            b.Services.AddControllers();
            b.Services.AddFluentValidationAutoValidation();
            b.Services.AddFluentValidationClientsideAdapters();
            b.Services.AddScoped<RestaurantSeeder>();
            b.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
            b.Services.AddScoped<IRestaurantService, RestaurantService>();
            b.Services.AddScoped<IDishService, DishService>();
            b.Services.AddScoped<IAuthorizationHandler, MinimumAgeRequirementHandler>();
            b.Services.AddScoped<IAuthorizationHandler, ResourceOperationRequirementHandler>();
            b.Services.AddScoped<IAuthorizationHandler, CreatedMultipleRestaurantsRequirementHandler>();
            b.Services.AddScoped<IAccountService, AccountService>();
            b.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            b.Services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();
            b.Services.AddScoped<IValidator<RestaurantQuery>, RestaurantQueryValidator>();
            b.Services.AddScoped<IUserContextService, UserContextService>();
            b.Services.AddScoped<RequestTimeMiddleware>();
            b.Services.AddScoped<ErrorHandlingMiddleware>();
            b.Services.AddHttpContextAccessor();
            b.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Restaurant API", Version = "v1" });
            });

            var allowedOrigins = b.Configuration["AllowedOrigins"];
            if (string.IsNullOrEmpty(allowedOrigins))
            {
                throw new ArgumentNullException(nameof(allowedOrigins), "AllowedOrigins cannot be null or empty.");
            }
            b.Services.AddCors(options =>
            {
                options.AddPolicy("FrontEndClient", builder =>
                    builder.AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins(allowedOrigins)
                );
            });

            

            b.Services.AddDbContext<RestaurantDbContext>(options =>
            {
                var connection = b.Environment.IsProduction()
                ? Environment.GetEnvironmentVariable($"AZURE_SQL_CONNECTIONSTRING")
                : b.Configuration.GetConnectionString("RestaurantDbConnection");
                options.UseSqlServer();
            });

            var app = b.Build();

            app.UseResponseCaching();
            app.UseStaticFiles();
            app.UseCors("FrontEndClient");
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<RequestTimeMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<RestaurantSeeder>();
                seeder.Seed();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API v1");
            });

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Application stopped because of exception.");
            throw;
        }
        finally
        {
            NLog.LogManager.Shutdown();
        }
    }
}
