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
using AutoserviceOrders.BLL.Cache;

namespace AutoserviceOrders.BLL.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<List<CustomerDto>> _customersCache;

        public CustomerService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            TwoLevelCacheService<List<CustomerDto>> customersCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _customersCache = customersCache;
        }

        public async Task AddCustomerAsync(CustomerDto customerDto)
        {
            var customer = _mapper.Map<Customer>(customerDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.CommitAsync();

                // Інвалідуємо кеш
                await _customersCache.InvalidateAsync("customers:all");
                await _customersCache.InvalidateAsync($"customer:{customer.CustomerId}");
                await _customersCache.InvalidateAsync($"customer:email:{customer.Email}");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateCustomerAsync(CustomerDto customerDto)
        {
            var customer = _mapper.Map<Customer>(customerDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var oldCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.CustomerId);
                await _unitOfWork.Customers.UpdateAsync(customer);
                await _unitOfWork.CommitAsync();

                // Інвалідуємо кеш
                await _customersCache.InvalidateAsync("customers:all");
                await _customersCache.InvalidateAsync($"customer:{customer.CustomerId}");
                if (oldCustomer != null)
                    await _customersCache.InvalidateAsync($"customer:email:{oldCustomer.Email}");
                await _customersCache.InvalidateAsync($"customer:email:{customer.Email}");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteCustomerAsync(int customerId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                if (customer == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return;
                }

                await _unitOfWork.Customers.DeleteAsync(customerId);
                await _unitOfWork.CommitAsync();

                // Інвалідуємо кеш
                await _customersCache.InvalidateAsync("customers:all");
                await _customersCache.InvalidateAsync($"customer:{customerId}");
                await _customersCache.InvalidateAsync($"customer:email:{customer.Email}");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            return await _customersCache.GetOrCreateAsync(
                key: "customers:all",
                factory: async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    var customers = await _unitOfWork.Customers.GetAllAsync();
                    await _unitOfWork.CommitAsync();

                    return _mapper.Map<List<CustomerDto>>(customers);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? new List<CustomerDto>();
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int customerId)
        {
            var key = $"customer:{customerId}";
            return await _customersCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                    await _unitOfWork.CommitAsync();

                    return customer != null ? new List<CustomerDto> { _mapper.Map<CustomerDto>(customer) } : new List<CustomerDto>();
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ).ContinueWith(t => t.Result.FirstOrDefault()) ?? null!;
        }

        public async Task<CustomerDto> GetCustomerByEmailAsync(string email)
        {
            var key = $"customer:email:{email}";
            return await _customersCache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    var customer = await _unitOfWork.Customers.GetByEmailAsync(email);
                    await _unitOfWork.CommitAsync();

                    return customer != null ? new List<CustomerDto> { _mapper.Map<CustomerDto>(customer) } : new List<CustomerDto>();
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ).ContinueWith(t => t.Result.FirstOrDefault()) ?? null!;
        }
    }

}
