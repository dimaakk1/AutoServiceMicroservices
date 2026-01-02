using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services.Interfaces;
using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.DAL.UnitOfWork;
using AutoserviceOrders.BLL.Cache;

namespace AutoserviceOrders.BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<List<OrderDto>> _ordersCache;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            TwoLevelCacheService<List<OrderDto>> ordersCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _ordersCache = ordersCache;
        }

        public async Task<int> CreateOrderAsync(OrderDto orderDto)
        {
            var order = _mapper.Map<Order>(orderDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int newId = await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.CommitAsync();

                // Інвалідація кешу після створення нового замовлення
                await _ordersCache.InvalidateAsync("orders:all");
                await _ordersCache.InvalidateAsync($"orders:customer:{order.CustomerId}");
                await _ordersCache.InvalidateAsync($"order:{newId}");

                return newId;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderDto> GetOrderByIdAsync(int orderId)
        {
            var key = $"order:{orderId}";
            return await _ordersCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                    await _unitOfWork.CommitAsync();

                    return order != null ? new List<OrderDto> { _mapper.Map<OrderDto>(order) } : new List<OrderDto>();
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ).ContinueWith(t => t.Result.FirstOrDefault()) ?? null!;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            return await _ordersCache.GetOrCreateAsync(
                key: "orders:all",
                factory: async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    var orders = await _unitOfWork.Orders.GetAllAsync();
                    await _unitOfWork.CommitAsync();

                    return _mapper.Map<List<OrderDto>>(orders);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<OrderDto>();
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(int customerId)
        {
            var key = $"orders:customer:{customerId}";
            return await _ordersCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    var orders = await _unitOfWork.Orders.GetOrdersByCustomerAsync(customerId);
                    await _unitOfWork.CommitAsync();

                    return _mapper.Map<List<OrderDto>>(orders);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<OrderDto>();
        }

        public async Task<bool> UpdateOrderAsync(OrderDto orderDto)
        {
            var order = _mapper.Map<Order>(orderDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int affected = await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.CommitAsync();

                // Інвалідуємо кеш
                await _ordersCache.InvalidateAsync("orders:all");
                await _ordersCache.InvalidateAsync($"orders:customer:{order.CustomerId}");
                await _ordersCache.InvalidateAsync($"order:{order.OrderId}");

                return affected > 0;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return false;
                }

                int affected = await _unitOfWork.Orders.DeleteAsync(orderId);
                await _unitOfWork.CommitAsync();

                // Інвалідуємо кеш
                await _ordersCache.InvalidateAsync("orders:all");
                await _ordersCache.InvalidateAsync($"orders:customer:{order.CustomerId}");
                await _ordersCache.InvalidateAsync($"order:{orderId}");

                return affected > 0;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ConfirmOrderAsync(int orderId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return false;
                }

                order.Status = "Confirmed";
                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.CommitAsync();

                // Інвалідуємо кеш
                await _ordersCache.InvalidateAsync("orders:all");
                await _ordersCache.InvalidateAsync($"orders:customer:{order.CustomerId}");
                await _ordersCache.InvalidateAsync($"order:{orderId}");

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }

}
