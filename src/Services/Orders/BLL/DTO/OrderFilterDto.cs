using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceOrders.BLL.DTO
{
    public class OrderFilterDto
    {
        public string? UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? Date { get; set; }

        public DateTime? ToDate { get; set; }
        public string? UserName { get; set; }
    }
}
