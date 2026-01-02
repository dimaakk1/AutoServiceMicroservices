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
using AutoserviceOrders.BLL.Cache;

namespace AutoserviceOrders.BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<List<ProductDto>> _productsCache;

        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            TwoLevelCacheService<List<ProductDto>> productsCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _productsCache = productsCache;
        }

        public async Task AddProductAsync(ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.CommitAsync();

                // Інвалідуємо кеш
                await _productsCache.InvalidateAsync("products:all");
                await _productsCache.InvalidateAsync($"product:{product.ProductId}");
                await _productsCache.InvalidateAsync($"products:name:{product.Name}");
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
                var oldProduct = await _unitOfWork.Products.GetByIdAsync(product.ProductId);
                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.CommitAsync();

                // Інвалідуємо кеш
                await _productsCache.InvalidateAsync("products:all");
                await _productsCache.InvalidateAsync($"product:{product.ProductId}");
                if (oldProduct != null)
                    await _productsCache.InvalidateAsync($"products:name:{oldProduct.Name}");
                await _productsCache.InvalidateAsync($"products:name:{product.Name}");
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
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return;
                }

                await _unitOfWork.Products.DeleteAsync(productId);
                await _unitOfWork.CommitAsync();

                // Інвалідуємо кеш
                await _productsCache.InvalidateAsync("products:all");
                await _productsCache.InvalidateAsync($"product:{productId}");
                await _productsCache.InvalidateAsync($"products:name:{product.Name}");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            return await _productsCache.GetOrCreateAsync(
                key: "products:all",
                factory: async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    var products = await _unitOfWork.Products.GetAllAsync();
                    await _unitOfWork.CommitAsync();

                    return _mapper.Map<List<ProductDto>>(products);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<ProductDto>();
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var key = $"product:{productId}";
            return await _productsCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    var product = await _unitOfWork.Products.GetByIdAsync(productId);
                    await _unitOfWork.CommitAsync();

                    return product != null ? new List<ProductDto> { _mapper.Map<ProductDto>(product) } : new List<ProductDto>();
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ).ContinueWith(t => t.Result.FirstOrDefault()) ?? null!;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByNameAsync(string name)
        {
            var key = $"products:name:{name}";
            return await _productsCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    var products = await _unitOfWork.Products.GetProductsByNameAsync(name);
                    await _unitOfWork.CommitAsync();

                    return _mapper.Map<List<ProductDto>>(products);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<ProductDto>();
        }
    }

}
