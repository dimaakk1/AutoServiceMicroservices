using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.BLL.DTO
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public Guid? VehicleId { get; set; }
        public string? VehicleDisplayName { get; set; }
        public string? VehicleVin { get; set; }
        public string? VehicleLicensePlate { get; set; }

    }
}
