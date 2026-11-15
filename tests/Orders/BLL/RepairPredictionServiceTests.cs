using AutoserviceOrders.BLL.Services;
using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.DAL.Repositories.Interfaces;
using AutoserviceOrders.DAL.UnitOfWork;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RepairPredictionGrpc;
using VehicleGrpc;
using Xunit;

namespace AutoserviceOrders.Tests.BLL;

public sealed class RepairPredictionServiceTests
{
    private readonly Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
    private readonly Mock<VehicleRegistry.VehicleRegistryClient> vehicles = new(MockBehavior.Strict);
    private readonly Mock<RepairPrediction.RepairPredictionClient> prediction = new(MockBehavior.Strict);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MapsOrderInputsAndPreservesOptionalRisk(bool hasPlan)
    {
        var order = PrepareOrder(new VehicleBookingReply { Year = 2018, MileageKm = 0 });
        if (hasPlan)
            order.PlannedCompletionAt = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc);
        var response = new PredictRepairReply { EstimatedDurationMinutes = 94 };
        if (hasPlan)
            response.DelayProbability = 0;

        prediction.Setup(client => client.PredictRepairAsync(It.Is<PredictRepairRequest>(request =>
                request.AppointmentLocal == "2026-10-01T10:00:00" && request.MechanicId == order.MechanicId &&
                request.VehicleYear == 2018 && request.HasMileageKm && request.MileageKm == 0 &&
                request.ServiceIds.SequenceEqual(new[] { 3, 7 }) && request.ServiceCount == 4 &&
                request.HasPlannedBudgetMinutes == hasPlan && (!hasPlan || request.PlannedBudgetMinutes == 120)),
                null, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(Call(Task.FromResult(response)));

        var result = Assert.IsType<RepairForecast>(await CreateService().TryPredictAsync(order));
        Assert.Equal(94, result.EstimatedDurationMinutes);
        Assert.Equal(hasPlan ? 0d : (double?)null, result.DelayProbability);
    }

    [Fact]
    public async Task MissingMileageSkipsPrediction()
    {
        var order = PrepareOrder(new VehicleBookingReply { Year = 2018 });
        Assert.Null(await CreateService().TryPredictAsync(order));
        prediction.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UnavailablePredictionReturnsNoForecast()
    {
        var order = PrepareOrder(new VehicleBookingReply { Year = 2018, MileageKm = 80000 });
        prediction.Setup(client => client.PredictRepairAsync(It.IsAny<PredictRepairRequest>(),
                null, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(Call(Task.FromException<PredictRepairReply>(
                new RpcException(new Status(StatusCode.Unavailable, "Prediction unavailable")))));

        Assert.Null(await CreateService().TryPredictAsync(order));
    }

    [Fact]
    public async Task DisabledPredictionSkipsAllDependencies()
    {
        var service = new RepairPredictionService(unitOfWork.Object, vehicles.Object,
            NullLogger<RepairPredictionService>.Instance);
        Assert.Null(await service.TryPredictAsync(new Order { VehicleId = Guid.NewGuid(), MechanicId = "m", UserId = "u" }));
    }

    private RepairPredictionService CreateService() =>
        new(unitOfWork.Object, vehicles.Object, NullLogger<RepairPredictionService>.Instance, prediction.Object);

    private Order PrepareOrder(VehicleBookingReply vehicle)
    {
        var order = new Order
        {
            OrderId = 10, OrderDate = new DateTime(2026, 10, 1, 10, 0, 0),
            VehicleId = Guid.NewGuid(), UserId = "customer", MechanicId = "mechanic"
        };
        var items = new Mock<IOrderItemRepository>(MockBehavior.Strict);
        items.Setup(repo => repo.GetItemsByOrderIdAsync(order.OrderId)).ReturnsAsync(new[]
        {
            new OrderItem { ProductId = 3, Quantity = 1 }, new OrderItem { ProductId = 3, Quantity = 2 },
            new OrderItem { ProductId = 7, Quantity = 1 }
        });
        unitOfWork.Setup(uow => uow.GetRepository<IOrderItemRepository>()).Returns(items.Object);
        vehicles.Setup(client => client.GetOwnedVehicleAsync(It.Is<GetOwnedVehicleRequest>(request =>
                request.VehicleId == order.VehicleId.ToString() && request.UserId == order.UserId),
                null, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(Call(Task.FromResult(vehicle)));
        return order;
    }

    private static AsyncUnaryCall<T> Call<T>(Task<T> response) =>
        new(response, Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
}
