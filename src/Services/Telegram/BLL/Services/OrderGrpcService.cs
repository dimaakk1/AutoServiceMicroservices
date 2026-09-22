using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceTelegram.BLL.Services
{
    public class OrderGrpcService
: IOrderGrpcService
    {

        private readonly OrderService.OrderServiceClient _client;


        public OrderGrpcService(
        OrderService.OrderServiceClient client)
        {
            _client = client;
        }


        public async Task<bool> UpdateStatusAsync(
        int id,
        string status)
        {

            var result =
            await _client.UpdateOrderStatusAsync(
            new UpdateOrderStatusRequest
            {
                OrderId = id,
                Status = status
            });


            return result.Success;

        }
        public async Task<OrderListResponse> GetTodayOrdersAsync()
        {
            var response =
                await _client.GetAllOrdersAsync(
                    new OrderFilterRequest
                    {
                        Date = DateTime.Today
                            .ToString("yyyy-MM-dd")
                    });


            return response;
        }
        public async Task<OrderListResponse> GetTomorrowOrdersAsync()
        {
            return await GetOrdersByDateAsync(
                DateTime.Today.AddDays(1));
        }



        public async Task<OrderListResponse> GetOrdersByDateAsync(
            DateTime date)
        {

            return await _client.GetAllOrdersAsync(
                new OrderFilterRequest
                {
                    Date = date.ToString("yyyy-MM-dd")
                });
        }




        public async Task<OrderListResponse> GetOrdersByStatusAsync(
            string status)
        {

            return await _client.GetAllOrdersAsync(
                new OrderFilterRequest
                {
                    Status = status
                });
        }




        public async Task<OrderListResponse> GetOrdersAsync(
            string? status = null,
            DateTime? date = null)
        {

            var request = new OrderFilterRequest();


            if (!string.IsNullOrEmpty(status))
            {
                request.Status = status;
            }


            if (date.HasValue)
            {
                request.Date =
                    date.Value.ToString("yyyy-MM-dd");
            }


            return await _client.GetAllOrdersAsync(request);
        }
    }

}