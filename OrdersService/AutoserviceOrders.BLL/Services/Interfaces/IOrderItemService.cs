using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoserviceOrders.BLL.DTO;

namespace AutoserviceOrders.BLL.Services.Interfaces
{
    public interface IOrderItemService
    {
        Task<IEnumerable<OrderItemDto>> GetAllAsync();
        Task<IEnumerable<OrderItemDto>> GetItemsByOrderIdAsync(int orderId);
        Task AddOrderItemAsync(OrderItemDto dto);
        Task UpdateOrderItemAsync(OrderItemDto dto);
        Task DeleteOrderItemAsync(int orderItemId);
        Task<IEnumerable<OrderItemWithProductDto>> GetOrderItemsWithProductAsync();
    }
}
