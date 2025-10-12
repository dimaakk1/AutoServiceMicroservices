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
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddProductAsync(ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateProductAsync(ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Products.DeleteAsync(productId);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            await _unitOfWork.BeginTransactionAsync();
            var products = await _unitOfWork.Products.GetAllAsync();
            await _unitOfWork.CommitAsync();

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            await _unitOfWork.BeginTransactionAsync();
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByNameAsync(string name)
        {
            await _unitOfWork.BeginTransactionAsync();
            var products = await _unitOfWork.Products.GetProductsByNameAsync(name);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
    }
}
