using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.DTO
{
    public class ServiceDetailCreateDto
    {
        public int ServiceId { get; set; }
        public string Manufacturer { get; set; }
        public string Warranty { get; set; }
    }
}
