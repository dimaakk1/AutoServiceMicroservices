using AutoserviceOrders.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.DAL.Repositories.Interfaces
{
    public interface IOrderDetailsRepository : IGenericRepository<OrderDetails>
    {
        Task<OrderDetails> GetByOrderIdAsync(int orderId);
    }
}
