
using AggregatorService.Services;
using AggregatorService.Cache;

namespace AggregatorService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Logging.AddConsole();

        builder.Services.AddMemoryCache();

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
        });
        builder.Services.AddScoped(typeof(TwoLevelCacheService<>));

        builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o =>
        {
            o.Address = new Uri("https://localhost:5003"); // OrdersService
        });

        builder.Services.AddGrpcClient<ReviewService.ReviewServiceClient>(o =>
        {
            o.Address = new Uri("https://localhost:5002"); // ReviewsService
        });

        builder.Services.AddScoped<IAggregationService, AggregationService>();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
