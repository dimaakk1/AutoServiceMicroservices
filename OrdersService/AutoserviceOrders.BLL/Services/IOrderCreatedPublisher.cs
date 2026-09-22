using AutoserviceOrders.BLL.DTO;

namespace AutoserviceOrders.BLL.Services;

public interface IOrderCreatedPublisher
{
    Task PublishOrderCreatedAsync(OrderCreatedEvent order);
}
