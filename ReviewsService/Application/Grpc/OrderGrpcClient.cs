using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Grpc
{
    public class OrderGrpcClient
    {
        private readonly OrderService.OrderServiceClient _client;

        public OrderGrpcClient(OrderService.OrderServiceClient client)
        {
            _client = client;
        }

        public async Task<bool> ExistsAsync(int orderId)
        {
            try
            {
                var request = new OrderRequest { OrderId = orderId };
                var response = await _client.GetOrderAsync(request);
                return response != null && response.OrderId != 0;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
