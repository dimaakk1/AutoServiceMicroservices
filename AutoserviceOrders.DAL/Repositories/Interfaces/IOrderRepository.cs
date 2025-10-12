using AutoserviceOrders.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.DAL.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<int> AddAsync(Order order);
        Task<Order> GetByIdAsync(int orderId);
        Task<List<Order>> GetAllAsync();
        Task<int> UpdateAsync(Order order);
        Task<int> DeleteAsync(int orderId);
        Task<List<Order>> GetOrdersByCustomerAsync(int customerId);
    }
}
