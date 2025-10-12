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

namespace AutoserviceOrders.BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<int> CreateOrderAsync(OrderDto orderDto)
        {
            var order = _mapper.Map<Order>(orderDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int newId = await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.CommitAsync();
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
            await _unitOfWork.BeginTransactionAsync();
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            await _unitOfWork.BeginTransactionAsync();
            var orders = await _unitOfWork.Orders.GetAllAsync();
            await _unitOfWork.CommitAsync();

            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(int customerId)
        {
            await _unitOfWork.BeginTransactionAsync();
            var orders = await _unitOfWork.Orders.GetOrdersByCustomerAsync(customerId);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<bool> UpdateOrderAsync(OrderDto orderDto)
        {
            var order = _mapper.Map<Order>(orderDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int affected = await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.CommitAsync();
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
                int affected = await _unitOfWork.Orders.DeleteAsync(orderId);
                await _unitOfWork.CommitAsync();
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
