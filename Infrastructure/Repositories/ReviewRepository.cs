using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IMongoCollection<Review> _reviews;

        public ReviewRepository(MongoDbContext context)
        {
            _reviews = context.Reviews;
        }

        public async Task AddAsync(Review review)
        {
            await _reviews.InsertOneAsync(review);
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _reviews.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByCustomerIdAsync(int customerId)
        {
            return await _reviews.Find(r => r.CustomerId == customerId).ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByOrderIdAsync(int orderId)
        {
            return await _reviews.Find(r => r.OrderId == orderId).ToListAsync();
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _reviews.DeleteOneAsync(r => r.Id == id);
            return result.DeletedCount > 0; 
        }
        public async Task<Review> GetByIdAsync(string id)
        {
            return await _reviews.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Review review)
        {
            var filter = Builders<Review>.Filter.Eq(r => r.Id, review.Id);
            await _reviews.ReplaceOneAsync(filter, review);
        }
    }
}
