using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceTelegram.BLL.Services
{
    public interface IOrderGrpcService
    {
        Task<OrderListResponse> GetTodayOrdersAsync();


        Task<OrderListResponse> GetTomorrowOrdersAsync();


        Task<OrderListResponse> GetOrdersByDateAsync(
            DateTime date);


        Task<OrderListResponse> GetOrdersByStatusAsync(
            string status);


        Task<OrderListResponse> GetOrdersAsync(
            string? status = null,
            DateTime? date = null);


        Task<bool>
            UpdateStatusAsync(
                int id,
                string status);
    }
}
