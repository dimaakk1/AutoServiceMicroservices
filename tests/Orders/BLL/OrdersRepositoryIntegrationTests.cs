using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.DAL.Repositories;
using Microsoft.Data.SqlClient;
using Xunit;

namespace AutoserviceOrders.Tests.BLL;

public sealed class OrdersRepositoryIntegrationTests
{
    public sealed class SeededSqlFactAttribute : FactAttribute
    {
        public SeededSqlFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ORDERS_SQL_TEST_CONNECTION")))
                Skip = "Requires an isolated SQL Server with the default MlSeed dataset.";
        }
    }

    [SeededSqlFact]
    public async Task SeededOrders_CanBePagedAndFilteredWithOnlyMatchingItems()
    {
        var settings = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("ORDERS_SQL_TEST_CONNECTION"));
        Assert.Equal("OrdersMlSeed", settings.InitialCatalog);
        await using var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync();
        var orders = new OrderRepository(connection);
        var items = new OrderItemRepository(connection);
        var filter = new OrderReadFilter(null, null, "Completed", null, null, null, Limit: 25);
        var first = await orders.GetFilteredAsync(filter);
        var second = await orders.GetFilteredAsync(filter with { Offset = 25 });
        Assert.Equal(25, first.Count);
        Assert.Equal(25, second.Count);
        Assert.Empty(first.Select(x => x.OrderId).Intersect(second.Select(x => x.OrderId)));
        Assert.True(first[^1].OrderDate >= second[0].OrderDate);

        var day = first[0].OrderDate.Date;
        var daily = await orders.GetFilteredAsync(filter with { Date = day });
        Assert.NotEmpty(daily);
        Assert.All(daily, order => Assert.Equal(day, order.OrderDate.Date));
        var ids = first.Select(x => x.OrderId).ToArray();
        var matching = (await items.GetItemsByOrderIdsAsync(ids)).ToArray();
        Assert.Equal(ids.OrderBy(x => x).ToArray(), matching.Select(x => x.OrderId).Distinct().OrderBy(x => x).ToArray());
    }
}
