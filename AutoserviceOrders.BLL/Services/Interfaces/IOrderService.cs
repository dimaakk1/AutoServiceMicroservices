using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.BLL.Services.Interfaces
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(OrderDto orderDto);
        Task<OrderDto> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(int customerId);
        Task<bool> UpdateOrderAsync(OrderDto orderDto);
        Task<bool> DeleteOrderAsync(int orderId);
        Task<bool> ConfirmOrderAsync(int orderId);
    }
}
