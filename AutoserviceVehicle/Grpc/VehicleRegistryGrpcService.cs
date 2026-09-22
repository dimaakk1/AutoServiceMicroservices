using AutoserviceVehicle.BLL.Services;
using Grpc.Core;
using VehicleGrpc;

namespace AutoserviceVehicle.API.Grpc;

public sealed class VehicleRegistryGrpcService(IVehicleService vehicleService)
    : VehicleRegistry.VehicleRegistryBase
{
    public override async Task<VehicleBookingReply> GetOwnedVehicle(
        GetOwnedVehicleRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.VehicleId, out var vehicleId) || string.IsNullOrWhiteSpace(request.UserId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Некоректні дані автомобіля."));

        var vehicle = await vehicleService.GetAsync(request.UserId, vehicleId, context.CancellationToken);
        if (vehicle is null)
            throw new RpcException(new Status(StatusCode.NotFound, "Автомобіль не знайдено у гаражі користувача."));

        return new VehicleBookingReply
        {
            VehicleId = vehicle.Id.ToString(),
            Vin = vehicle.Vin,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            LicensePlate = vehicle.LicensePlate ?? string.Empty
        };
    }
}
