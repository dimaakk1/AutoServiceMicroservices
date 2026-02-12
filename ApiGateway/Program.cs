
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiGateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("JwtPolicy",
                policy => policy.RequireAuthenticatedUser());
        });

        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
            .ConfigureHttpClient((context, httpClient) =>
            {
                httpClient.ConnectTimeout = TimeSpan.FromSeconds(10);
            });

        builder.Services.AddHttpContextAccessor();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapReverseProxy();

        app.Run();
    }
}
