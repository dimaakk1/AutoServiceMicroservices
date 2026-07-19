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


        public async Task<IEnumerable<OrderWithItemsDto>> GetOrdersWithItemsAsync(string userId)
        {
            await _unitOfWork.BeginTransactionAsync();

            // 🔥 тільки мої замовлення
            var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId);
            var items = await _unitOfWork.OrderItems.GetAllAsync();

            await _unitOfWork.CommitAsync();

            var result = new List<OrderWithItemsDto>();

            foreach (var order in orders)
            {
                var orderDto = new OrderWithItemsDto
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    Items = new List<OrderItemWithProductDto>()
                };

                var orderItems = items.Where(i => i.OrderId == order.OrderId);

                foreach (var item in orderItems)
                {
                    var part = await _partGrpcClient.GetPartAsync(
                        new GetPartRequest { Id = item.ProductId }
                    );

                    orderDto.Items.Add(new OrderItemWithProductDto
                    {
                        OrderItemId = item.OrderItemId,
                        OrderId = item.OrderId,
                        ProductId = item.ProductId,
                        ProductName = part.Name,
                        Price = Convert.ToDecimal(part.Price),
                        Quantity = item.Quantity,
                        TotalPrice = Convert.ToDecimal(part.Price) * item.Quantity
                    });
                }

                result.Add(orderDto);
            }

            return result;
        }

        public async Task<IEnumerable<OrderWithItemsDto>> GetAllOrdersWithItemsAsync(
     OrderFilterDto filter)
        {
            await _unitOfWork.BeginTransactionAsync();

            var orders = await _unitOfWork.Orders.GetAllAsync();
            var items = await _unitOfWork.OrderItems.GetAllAsync();

            await _unitOfWork.CommitAsync();

            var result = new List<OrderWithItemsDto>();

            foreach (var order in orders)
            {
                // 🔎 FILTERS
                if (!string.IsNullOrEmpty(filter.Status) &&
                    order.Status != filter.Status)
                    continue;

                if (filter.Date.HasValue &&
    order.OrderDate.Date != filter.Date.Value.Date)
                    continue;

                if (filter.FromDate.HasValue &&
                    order.OrderDate < filter.FromDate.Value)
                    continue;

                if (filter.ToDate.HasValue &&
                    order.OrderDate > filter.ToDate.Value)
                    continue;

                if (!string.IsNullOrEmpty(filter.UserId) &&
                    order.UserId != filter.UserId)
                    continue;

                var dto = new OrderWithItemsDto
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId, // 🔥 FIX
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    Items = new List<OrderItemWithProductDto>()
                };

                var orderItems = items.Where(i => i.OrderId == order.OrderId);

                foreach (var item in orderItems)
                {
                    var part = await _partGrpcClient.GetPartAsync(
                        new GetPartRequest { Id = item.ProductId }
                    );

                    dto.Items.Add(new OrderItemWithProductDto
                    {
                        OrderItemId = item.OrderItemId,
                        OrderId = item.OrderId,
                        ProductId = item.ProductId,
                        ProductName = part.Name,
                        Price = Convert.ToDecimal(part.Price),
                        Quantity = item.Quantity,
                        TotalPrice = Convert.ToDecimal(part.Price) * item.Quantity
                    });
                }

                result.Add(dto);
            }

            return result;
        }


    }
}
