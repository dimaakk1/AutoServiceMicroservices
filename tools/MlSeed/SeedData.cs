using Bogus;

namespace MlSeed;

internal sealed record ServiceSpec(string Category, string Name, int Minutes, decimal Price);
internal sealed record Person(string Id, string Name, string UserName, string Role);
internal sealed record Car(Guid Id, string OwnerId, string Vin, string Make, string Model, int Year, int BaseMileage, string Color);
internal sealed record Repair(DateTime Slot, DateTime PlannedAt, DateTime StartedAt, DateTime CompletedAt,
    Person Mechanic, Car Vehicle, int MileageKm, ServiceSpec[] Services);

internal sealed record SeedData(Person[] Customers, Person[] Mechanics, Car[] Vehicles, Repair[] Repairs, DateTime PeriodStart)
{
    internal static readonly ServiceSpec[] Catalog =
    [
        new("Діагностика", "Комп’ютерна діагностика", 40, 700),
        new("Діагностика", "Комплексна перевірка автомобіля", 75, 1200),
        new("Діагностика", "Діагностика ходової частини", 45, 650),
        new("Технічне обслуговування", "Заміна моторної оливи", 45, 850),
        new("Технічне обслуговування", "Заміна повітряного фільтра", 20, 350),
        new("Технічне обслуговування", "Заміна гальмівної рідини", 55, 950),
        new("Гальмівна система", "Заміна передніх гальмівних колодок", 75, 1550),
        new("Гальмівна система", "Заміна гальмівних дисків", 105, 2450),
        new("Ходова частина", "Регулювання кутів розвалу та сходження", 55, 1050),
        new("Ходова частина", "Заміна амортизаторів", 140, 2800),
        new("Ходова частина", "Заміна стійок стабілізатора", 65, 1250),
        new("Двигун", "Заміна свічок запалювання", 55, 1050),
        new("Двигун", "Заміна комплекту ГРМ", 235, 5900),
        new("Двигун", "Промивання форсунок", 110, 2300),
        new("Електрика", "Діагностика електропроводки", 85, 1350),
        new("Електрика", "Заміна акумулятора", 35, 650),
        new("Електрика", "Ремонт генератора", 185, 3900),
        new("Шиномонтаж", "Сезонна заміна комплекту шин", 55, 1200),
        new("Шиномонтаж", "Балансування чотирьох коліс", 40, 800),
        new("Кліматична система", "Перевірка та заправка кондиціонера", 65, 1350),
        new("Кліматична система", "Заміна салонного фільтра", 25, 450),
        new("Кузовні роботи", "Локальне полірування кузова", 120, 2300),
        new("Кузовні роботи", "Антикорозійна обробка днища", 210, 4800)
    ];

    private static readonly (string Make, string Model)[] Models =
    [
        ("Toyota", "Corolla"), ("Volkswagen", "Golf"), ("Škoda", "Octavia"),
        ("Renault", "Mégane"), ("Ford", "Focus"), ("Hyundai", "Tucson"),
        ("Kia", "Ceed"), ("Mazda", "CX-5"), ("Nissan", "Qashqai"), ("Honda", "Civic")
    ];
    private static readonly string[] Colors = ["білий", "чорний", "сірий", "синій", "сріблястий"];

    internal static SeedData Generate(int count, int seed, DateOnly endDate)
    {
        var faker = new Faker("uk") { Random = new Randomizer(seed) };
        var days = new List<DateTime>();
        for (var day = endDate; days.Count * 7 < count; day = day.AddDays(-1))
            if (day.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                days.Add(day.ToDateTime(TimeOnly.MinValue));
        days.Reverse();
        var firstDay = days[0];

        Person MakePerson(int number, string role)
        {
            var userName = $"ml-{role.ToLowerInvariant()}-{number:D3}";
            return new(faker.Random.Guid().ToString(), faker.Name.FullName(), userName, role);
        }

        var customers = Enumerable.Range(1, Math.Clamp(count / 5, 12, 400))
            .Select(i => MakePerson(i, "User")).ToArray();
        var mechanics = Enumerable.Range(1, 10).Select(i => MakePerson(i, "Mechanic")).ToArray();
        var vehicles = customers.Select((customer, i) =>
        {
            var (make, model) = faker.PickRandom(Models);
            var age = faker.Random.Int(2, 24);
            return new Car(faker.Random.Guid(), customer.Id, $"ZZZ{i + 1:D14}", make, model,
                firstDay.Year - age, age * faker.Random.Int(7_000, 16_000), faker.PickRandom(Colors));
        }).ToArray();
        var drivingPerDay = vehicles.ToDictionary(car => car.Id, _ => faker.Random.Int(8, 64));
        var speed = mechanics.ToDictionary(person => person.Id, _ => faker.Random.Double(0.83, 1.17));
        var busyUntil = mechanics.ToDictionary(person => person.Id, _ => DateTime.MinValue);
        var shopZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        var repairs = new List<Repair>(count);

        foreach (var day in days)
        {
            var remainingVehicles = vehicles.ToList();
            for (var hour = 9; hour < 16 && repairs.Count < count; hour++)
            {
                var slot = day.AddHours(hour);
                var slotUtc = TimeZoneInfo.ConvertTimeToUtc(slot, shopZone);
                var vehicle = faker.PickRandom(remainingVehicles);
                remainingVehicles.Remove(vehicle);
                var primary = faker.PickRandom(Catalog);
                var related = Catalog.Where(x => x != primary && x.Category == primary.Category).ToArray();
                var services = faker.Random.Bool(0.28f) ? new[] { primary, faker.PickRandom(related) } : [primary];
                var usualMinutes = services.Sum(service => service.Minutes);
                var planned = slotUtc.AddMinutes(usualMinutes + faker.Random.Int(12, 55));
                var earliestStart = slotUtc.AddMinutes(faker.Random.Int(0, 25));
                var available = mechanics.Where(person => busyUntil[person.Id] <= earliestStart).ToArray();
                var mechanic = available.Length > 0
                    ? faker.PickRandom(available)
                    : mechanics.MinBy(person => busyUntil[person.Id])!;
                var started = earliestStart >= busyUntil[mechanic.Id] ? earliestStart : busyUntil[mechanic.Id];
                var mileage = vehicle.BaseMileage + (day - firstDay).Days * drivingPerDay[vehicle.Id];
                var variation = faker.Random.Double(0.75, 1.23) * speed[mechanic.Id];
                var inspection = faker.Random.Bool(0.19f) ? faker.Random.Int(15, 100) : 0;
                var wear = Math.Min(mileage / 22_000, 15);
                var completed = started.AddMinutes(Math.Max(15, Math.Round(usualMinutes * variation) + wear + inspection));
                busyUntil[mechanic.Id] = completed;
                repairs.Add(new(slot, planned, started, completed, mechanic, vehicle, mileage, services));
            }
        }
        return new(customers, mechanics, vehicles, repairs.ToArray(), firstDay);
    }
}
