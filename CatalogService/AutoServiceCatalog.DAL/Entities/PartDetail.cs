using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Entities
{
    public class ServiceDetail
    {
        public int ServiceDetailId { get; set; }
        public string Manufacturer { get; set; } = null!;
        public string Warranty { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }
}
