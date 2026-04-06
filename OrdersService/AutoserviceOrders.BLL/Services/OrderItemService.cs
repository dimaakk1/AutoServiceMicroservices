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

        public async Task<IEnumerable<OrderItemWithProductDto>> GetOrderItemsWithProductAsync()
        {
            await _unitOfWork.BeginTransactionAsync();
            var result = await _unitOfWork.OrderItems.GetOrderItemsWithProductAsync();
            await _unitOfWork.CommitAsync();

            var dtoList = new List<OrderItemWithProductDto>();

            foreach (var item in result)
            {
                try
                {
                    var part = await _partGrpcClient.GetPartAsync(new GetPartRequest { Id = item.ProductId });
                    dtoList.Add(new OrderItemWithProductDto
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
                catch (Exception ex)
                {
                    // Log or handle error - if part not found, skip or use cached data
                    dtoList.Add(new OrderItemWithProductDto
                    {
                        OrderItemId = item.OrderItemId,
                        OrderId = item.OrderId,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Price = item.Price,
                        Quantity = item.Quantity,
                        TotalPrice = item.Price * item.Quantity
                    });
                }
            }

            return dtoList;
        }
    }
}
