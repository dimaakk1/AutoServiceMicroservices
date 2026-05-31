using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services;
using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.Tests.Common;
using AutoMapper;
using FluentAssertions;
using Grpc.Core;
using Moq;
using Part;
using Xunit;

namespace AutoserviceOrders.Tests.BLL;

public class OrderItemServiceTests
{
    private readonly UnitOfWorkMockBuilder _uow = new();
    private readonly Mock<IMapper> _mapper = new();

    private OrderItemService CreateSut(IReadOnlyDictionary<int, PartReply>? catalog = null) =>
        new(_uow.UnitOfWork.Object, _mapper.Object, new FakePartServiceClient(catalog));

    private static PartReply CreatePart(int id, string name, double price) =>
        new() { Id = id, Name = name, Price = price };

    [Fact]
    public async Task GetAllAsync_ReturnsMappedItems()
    {
        var sut = CreateSut();
        var items = new List<OrderItem>
        {
            new() { OrderItemId = 1, OrderId = 1, ProductId = 10, Quantity = 2 },
            new() { OrderItemId = 2, OrderId = 1, ProductId = 20, Quantity = 1 }
        };
        var dtos = new List<OrderItemDto>
        {
            new() { OrderItemId = 1, OrderId = 1, ProductId = 10, Quantity = 2 },
            new() { OrderItemId = 2, OrderId = 1, ProductId = 20, Quantity = 1 }
        };

        _uow.OrderItems.Setup(r => r.GetAllAsync()).ReturnsAsync(items);
        _mapper.Setup(m => m.Map<IEnumerable<OrderItemDto>>(items)).Returns(dtos);

        var result = await sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetItemsByOrderIdAsync_ReturnsItemsForOrder()
    {
        var sut = CreateSut();
        const int orderId = 5;
        var items = new List<OrderItem>
        {
            new() { OrderItemId = 1, OrderId = orderId, ProductId = 3, Quantity = 1 }
        };
        var dtos = new List<OrderItemDto>
        {
            new() { OrderItemId = 1, OrderId = orderId, ProductId = 3, Quantity = 1 }
        };

        _uow.OrderItems.Setup(r => r.GetItemsByOrderIdAsync(orderId)).ReturnsAsync(items);
        _mapper.Setup(m => m.Map<IEnumerable<OrderItemDto>>(items)).Returns(dtos);

        var result = await sut.GetItemsByOrderIdAsync(orderId);

        result.Should().ContainSingle().Which.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task AddOrderItemAsync_ValidProduct_PersistsItem()
    {
        var catalog = new Dictionary<int, PartReply>
        {
            [7] = CreatePart(7, "Brake pads", 120)
        };
        var sut = CreateSut(catalog);
        var dto = new OrderItemDto { OrderId = 1, ProductId = 7, Quantity = 2 };
        var entity = new OrderItem { OrderId = 1, ProductId = 7, Quantity = 2 };

        _mapper.Setup(m => m.Map<OrderItem>(dto)).Returns(entity);
        _uow.OrderItems.Setup(r => r.AddAsync(entity)).ReturnsAsync(1);

        await sut.AddOrderItemAsync(dto);

        _uow.OrderItems.Verify(r => r.AddAsync(entity), Times.Once);
        _uow.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task AddOrderItemAsync_ProductNotFound_ThrowsWithoutPersisting()
    {
        var sut = CreateSut();
        var dto = new OrderItemDto { OrderId = 1, ProductId = 999, Quantity = 1 };

        var act = () => sut.AddOrderItemAsync(dto);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Service (Product) with id 999 not found");
        _uow.OrderItems.Verify(r => r.AddAsync(It.IsAny<OrderItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOrderItemAsync_ValidItem_UpdatesEntity()
    {
        var sut = CreateSut();
        var dto = new OrderItemDto { OrderItemId = 1, OrderId = 1, ProductId = 2, Quantity = 5 };
        var entity = new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 2, Quantity = 5 };

        _mapper.Setup(m => m.Map<OrderItem>(dto)).Returns(entity);
        _uow.OrderItems.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);

        await sut.UpdateOrderItemAsync(dto);

        _uow.OrderItems.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderItemAsync_DeletesById()
    {
        var sut = CreateSut();
        const int orderItemId = 12;
        _uow.OrderItems.Setup(r => r.DeleteAsync(orderItemId)).ReturnsAsync(1);

        await sut.DeleteOrderItemAsync(orderItemId);

        _uow.OrderItems.Verify(r => r.DeleteAsync(orderItemId), Times.Once);
    }

    [Fact]
    public async Task GetOrdersWithItemsAsync_EnrichesItemsFromCatalog()
    {
        var catalog = new Dictionary<int, PartReply>
        {
            [5] = CreatePart(5, "Oil filter", 25.5)
        };
        var sut = CreateSut(catalog);
        const string userId = "user-1";
        var orders = new List<Order>
        {
            new()
            {
                OrderId = 1,
                UserId = userId,
                OrderDate = new DateTime(2026, 5, 1),
                Status = "Confirmed"
            }
        };
        var items = new List<OrderItem>
        {
            new() { OrderItemId = 10, OrderId = 1, ProductId = 5, Quantity = 2 }
        };

        _uow.Orders.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(orders);
        _uow.OrderItems.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        var result = (await sut.GetOrdersWithItemsAsync(userId)).ToList();

        result.Should().ContainSingle();
        var order = result[0];
        order.OrderId.Should().Be(1);
        order.Items.Should().ContainSingle();
        order.Items[0].ProductName.Should().Be("Oil filter");
        order.Items[0].Price.Should().Be(25.5m);
        order.Items[0].TotalPrice.Should().Be(51m);
    }

    [Fact]
    public async Task GetAllOrdersWithItemsAsync_AppliesStatusAndDateFilters()
    {
        var sut = CreateSut();
        var from = new DateTime(2026, 5, 1);
        var to = new DateTime(2026, 5, 31);
        var orders = new List<Order>
        {
            new() { OrderId = 1, UserId = "a", Status = "Confirmed", OrderDate = new DateTime(2026, 5, 10) },
            new() { OrderId = 2, UserId = "b", Status = "Pending", OrderDate = new DateTime(2026, 5, 10) },
            new() { OrderId = 3, UserId = "a", Status = "Confirmed", OrderDate = new DateTime(2026, 6, 1) }
        };

        _uow.Orders.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _uow.OrderItems.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var filter = new OrderFilterDto
        {
            UserId = "a",
            Status = "Confirmed",
            FromDate = from,
            ToDate = to
        };

        var result = (await sut.GetAllOrdersWithItemsAsync(filter)).ToList();

        result.Should().ContainSingle().Which.OrderId.Should().Be(1);
    }
}
