using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.Tests.Common;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using OrderBLL = AutoserviceOrders.BLL.Services.OrderService;

namespace AutoserviceOrders.Tests.BLL;

public class OrderServiceTests
{
    private readonly UnitOfWorkMockBuilder _uow = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly OrderBLL _sut;

    public OrderServiceTests()
    {
        _sut = new OrderBLL(
            _uow.UnitOfWork.Object,
            _mapper.Object,
            TestCacheFactory.CreateOrdersCache());
    }

    [Fact]
    public async Task CreateOrderAsync_ValidOrder_ReturnsNewId()
    {
        var orderDate = new DateTime(2026, 5, 22, 10, 0, 0, DateTimeKind.Utc);
        var dto = new OrderDto { UserId = "user-1", OrderDate = orderDate, Status = "Pending" };
        var entity = new Order { UserId = "user-1", OrderDate = orderDate, Status = "Pending" };

        _mapper.Setup(m => m.Map<Order>(dto)).Returns(entity);
        _uow.Orders.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        _uow.Orders.Setup(r => r.AddAsync(entity)).ReturnsAsync(42);

        var result = await _sut.CreateOrderAsync(dto);

        result.Should().Be(42);
        _uow.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _uow.UnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_SlotAlreadyTaken_ThrowsAndRollsBack()
    {
        var orderDate = new DateTime(2026, 5, 22, 14, 30, 0, DateTimeKind.Utc);
        var dto = new OrderDto { UserId = "user-1", OrderDate = orderDate, Status = "Pending" };
        var entity = new Order { UserId = "user-1", OrderDate = orderDate, Status = "Pending" };
        var existing = new Order
        {
            OrderId = 1,
            UserId = "user-2",
            OrderDate = orderDate,
            Status = "Confirmed"
        };

        _mapper.Setup(m => m.Map<Order>(dto)).Returns(entity);
        _uow.Orders.Setup(r => r.GetAllAsync()).ReturnsAsync([existing]);

        var act = () => _sut.CreateOrderAsync(dto);

        await act.Should().ThrowAsync<Exception>().WithMessage("??? ??? ??? ????????");
        _uow.UnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        _uow.Orders.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_OnlyCancelledOrderAtSameTime_AllowsBooking()
    {
        var orderDate = new DateTime(2026, 5, 22, 9, 0, 0, DateTimeKind.Utc);
        var dto = new OrderDto { UserId = "user-1", OrderDate = orderDate, Status = "Pending" };
        var entity = new Order { UserId = "user-1", OrderDate = orderDate, Status = "Pending" };
        var cancelled = new Order
        {
            OrderId = 5,
            UserId = "user-2",
            OrderDate = orderDate,
            Status = "Cancelled"
        };

        _mapper.Setup(m => m.Map<Order>(dto)).Returns(entity);
        _uow.Orders.Setup(r => r.GetAllAsync()).ReturnsAsync([cancelled]);
        _uow.Orders.Setup(r => r.AddAsync(entity)).ReturnsAsync(10);

        var result = await _sut.CreateOrderAsync(dto);

        result.Should().Be(10);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ExistingOrder_ReturnsMappedDto()
    {
        const int orderId = 7;
        var order = new Order
        {
            OrderId = orderId,
            UserId = "user-1",
            OrderDate = DateTime.UtcNow,
            Status = "Confirmed"
        };
        var dto = new OrderDto { OrderId = orderId, UserId = "user-1", Status = "Confirmed" };

        _uow.Orders.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        _mapper.Setup(m => m.Map<OrderDto>(order)).Returns(dto);

        var result = await _sut.GetOrderByIdAsync(orderId);

        result.Should().NotBeNull();
        result!.OrderId.Should().Be(orderId);
        result.UserId.Should().Be("user-1");
    }

    [Fact]
    public async Task GetOrderByIdAsync_NotFound_ReturnsNull()
    {
        const int orderId = 404;
        _uow.Orders.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        var result = await _sut.GetOrderByIdAsync(orderId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsMappedList()
    {
        var orders = new List<Order>
        {
            new() { OrderId = 1, UserId = "a", Status = "Pending" },
            new() { OrderId = 2, UserId = "b", Status = "Confirmed" }
        };
        var dtos = new List<OrderDto>
        {
            new() { OrderId = 1, UserId = "a", Status = "Pending" },
            new() { OrderId = 2, UserId = "b", Status = "Confirmed" }
        };

        _uow.Orders.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _mapper.Setup(m => m.Map<List<OrderDto>>(orders)).Returns(dtos);

        var result = await _sut.GetAllOrdersAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateOrderAsync_AffectedRows_ReturnsTrue()
    {
        var dto = new OrderDto { OrderId = 1, UserId = "user-1", Status = "Completed" };
        var entity = new Order { OrderId = 1, UserId = "user-1", Status = "Completed" };

        _mapper.Setup(m => m.Map<Order>(dto)).Returns(entity);
        _uow.Orders.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);

        var result = await _sut.UpdateOrderAsync(dto);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOrderAsync_NoRowsAffected_ReturnsFalse()
    {
        var dto = new OrderDto { OrderId = 99, Status = "Completed" };
        var entity = new Order { OrderId = 99, Status = "Completed" };

        _mapper.Setup(m => m.Map<Order>(dto)).Returns(entity);
        _uow.Orders.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(0);

        var result = await _sut.UpdateOrderAsync(dto);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteOrderAsync_ExistingOrder_ReturnsTrue()
    {
        const int orderId = 3;
        var order = new Order { OrderId = orderId, Status = "Pending" };

        _uow.Orders.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        _uow.Orders.Setup(r => r.DeleteAsync(orderId)).ReturnsAsync(1);

        var result = await _sut.DeleteOrderAsync(orderId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteOrderAsync_NotFound_ReturnsFalse()
    {
        const int orderId = 999;
        _uow.Orders.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        var result = await _sut.DeleteOrderAsync(orderId);

        result.Should().BeFalse();
        _uow.UnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task ConfirmOrderAsync_ExistingOrder_SetsConfirmedStatus()
    {
        const int orderId = 5;
        var order = new Order { OrderId = orderId, Status = "Pending" };

        _uow.Orders.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        _uow.Orders.Setup(r => r.UpdateAsync(It.Is<Order>(o => o.Status == "Confirmed"))).ReturnsAsync(1);

        var result = await _sut.ConfirmOrderAsync(orderId);

        result.Should().BeTrue();
        order.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task ConfirmOrderAsync_NotFound_ReturnsFalse()
    {
        _uow.Orders.Setup(r => r.GetByIdAsync(100)).ReturnsAsync((Order?)null);

        var result = await _sut.ConfirmOrderAsync(100);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetMyOrdersAsync_ReturnsOrdersForUser()
    {
        const string userId = "user-42";
        var orders = new List<Order>
        {
            new() { OrderId = 1, UserId = userId, Status = "Pending" }
        };
        var dtos = new List<OrderDto>
        {
            new() { OrderId = 1, UserId = userId, Status = "Pending" }
        };

        _uow.Orders.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(orders);
        _mapper.Setup(m => m.Map<List<OrderDto>>(orders)).Returns(dtos);

        var result = await _sut.GetMyOrdersAsync(userId);

        result.Should().ContainSingle().Which.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetTakenSlotsAsync_ReturnsDistinctNonCancelledSlots()
    {
        var date = new DateTime(2026, 6, 1);
        var orders = new List<Order>
        {
            new() { OrderId = 1, OrderDate = date.AddHours(9), Status = "Confirmed" },
            new() { OrderId = 2, OrderDate = date.AddHours(9).AddMinutes(5), Status = "Pending" },
            new() { OrderId = 3, OrderDate = date.AddHours(11), Status = "Cancelled" },
            new() { OrderId = 4, OrderDate = date.AddHours(14), Status = "Confirmed" }
        };

        _uow.Orders.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

        var result = await _sut.GetTakenSlotsAsync(date);

        result.Should().BeEquivalentTo(["09:00", "09:05", "14:00"]);
    }

    [Fact]
    public async Task GetOrdersByDateAsync_ReturnsOnlyActiveOrdersForDate()
    {
        var date = new DateTime(2026, 7, 10);
        var orders = new List<Order>
        {
            new() { OrderId = 1, OrderDate = date.AddHours(8), Status = "Confirmed", UserId = "a" },
            new() { OrderId = 2, OrderDate = date.AddHours(10), Status = "Cancelled", UserId = "b" },
            new() { OrderId = 3, OrderDate = date.AddDays(1), Status = "Confirmed", UserId = "c" }
        };
        var expectedDtos = new List<OrderDto>
        {
            new() { OrderId = 1, UserId = "a", Status = "Confirmed" }
        };

        _uow.Orders.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _mapper.Setup(m => m.Map<List<OrderDto>>(It.Is<List<Order>>(l => l.Count == 1 && l[0].OrderId == 1)))
            .Returns(expectedDtos);

        var result = await _sut.GetOrdersByDateAsync(date);

        result.Should().HaveCount(1);
        result[0].OrderId.Should().Be(1);
    }
}
