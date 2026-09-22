using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoserviceOrders.BLL.Services.Interfaces;
using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.DAL.UnitOfWork;
using AutoserviceOrders.BLL.DTO;

namespace AutoserviceOrders.BLL.Services
{
    public class OrderDetailsService : IOrderDetailsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderDetailsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddOrderDetailsAsync(OrderDetailsDto orderDetailsDto)
        {
            var orderDetails = _mapper.Map<OrderDetails>(orderDetailsDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.OrderDetails.AddAsync(orderDetails);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOrderDetailsAsync(OrderDetailsDto orderDetailsDto)
        {
            var orderDetails = _mapper.Map<OrderDetails>(orderDetailsDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.OrderDetails.UpdateAsync(orderDetails);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteOrderDetailsAsync(int orderId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.OrderDetails.DeleteAsync(orderId);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderDetailsDto> GetByOrderIdAsync(int orderId)
        {
            await _unitOfWork.BeginTransactionAsync();
            var orderDetails = await _unitOfWork.OrderDetails.GetByOrderIdAsync(orderId);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<OrderDetailsDto>(orderDetails);
        }

        public async Task<IEnumerable<OrderDetailsDto>> GetAllAsync()
        {
            await _unitOfWork.BeginTransactionAsync();
            var all = await _unitOfWork.OrderDetails.GetAllAsync();
            await _unitOfWork.CommitAsync();

            return _mapper.Map<IEnumerable<OrderDetailsDto>>(all);
        }
    }
}
