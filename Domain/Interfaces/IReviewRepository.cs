using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IReviewRepository
    {
        Task AddAsync(Review review);
        Task<IEnumerable<Review>> GetAllAsync();
        Task<IEnumerable<Review>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Review>> GetByOrderIdAsync(int orderId);
    }
}
