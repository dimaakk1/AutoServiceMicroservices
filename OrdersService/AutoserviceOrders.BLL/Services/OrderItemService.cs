using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.DAL.UnitOfWork;
using AutoserviceOrders.BLL.Services.Interfaces;

namespace AutoserviceOrders.BLL.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDetailsDto>> GetAllAsync()
        {
            await _unitOfWork.BeginTransactionAsync();
            var all = await _unitOfWork.OrderItems.GetAllAsync();
            await _unitOfWork.CommitAsync();

            return _mapper.Map<IEnumerable<OrderDetailsDto>>(all);
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

        public async Task<IEnumerable<OrderItemWithProductDto>> GetOrderItemsWithProductAsync()
        {
            await _unitOfWork.BeginTransactionAsync();
            var result = await _unitOfWork.OrderItems.GetOrderItemsWithProductAsync();
            await _unitOfWork.CommitAsync();

            return _mapper.Map<IEnumerable<OrderItemWithProductDto>>(result);
        }
    }
}
