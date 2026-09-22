using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Entities
{
    public class ServiceSupplier
    {
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
    }
}
