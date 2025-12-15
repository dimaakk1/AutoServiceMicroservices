using AutoserviceOrders.BLL.Services.Interfaces;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.BLL.Grpc
{
    public class OrderServiceImpl : OrderService.OrderServiceBase
    {
        private readonly IOrderService _orderService;

        public OrderServiceImpl(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public override async Task<OrderResponse> GetOrder(OrderRequest request, ServerCallContext context)
        {
            var order = await _orderService.GetOrderByIdAsync(request.OrderId);
            return new OrderResponse
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                Status = order.Status,
                OrderDate = order.OrderDate.ToString("O")
            };
        }

    }
}
