using AutoMapper;
using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services.Interfaces;
using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.DAL.UnitOfWork;
using Grpc.Core;
using Part;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.BLL.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly PartService.PartServiceClient _partGrpcClient;

        public OrderItemService(IUnitOfWork unitOfWork, IMapper mapper, PartService.PartServiceClient partGrpcClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _partGrpcClient = partGrpcClient;
        }

        public async Task<IEnumerable<OrderItemDto>> GetAllAsync()
        {
            await _unitOfWork.BeginTransactionAsync();
            var all = await _unitOfWork.OrderItems.GetAllAsync();
            await _unitOfWork.CommitAsync();

            return _mapper.Map<IEnumerable<OrderItemDto>>(all);
        }

        public async Task<IEnumerable<OrderItemDto>> GetItemsByOrderIdAsync(int orderId)
        {
            await _unitOfWork.BeginTransactionAsync();
            var items = await _unitOfWork.OrderItems.GetItemsByOrderIdAsync(orderId);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<IEnumerable<OrderItemDto>>(items);
        }

        public async Task AddOrderItemAsync(OrderItemDto dto)
        {
            try
            {
                // 🔹 Викликаємо існуючий gRPC метод
                var part = await _partGrpcClient.GetPartAsync(new GetPartRequest { Id = dto.ProductId });

                // part існує → можна додавати
                var entity = _mapper.Map<DAL.Models.OrderItem>(dto);

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    await _unitOfWork.OrderItems.AddAsync(entity);
                    await _unitOfWork.CommitAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                // Якщо gRPC кинув NotFound → сервіс не існує
                throw new Exception($"Service (Product) with id {dto.ProductId} not found");
            }
        }

        public async Task UpdateOrderItemAsync(OrderItemDto dto)
        {
            var entity = _mapper.Map<DAL.Models.OrderItem>(dto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.OrderItems.UpdateAsync(entity);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteOrderItemAsync(int orderItemId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.OrderItems.DeleteAsync(orderItemId);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }


        public Task<IEnumerable<OrderWithItemsDto>> GetOrdersWithItemsAsync(string userId) =>
            LoadOrdersAsync(() => _unitOfWork.Orders!.GetByUserIdAsync(userId));

        public Task<IEnumerable<OrderWithItemsDto>> GetAllOrdersWithItemsAsync(OrderFilterDto filter)
        {
            static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
            var query = new OrderReadFilter(Optional(filter.UserId), Optional(filter.MechanicId),
                Optional(filter.Status), filter.Date, filter.FromDate, filter.ToDate,
                Math.Max(0, filter.Offset), filter.Limit is int limit ? Math.Clamp(limit, 1, 101) : null);
            return LoadOrdersAsync(() => _unitOfWork.Orders!.GetFilteredAsync(query));
        }

        private async Task<IEnumerable<OrderWithItemsDto>> LoadOrdersAsync(Func<Task<List<Order>>> getOrders)
        {
            List<Order> orders;
            IEnumerable<OrderItem> items;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                orders = await getOrders();
                items = orders.Count == 0
                    ? Array.Empty<OrderItem>()
                    : await _unitOfWork.OrderItems!.GetItemsByOrderIdsAsync(orders.Select(o => o.OrderId).ToArray());
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            var itemsByOrder = items.ToLookup(item => item.OrderId);
            var parts = new Dictionary<int, PartReply>();
            var deadline = DateTime.UtcNow.AddSeconds(6);
            foreach (var ids in items.Select(item => item.ProductId).Distinct().Chunk(500))
            {
                var request = new GetPartsByIdsRequest();
                request.Ids.AddRange(ids);
                var reply = await _partGrpcClient.GetPartsByIdsAsync(request, deadline: deadline);
                foreach (var part in reply.Parts)
                    parts[part.Id] = part;
            }

            return orders.Select(order => new OrderWithItemsDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                VehicleId = order.VehicleId,
                VehicleDisplayName = order.VehicleDisplayName,
                VehicleVin = order.VehicleVin,
                VehicleLicensePlate = order.VehicleLicensePlate,
                MechanicId = order.MechanicId,
                MechanicName = order.MechanicName,
                WorkStartedAt = order.WorkStartedAt,
                WorkCompletedAt = order.WorkCompletedAt,
                PlannedCompletionAt = order.PlannedCompletionAt,
                Items = itemsByOrder[order.OrderId].Select(item =>
                {
                    if (!parts.TryGetValue(item.ProductId, out var part))
                        throw new RpcException(new Status(StatusCode.NotFound, $"Catalog service {item.ProductId} was not found."));
                    var price = Convert.ToDecimal(part.Price);
                    return new OrderItemWithProductDto
                    {
                        OrderItemId = item.OrderItemId,
                        OrderId = item.OrderId,
                        ProductId = item.ProductId,
                        ProductName = part.Name,
                        Price = price,
                        Quantity = item.Quantity,
                        TotalPrice = price * item.Quantity
                    };
                }).ToList()
            }).ToList();
        }
    }
}
