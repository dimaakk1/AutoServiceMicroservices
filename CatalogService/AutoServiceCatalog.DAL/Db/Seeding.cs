using AutoServiceCatalog.DAL.Entities;
using Bogus;
using Bogus.DataSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Db
{
    public static class Seeding
    {
        public static async Task SeedAsync(CarServiceContext context)
        {
            if (context.Categories.Any())
                return; // БД вже заповнена

            var faker = new Faker();

            // ----------------- Categories -----------------
            var categories = new List<Category>
            {
                new Category { Name = "Діагностика" },
                new Category { Name = "ТО" },
                new Category { Name = "Ремонт двигуна" },
                new Category { Name = "Кузовні роботи" }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // ----------------- Suppliers -----------------
            var suppliers = new Faker<Supplier>()
                .RuleFor(s => s.Name, f => f.Company.CompanyName())
                .RuleFor(s => s.Phone, f => f.Phone.PhoneNumber())
                .Generate(5);

            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();

            // ----------------- Services -----------------
            var serviceNames = new[]
            {
                "Діагностика двигуна", "Заміна масла", "Заміна фільтрів", "Регулювання розвалу-схождення",
                "Ремонт тормозів", "Заміна гальмівних колодок", "Заміна шин", "Балансування коліс",
                "Переди диагностика", "Чистка інжектора", "Заміна свічок займання",
                "Прошивка ЕБУ", "Полірування кузова", "Антикорозійна обробка", "Химчистка салону"
            };

            var services = new Faker<Service>()
                .RuleFor(s => s.Name, f => f.PickRandom(serviceNames))
                .RuleFor(s => s.Price, f => Math.Round(f.Random.Decimal(50, 1500), 2))
                .RuleFor(s => s.CategoryId, f => f.PickRandom(categories).CategoryId)
                .Generate(10);

            await context.Services.AddRangeAsync(services);
            await context.SaveChangesAsync(); // тут ServiceId автоматично присвоїться

            // ----------------- ServiceDetails (1:1) -----------------
            var manufacturers = new[] { "Bosch", "Valeo", "Denso", "Delphi", "NGK", "Hella", "Magneti Marelli", "Philips" };
            var warranties = new[] { "6 місяців", "12 місяців", "24 місяці" };

            var details = services.Select(s => new ServiceDetail
            {
                Manufacturer = faker.PickRandom(manufacturers),
                Warranty = faker.PickRandom(warranties),
                ServiceId = s.ServiceId, // FK автоматично
                Service = s
            }).ToList();

            await context.ServiceDetails.AddRangeAsync(details);
            await context.SaveChangesAsync();

            // ----------------- ServiceSuppliers (M:N) -----------------
            var random = new Random();
            var serviceSuppliers = new List<ServiceSupplier>();

            foreach (var service in services)
            {
                var selectedSuppliers = suppliers
                    .OrderBy(s => random.Next())
                    .Take(random.Next(1, 4)) // 1-3 suppliers per service
                    .ToList();

                foreach (var supplier in selectedSuppliers)
                {
                    serviceSuppliers.Add(new ServiceSupplier
                    {
                        ServiceId = service.ServiceId,
                        SupplierId = supplier.SupplierId
                    });
                }
            }

            await context.Set<ServiceSupplier>().AddRangeAsync(serviceSuppliers);
            await context.SaveChangesAsync();
        }
    }
}
