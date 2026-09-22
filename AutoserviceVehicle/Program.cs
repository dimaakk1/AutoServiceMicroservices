using System.Security.Claims;
using System.Text;
using AutoserviceVehicle.API;
using AutoserviceVehicle.BLL.Services;
using AutoserviceVehicle.DAL.DB;
using AutoserviceVehicle.DAL.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AutoserviceVehicle;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        DotNetEnv.Env.NoClobber().Load(Path.Combine(builder.Environment.ContentRootPath, ".env"));
        builder.Configuration.AddEnvironmentVariables();
        builder.AddServiceDefaults();
        var connectionString = builder.Configuration.GetConnectionString("VehiclesDb")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:VehiclesDb.");
        var jwtKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
            throw new InvalidOperationException("Configure Jwt:Key with the same signing key as UsersService (at least 32 bytes).");

        builder.Services.AddDbContext<VehicleDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
        builder.Services.AddScoped<IVehicleService, VehicleService>();
        builder.Services.AddHttpClient<IVinDecoderService, NhtsaVinDecoderService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Nhtsa:BaseUrl"] ?? "https://vpic.nhtsa.dot.gov/api/");
            client.Timeout = TimeSpan.FromSeconds(20);
        });
        builder.Services.AddExceptionHandler<VehicleExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddControllers();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidateAudience = true,
                ValidateLifetime = true, ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });
        builder.Services.AddAuthorization(options => options.AddPolicy("VehicleOwner", policy =>
            policy.RequireAuthenticatedUser().RequireAssertion(context =>
                !string.IsNullOrWhiteSpace(context.User.FindFirstValue(ClaimTypes.NameIdentifier)) &&
                context.User.FindFirstValue(ClaimTypes.NameIdentifier)!.Length <= 450)));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Vehicle Service", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT",
                Description = "Access token from UsersService."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference
                    { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>()
            });
        });

        var app = builder.Build();
        // Development/AppHost/Compose initialize their own database. Production uses deployment migrations.
        if (app.Configuration.GetValue("Database:ApplyMigrations", app.Environment.IsDevelopment()))
        {
            await using var scope = app.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<VehicleDbContext>().Database.MigrateAsync();
        }
        app.UseExceptionHandler();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapDefaultEndpoints();
        app.MapControllers();
        await app.RunAsync();
    }
}
