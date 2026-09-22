using AutoserviceOrders.DAL.Repositories.Interfaces;
using AutoserviceOrders.DAL.UnitOfWork;
using Moq;

namespace AutoserviceOrders.Tests.Common;

internal sealed class UnitOfWorkMockBuilder
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IOrderItemRepository> _orderItems = new();
    private readonly Mock<IOrderDetailsRepository> _orderDetails = new();

    public UnitOfWorkMockBuilder()
    {
        _unitOfWork.Setup(u => u.Orders).Returns(_orders.Object);
        _unitOfWork.Setup(u => u.OrderItems).Returns(_orderItems.Object);
        _unitOfWork.Setup(u => u.OrderDetails).Returns(_orderDetails.Object);
        _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
    }

    public Mock<IOrderRepository> Orders => _orders;
    public Mock<IOrderItemRepository> OrderItems => _orderItems;
    public Mock<IOrderDetailsRepository> OrderDetails => _orderDetails;
    public Mock<IUnitOfWork> UnitOfWork => _unitOfWork;
}
