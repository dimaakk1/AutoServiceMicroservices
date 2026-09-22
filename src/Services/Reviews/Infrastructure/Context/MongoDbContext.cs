using Domain.Entities;
using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Context
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("mongo");                          

            string databaseName = configuration["MongoDbSettings:DatabaseName"] ?? "reviewsdb";

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Review> Reviews =>
            _database.GetCollection<Review>("reviews");
    }
}
