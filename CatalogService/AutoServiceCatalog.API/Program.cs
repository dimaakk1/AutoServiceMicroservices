
using AutoServiceCatalog.DAL.Db;
using AutoServiceCatalog.DAL.Repositories.Intarfaces;
using AutoServiceCatalog.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using AutoServiceCatalog.BLL.Automapper;
using Microsoft.Extensions.DependencyInjection;
using AutoServiceCatalog.BLL.Services.Interfaces;
using AutoServiceCatalog.BLL.Services;
using AutoServiceCatalog.DAL.UOW;
using FluentValidation.AspNetCore;
using FluentValidation;
using AutoServiceCatalog.BLL.DTO;
using AutoServiceCatalog.BLL.Validation;
using AutoServiceCatalog.API.Middleware;
using AutoServiceCatalog.BLL.Cache;
namespace AutoServiceCatalog.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.AddConsole();

            builder.Services.AddMemoryCache();
            
            builder.AddServiceDefaults();
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis");
            });
            builder.Services.AddScoped(typeof(TwoLevelCacheService<>));
            

            // Add services to the container.
            var sqlConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer");

            builder.Services.AddDbContext<CarServiceContext>(options =>
                options.UseSqlServer(sqlConnectionString));

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IPartRepository, PartRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
            builder.Services.AddScoped<IPartDetailRepository, PartDetailRepository>();
            builder.Services.AddScoped<IPartSupplierRepository, PartSupplierRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IPartService, PartService>();
            builder.Services.AddScoped<ISupplierService, SupplierService>();
            builder.Services.AddScoped<IPartDetailService, PartDetailService>();
            builder.Services.AddScoped<IPartSupplierService, PartSupplierService>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddScoped<IValidator<CategoryDto>, CategoryCreateDtoValidator>();
            builder.Services.AddScoped<IValidator<PartCreateDto>, PartCreateDtoValidator>();
            builder.Services.AddScoped<IValidator<PartDetailCreateDto>, PartDetailCreateDtoValidator>();
            builder.Services.AddScoped<IValidator<PartSupplierDto>, PartSupplierCreateDtoValidator>();
            builder.Services.AddScoped<IValidator<SupplierCreateDto>, SupplierCreateDtoValidator>();


            builder.Services.AddAutoMapper(typeof(MappingProfile));


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseMiddleware<GlobalExceptionMiddleware>();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CarServiceContext>();
                context.Database.Migrate();
                Seeding.SeedAsync(context).GetAwaiter().GetResult();
            }

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
}
