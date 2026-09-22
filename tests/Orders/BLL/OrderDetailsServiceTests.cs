using AutoserviceOrders.BLL.DTO;
using AutoserviceOrders.BLL.Services;
using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.Tests.Common;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoserviceOrders.Tests.BLL;

public class OrderDetailsServiceTests
{
    private readonly UnitOfWorkMockBuilder _uow = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly OrderDetailsService _sut;

    public OrderDetailsServiceTests()
    {
        _sut = new OrderDetailsService(_uow.UnitOfWork.Object, _mapper.Object);
    }

    [Fact]
    public async Task AddOrderDetailsAsync_ValidDto_PersistsEntity()
    {
        var dto = new OrderDetailsDto
        {
            OrderId = 1,
            MechanicName = "Ivan",
            EstimatedCompletionDate = new DateTime(2026, 5, 25)
        };
        var entity = new OrderDetails
        {
            OrderId = 1,
            MechanicName = "Ivan",
            EstimatedCompletionDate = dto.EstimatedCompletionDate
        };

        _mapper.Setup(m => m.Map<OrderDetails>(dto)).Returns(entity);
        _uow.OrderDetails.Setup(r => r.AddAsync(entity)).ReturnsAsync(1);

        await _sut.AddOrderDetailsAsync(dto);

        _uow.OrderDetails.Verify(r => r.AddAsync(entity), Times.Once);
        _uow.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderDetailsAsync_ValidDto_UpdatesEntity()
    {
        var dto = new OrderDetailsDto
        {
            OrderId = 2,
            MechanicName = "Petro",
            EstimatedCompletionDate = new DateTime(2026, 6, 1)
        };
        var entity = new OrderDetails
        {
            OrderId = 2,
            MechanicName = "Petro",
            EstimatedCompletionDate = dto.EstimatedCompletionDate
        };

        _mapper.Setup(m => m.Map<OrderDetails>(dto)).Returns(entity);
        _uow.OrderDetails.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);

        await _sut.UpdateOrderDetailsAsync(dto);

        _uow.OrderDetails.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderDetailsAsync_DeletesByOrderId()
    {
        const int orderId = 3;
        _uow.OrderDetails.Setup(r => r.DeleteAsync(orderId)).ReturnsAsync(1);

        await _sut.DeleteOrderDetailsAsync(orderId);

        _uow.OrderDetails.Verify(r => r.DeleteAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task GetByOrderIdAsync_ReturnsMappedDto()
    {
        const int orderId = 4;
        var entity = new OrderDetails
        {
            OrderId = orderId,
            MechanicName = "Olena",
            EstimatedCompletionDate = new DateTime(2026, 5, 30)
        };
        var dto = new OrderDetailsDto
        {
            OrderId = orderId,
            MechanicName = "Olena",
            EstimatedCompletionDate = entity.EstimatedCompletionDate
        };

        _uow.OrderDetails.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<OrderDetailsDto>(entity)).Returns(dto);

        var result = await _sut.GetByOrderIdAsync(orderId);

        result.OrderId.Should().Be(orderId);
        result.MechanicName.Should().Be("Olena");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMappedDtos()
    {
        var entities = new List<OrderDetails>
        {
            new() { OrderId = 1, MechanicName = "A", EstimatedCompletionDate = DateTime.UtcNow },
            new() { OrderId = 2, MechanicName = "B", EstimatedCompletionDate = DateTime.UtcNow.AddDays(1) }
        };
        var dtos = new List<OrderDetailsDto>
        {
            new() { OrderId = 1, MechanicName = "A" },
            new() { OrderId = 2, MechanicName = "B" }
        };

        _uow.OrderDetails.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);
        _mapper.Setup(m => m.Map<IEnumerable<OrderDetailsDto>>(entities)).Returns(dtos);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }
}
