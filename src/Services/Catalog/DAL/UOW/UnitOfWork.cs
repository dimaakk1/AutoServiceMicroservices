using AutoServiceCatalog.DAL.Db;
using AutoServiceCatalog.DAL.Repositories.Intarfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CarServiceContext _context;

        public IServiceRepository Services { get; }
        public ICategoryRepository Categories { get; }
        public ISupplierRepository Suppliers { get; }
        public IServiceDetailRepository ServiceDetail { get; }
        public IServiceSupplierRepository ServiceSupplier { get; }

        public UnitOfWork(
            CarServiceContext context,
            IServiceRepository serviceRepository,
            ICategoryRepository categoryRepository,
            ISupplierRepository supplierRepository,
            IServiceSupplierRepository serviceSupplierRepository,
            IServiceDetailRepository serviceDetailRepository)
        {
            _context = context;
            Services = serviceRepository;
            Categories = categoryRepository;
            Suppliers = supplierRepository;
            ServiceDetail = serviceDetailRepository;
            ServiceSupplier = serviceSupplierRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
