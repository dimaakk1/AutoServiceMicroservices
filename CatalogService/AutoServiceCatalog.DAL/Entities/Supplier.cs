using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Entities
{
    public class Supplier
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = null!;
        public string Phone { get; set; }

        public ICollection<ServiceSupplier> ServiceSuppliers { get; set; }
    }
}
