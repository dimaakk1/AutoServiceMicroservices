
using Infrastructure;
using Application;
using Application.Automapper;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Application.Grpc;
using WebApi.Middleware;
using Application.Cache;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddGrpc();

            builder.Services.AddMemoryCache();

            builder.AddServiceDefaults();
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis");
            });
            builder.Services.AddScoped(typeof(TwoLevelCacheService<>));
            // Add services to the container.
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });

            builder.Services.AddScoped<OrderGrpcClient>();

            builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o =>
            {
                o.Address = new Uri("https://localhost:5003"); // OrdersService
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGrpcService<ReviewServiceImpl>(); // або ReviewServiceImpl

            app.MapControllers();

            app.Run();
        }
    }
}
