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
        private readonly IOrderCreatedPublisher _publisher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<List<OrderDto>> _ordersCache;
        public OrderService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    TwoLevelCacheService<List<OrderDto>> ordersCache,
    IOrderCreatedPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _ordersCache = ordersCache;
            _publisher = publisher;
        }

        public async Task<int> CreateOrderAsync(OrderDto orderDto)
        {
            var order = _mapper.Map<Order>(orderDto);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // =========================
                // CHECK SLOT
                // =========================

                var existingOrders = await _unitOfWork.Orders.GetAllAsync();

                var isTaken = existingOrders.Any(o =>
                    o.Status != "Cancelled" &&
                    o.OrderDate.Date == order.OrderDate.Date &&
                    o.OrderDate.Hour == order.OrderDate.Hour &&
                    o.OrderDate.Minute == order.OrderDate.Minute
                );

                if (isTaken)
                {
                    throw new Exception("Цей час вже зайнятий");
                }

                // =========================
                // CREATE
                // =========================

                var newId = await _unitOfWork.Orders.AddAsync(order);

                await _unitOfWork.CommitAsync();

                order.OrderId = newId;

                await _publisher.PublishOrderCreatedAsync(
    new OrderCreatedEvent
    {
        OrderId = order.OrderId,
        UserId = order.UserId,
                OrderDate = order.OrderDate
    });

  

                // invalidate cache
                await _ordersCache.InvalidateAsync("orders:all");
                await _ordersCache.InvalidateAsync($"order:{order.OrderId}");

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
                await _ordersCache.InvalidateAsync($"order:{orderId}");

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(string userId)
        {
            await _unitOfWork.BeginTransactionAsync();

            var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId);

            await _unitOfWork.CommitAsync();

            return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetMechanicOrdersAsync(string mechanicId)
        {
            await _unitOfWork.BeginTransactionAsync();
            var orders = await _unitOfWork.Orders!.GetByMechanicIdAsync(mechanicId);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task<bool> AssignMechanicAsync(int orderId, string mechanicId, string mechanicName)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var affected = await _unitOfWork.Orders!.AssignMechanicAsync(
                    orderId,
                    mechanicId,
                    mechanicName.Trim());

                await _unitOfWork.CommitAsync();
                await InvalidateOrderCacheAsync(orderId);
                return affected > 0;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateMechanicStatusAsync(int orderId, string mechanicId, string status)
        {
            var allowedStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Confirmed",
                "Diagnosing",
                "AwaitingApproval",
                "InProgress",
                "QualityCheck",
                "Completed"
            };

            if (!allowedStatuses.Contains(status))
                throw new ArgumentException("Недоступний статус замовлення.", nameof(status));

            var normalizedStatus = allowedStatuses.First(value =>
                value.Equals(status, StringComparison.OrdinalIgnoreCase));

            var now = DateTime.UtcNow;
            var startedAt = normalizedStatus is "Diagnosing" or "InProgress" ? now : (DateTime?)null;
            var completedAt = normalizedStatus == "Completed" ? now : (DateTime?)null;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var affected = await _unitOfWork.Orders!.UpdateMechanicStatusAsync(
                    orderId,
                    mechanicId,
                    normalizedStatus,
                    startedAt,
                    completedAt);

                await _unitOfWork.CommitAsync();
                await InvalidateOrderCacheAsync(orderId);
                return affected > 0;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private async Task InvalidateOrderCacheAsync(int orderId)
        {
            await _ordersCache.InvalidateAsync("orders:all");
            await _ordersCache.InvalidateAsync($"order:{orderId}");
        }


        public async Task<List<string>> GetTakenSlotsAsync(DateTime date)
        {
            await _unitOfWork.BeginTransactionAsync();

            var orders = await _unitOfWork.Orders.GetAllAsync();

            await _unitOfWork.CommitAsync();

            return orders
                .Where(o =>
                    o.Status != "Cancelled" &&
                    o.OrderDate.Date == date.Date
                )
                .Select(o => o.OrderDate.ToString("HH:mm"))
                .Distinct()
                .ToList();
        }
        public async Task<List<OrderDto>> GetOrdersByDateAsync(DateTime date)
        {
            await _unitOfWork.BeginTransactionAsync();

            var orders = await _unitOfWork.Orders.GetAllAsync();

            await _unitOfWork.CommitAsync();

            return _mapper.Map<List<OrderDto>>(
                orders.Where(o =>
                    o.Status != "Cancelled" &&
                    o.OrderDate.Date == date.Date
                ).ToList()
            );
        }

        public async Task<bool> UpdateStatusAsync(
            int orderId,
            string status)
        {
            var allowedStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Pending",
                "Confirmed",
                "Diagnosing",
                "AwaitingApproval",
                "InProgress",
                "QualityCheck",
                "Completed",
                "Cancelled"
            };

            if (!allowedStatuses.Contains(status))
                throw new ArgumentException("Недоступний статус замовлення.", nameof(status));

            var normalizedStatus = allowedStatuses.First(value =>
                value.Equals(status, StringComparison.OrdinalIgnoreCase));
            var now = DateTime.UtcNow;
            var startedAt = normalizedStatus is "Diagnosing" or "InProgress" ? now : (DateTime?)null;
            var completedAt = normalizedStatus == "Completed" ? now : (DateTime?)null;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var affected = await _unitOfWork.Orders.UpdateStatusAsync(
                    orderId,
                    normalizedStatus,
                    startedAt,
                    completedAt);
                await _unitOfWork.CommitAsync();
                await InvalidateOrderCacheAsync(orderId);
                return affected > 0;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }


    }

}
