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

        public async Task<OrderResponse> GetOrderAsync(int orderId)
        {
            try
            {
                return await _client.GetOrderAsync(new OrderRequest
                {
                    OrderId = orderId
                });
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
