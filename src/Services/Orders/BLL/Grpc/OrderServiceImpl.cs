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
                OrderDate = order.OrderDate.ToString("O"),
                VehicleId = order.VehicleId?.ToString() ?? string.Empty,
                VehicleDisplayName = order.VehicleDisplayName ?? string.Empty,
                VehicleVin = order.VehicleVin ?? string.Empty,
                VehicleLicensePlate = order.VehicleLicensePlate ?? string.Empty
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
                Date = ParseDate(request.Date),
                FromDate = ParseDate(request.FromDate),
                ToDate = ParseDate(request.ToDate),
                Offset = request.Offset,
                Limit = request.Limit > 0 ? request.Limit : null
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
                    VehicleId = o.VehicleId?.ToString() ?? string.Empty,
                    VehicleDisplayName = o.VehicleDisplayName ?? string.Empty,
                    VehicleVin = o.VehicleVin ?? string.Empty,
                    VehicleLicensePlate = o.VehicleLicensePlate ?? string.Empty,
                    MechanicId = o.MechanicId ?? string.Empty,
                    MechanicName = o.MechanicName ?? string.Empty,
                    WorkStartedAt = o.WorkStartedAt?.ToString("O") ?? string.Empty,
                    WorkCompletedAt = o.WorkCompletedAt?.ToString("O") ?? string.Empty,
                    PlannedCompletionAt = o.PlannedCompletionAt?.ToString("O") ?? string.Empty,

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

        private static DateTime? ParseDate(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, out var date))
                return date;
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid order date filter."));
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
