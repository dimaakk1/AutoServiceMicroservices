
namespace AutoserviceNotification;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        DotNetEnv.Env.Load();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddGrpcClient<UserService.UserServiceClient>(o =>
        {
            o.Address = new Uri(builder.Configuration["Services:UsersServiceUrl"] ?? "https://localhost:5004");
        });

        builder.Services.AddHostedService<OrderCreatedConsumer>();

        builder.Services.AddScoped<IEmailService, EmailService>();
        var app = builder.Build();

        app.MapDefaultEndpoints();

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
