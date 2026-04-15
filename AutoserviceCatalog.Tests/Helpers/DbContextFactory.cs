using AutoServiceCatalog.DAL.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceCatalog.Tests.Helpers
{
    public static class DbContextFactory
    {
        public static CarServiceContext Create()
        {
            var options = new DbContextOptionsBuilder<CarServiceContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new CarServiceContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
