using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoserviceOrders.BLL.DTO;

namespace AutoserviceOrders.BLL.Services.Interfaces
{
    public interface IOrderDetailsService
    {
        Task AddOrderDetailsAsync(OrderDetailsDto orderDetailsDto);
        Task UpdateOrderDetailsAsync(OrderDetailsDto orderDetailsDto);
        Task DeleteOrderDetailsAsync(int orderId);
        Task<OrderDetailsDto> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<OrderDetailsDto>> GetAllAsync();
    }
}
