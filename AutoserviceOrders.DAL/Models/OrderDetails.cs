using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.DAL.Models
{
    public class OrderDetails
    {
        public int OrderId { get; set; }
        public string MechanicName { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
    }
}
