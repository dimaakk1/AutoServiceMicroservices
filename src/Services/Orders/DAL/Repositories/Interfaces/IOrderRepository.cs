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
        Task<List<Order>> GetFilteredAsync(OrderReadFilter filter);
        Task<int> UpdateAsync(Order order);
        Task<int> DeleteAsync(int orderId);
        Task<List<Order>> GetByUserIdAsync(string userId);
        Task<List<Order>> GetByMechanicIdAsync(string mechanicId);
        Task<int> AssignMechanicAsync(int orderId, string mechanicId, string mechanicName);
        Task<int> UpdateStatusAsync(int orderId, string status, DateTime? workStartedAt, DateTime? workCompletedAt);
        Task<int> UpdateMechanicStatusAsync(int orderId, string mechanicId, string status, DateTime? workStartedAt, DateTime? workCompletedAt);
        Task<int> SetPlannedCompletionAsync(int orderId, DateTime plannedCompletionUtc, DateTime plannedCompletionShopTime);
    }
}
