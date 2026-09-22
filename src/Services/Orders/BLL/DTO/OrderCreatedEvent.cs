using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.BLL.DTO
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }
    }
}
