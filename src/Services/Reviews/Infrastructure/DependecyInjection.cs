using Domain.Interfaces;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = new MongoDbSettings();
            configuration.GetSection("MongoDbSettings").Bind(settings);

            services.AddSingleton(settings);
            services.AddSingleton<MongoDbContext>();
            services.AddScoped<IReviewRepository, ReviewRepository>();

            return services;
        }
    }
}
