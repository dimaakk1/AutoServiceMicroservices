using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Entities
{
    public class Service
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ServiceDetail ServiceDetail { get; set; }
        public ICollection<ServiceSupplier> ServiceSuppliers { get; set; }
    }
}
