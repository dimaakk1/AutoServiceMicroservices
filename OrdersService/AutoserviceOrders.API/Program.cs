using AutoserviceOrders.DAL.UnitOfWork;
using AutoserviceOrders.DAL.Repositories;
using AutoserviceOrders.DAL.Repositories.Interfaces;
using AutoserviceOrders.BLL.Services.Interfaces;
using AutoserviceOrders.BLL.Services;
using AutoserviceOrders.BLL.Automapper;
using AutoMapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using AutoserviceOrders.DAL.db;
using AutoserviceOrders.BLL.Grpc;


namespace AutoserviceOrders.API
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddGrpc();
            builder.AddServiceDefaults();



            var sqlConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer");
            DatabaseInitializer.Initialize(sqlConnectionString);

            builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(sqlConnectionString));

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IProductRepository, ProductRepository>();

            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderService, BLL.Services.OrderService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IOrderDetailsService, OrderDetailsService>();
            builder.Services.AddScoped<IOrderItemService, OrderItemService>();


            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddAutoMapper(typeof(MappingProfile));
            // Add services to the container.

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
            app.MapGrpcService<OrderServiceImpl>(); 


            app.MapControllers();

            app.Run();

        }
    }
}


