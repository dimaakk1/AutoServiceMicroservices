using AutoserviceOrders.BLL.DTO;
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
        private readonly IOrderItemService _orderItemService;

        public OrderServiceImpl(IOrderService orderService, IOrderItemService orderItemService)
        {
            _orderService = orderService;
            _orderItemService = orderItemService;
        }

        public override async Task<OrderResponse> GetOrder(OrderRequest request, ServerCallContext context)
        {
            var order = await _orderService.GetOrderByIdAsync(request.OrderId);
            return new OrderResponse
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                Status = order.Status,
                OrderDate = order.OrderDate.ToString("O")
            };
        }

        public override async Task<OrderListResponse> GetAllOrders(
        OrderFilterRequest request,
        ServerCallContext context)
        {
            var filter = new OrderFilterDto
            {
                UserId = request.UserId,
                Status = request.Status,
                Date = string.IsNullOrEmpty(request.Date)
        ? null
        : DateTime.Parse(request.Date)
            };


            var orders =
                await _orderItemService.GetAllOrdersWithItemsAsync(filter);

            var response = new OrderListResponse();

            response.Orders.AddRange(
                orders.Select(o => new OrderResponse
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId, // 🔥 FIX
                    Status = o.Status,
                    OrderDate = o.OrderDate.ToString("O"),

                    Items =
                    {
                    o.Items.Select(i => new OrderItemResponse
                    {
                        OrderItemId = i.OrderItemId,
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Price = (double)i.Price,
                        Quantity = i.Quantity,
                        TotalPrice = (double)i.TotalPrice
                    })
                    }
                })
            );

            return response;
        }

        public override async Task<UpdateOrderStatusResponse>
UpdateOrderStatus(
    UpdateOrderStatusRequest request,
    ServerCallContext context)
        {

            try
            {

                await _orderService.UpdateStatusAsync(
                    request.OrderId,
                    request.Status);


                return new UpdateOrderStatusResponse
                {
                    Success = true,
                    Message = "Статус оновлено"
                };

            }
            catch (Exception ex)
            {

                return new UpdateOrderStatusResponse
                {
                    Success = false,
                    Message = ex.Message
                };

            }

        }

    }
}