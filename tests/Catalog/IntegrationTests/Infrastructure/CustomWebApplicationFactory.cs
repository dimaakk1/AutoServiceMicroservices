using AutoServiceCatalog.DAL.Db;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceCatalog.Tests.IntegrationTests.Infrastructure
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CarServiceContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<CarServiceContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

               
                services.AddSingleton(typeof(AutoServiceCatalog.BLL.Cache.ITwoLevelCacheService<>),
                    typeof(FakeTwoLevelCacheService<>));

                
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test", options => { });

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes("Test")
                        .RequireAuthenticatedUser()
                        .Build();
                });

                
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<CarServiceContext>();

                
                context.Database.OpenConnection();
                context.Database.EnsureCreated();

                
                Seeding.SeedAsync(context).GetAwaiter().GetResult();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
