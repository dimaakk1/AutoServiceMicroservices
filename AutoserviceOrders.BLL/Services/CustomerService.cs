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
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddCustomerAsync(CustomerDto customerDto)
        {
            var customer = _mapper.Map<Customer>(customerDto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.CommitAsync();
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
                await _unitOfWork.Customers.UpdateAsync(customer);
                await _unitOfWork.CommitAsync();
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
                await _unitOfWork.Customers.DeleteAsync(customerId);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            await _unitOfWork.BeginTransactionAsync();
            var customers = await _unitOfWork.Customers.GetAllAsync();
            await _unitOfWork.CommitAsync();

            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int customerId)
        {
            await _unitOfWork.BeginTransactionAsync();
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> GetCustomerByEmailAsync(string email)
        {
            await _unitOfWork.BeginTransactionAsync();
            var customer = await _unitOfWork.Customers.GetByEmailAsync(email);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<CustomerDto>(customer);
        }
    }
}
