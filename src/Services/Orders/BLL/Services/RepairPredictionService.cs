using System.Globalization;
using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.DAL.Repositories.Interfaces;
using AutoserviceOrders.DAL.UnitOfWork;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RepairPredictionGrpc;
using VehicleGrpc;

namespace AutoserviceOrders.BLL.Services;

public sealed record RepairForecast(double EstimatedDurationMinutes, double? DelayProbability);

public sealed class RepairPredictionService(
    IUnitOfWork unitOfWork,
    VehicleRegistry.VehicleRegistryClient vehicles,
    ILogger<RepairPredictionService> logger,
    RepairPrediction.RepairPredictionClient? prediction = null)
{
    private static readonly TimeSpan RpcTimeout = TimeSpan.FromSeconds(2);

    // The caller must load the order and check access before requesting a forecast.
    public async Task<RepairForecast?> TryPredictAsync(Order order, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (prediction is null || order.VehicleId is not { } vehicleId ||
            string.IsNullOrWhiteSpace(order.MechanicId) || string.IsNullOrWhiteSpace(order.UserId))
            return null;

        var items = (await unitOfWork.GetRepository<IOrderItemRepository>()
            .GetItemsByOrderIdAsync(order.OrderId)).ToArray();
        if (items.Length == 0 || items.Any(item => item.ProductId <= 0 || item.Quantity <= 0))
            return null;

        try
        {
            var vehicle = await vehicles.GetOwnedVehicleAsync(new GetOwnedVehicleRequest
            {
                VehicleId = vehicleId.ToString(),
                UserId = order.UserId
            }, deadline: DateTime.UtcNow.Add(RpcTimeout), cancellationToken: cancellationToken);

            if (!vehicle.HasMileageKm || vehicle.MileageKm < 0 ||
                vehicle.Year < 1886 || vehicle.Year > order.OrderDate.Year)
                return null;

            var request = new PredictRepairRequest
            {
                AppointmentLocal = order.OrderDate.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
                MechanicId = order.MechanicId,
                VehicleYear = vehicle.Year,
                MileageKm = vehicle.MileageKm,
                ServiceCount = items.Sum(item => item.Quantity)
            };
            request.ServiceIds.AddRange(items.Select(item => item.ProductId).Distinct());

            if (order.PlannedCompletionAt is { } planned)
            {
                // Match the training export: local appointment in the workshop's time zone,
                // planned completion stored in UTC (including across a DST transition).
                var shopTimeZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
                var localAppointment = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Unspecified);
                if (!shopTimeZone.IsInvalidTime(localAppointment))
                {
                    var appointmentUtc = TimeZoneInfo.ConvertTimeToUtc(localAppointment, shopTimeZone);
                    var budget = (planned - appointmentUtc).TotalMinutes;
                    if (budget > 0)
                        request.PlannedBudgetMinutes = budget;
                }
            }

            var reply = await prediction.PredictRepairAsync(request,
                deadline: DateTime.UtcNow.Add(RpcTimeout), cancellationToken: cancellationToken);
            return new RepairForecast(reply.EstimatedDurationMinutes,
                reply.HasDelayProbability ? reply.DelayProbability : null);
        }
        catch (RpcException exception)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogDebug("Repair prediction unavailable for order {OrderId}: {StatusCode}",
                order.OrderId, exception.StatusCode);
            return null;
        }
    }
}
