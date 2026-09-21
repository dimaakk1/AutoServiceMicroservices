using System.ComponentModel.DataAnnotations;
using AutoserviceVehicle.BLL.DTO;
using AutoserviceVehicle.DAL.Entities;
using AutoserviceVehicle.DAL.Repositories;

namespace AutoserviceVehicle.BLL.Services;

public sealed class VehicleService(IVehicleRepository repository) : IVehicleService
{
    public async Task<IReadOnlyList<VehicleDto>> GetAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        ValidateOwner(userId);
        return (await repository.GetAllAsync(userId, cancellationToken)).Select(Map).ToList();
    }

    public async Task<VehicleDto?> GetAsync(string userId, Guid id, CancellationToken cancellationToken = default)
    {
        ValidateOwner(userId);
        var vehicle = await repository.GetAsync(userId, id, cancellationToken);
        return vehicle is null ? null : Map(vehicle);
    }

    public async Task<VehicleDto> CreateAsync(string userId, SaveVehicleDto request, CancellationToken cancellationToken = default)
    {
        ValidateOwner(userId);
        NormalizeAndValidate(request);
        if (await repository.VinExistsAsync(userId, request.Vin, null, cancellationToken))
            throw new DuplicateVehicleException();
        var vehicle = new Vehicle { Id = Guid.NewGuid(), UserId = userId, CreatedAt = DateTime.UtcNow };
        Apply(vehicle, request);
        repository.Add(vehicle);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(vehicle);
    }

    public async Task<VehicleDto?> UpdateAsync(string userId, Guid id, SaveVehicleDto request, CancellationToken cancellationToken = default)
    {
        ValidateOwner(userId);
        var vehicle = await repository.GetAsync(userId, id, cancellationToken);
        if (vehicle is null) return null;
        NormalizeAndValidate(request);
        if (await repository.VinExistsAsync(userId, request.Vin, id, cancellationToken))
            throw new DuplicateVehicleException();
        Apply(vehicle, request);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(vehicle);
    }

    public async Task<bool> DeleteAsync(string userId, Guid id, CancellationToken cancellationToken = default)
    {
        ValidateOwner(userId);
        var vehicle = await repository.GetAsync(userId, id, cancellationToken);
        if (vehicle is null) return false;
        repository.Remove(vehicle);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void ValidateOwner(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId) || userId.Length > 450)
            throw new UnauthorizedAccessException("Потрібна авторизація користувача.");
    }

    private static void NormalizeAndValidate(SaveVehicleDto request)
    {
        request.Vin = request.Vin?.Trim().ToUpperInvariant() ?? string.Empty;
        request.Make = request.Make?.Trim() ?? string.Empty;
        request.Model = request.Model?.Trim() ?? string.Empty;
        request.LicensePlate = OptionalText(request.LicensePlate)?.ToUpperInvariant();
        request.Color = OptionalText(request.Color);
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);
    }

    private static string? OptionalText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Apply(Vehicle vehicle, SaveVehicleDto request)
    {
        vehicle.Vin = request.Vin;
        vehicle.Make = request.Make;
        vehicle.Model = request.Model;
        vehicle.Year = request.Year;
        vehicle.MileageKm = request.MileageKm;
        vehicle.LicensePlate = request.LicensePlate;
        vehicle.Color = request.Color;
        vehicle.UpdatedAt = DateTime.UtcNow;
    }

    private static VehicleDto Map(Vehicle vehicle) => new(vehicle.Id, vehicle.Vin, vehicle.Make,
        vehicle.Model, vehicle.Year, vehicle.MileageKm, vehicle.LicensePlate, vehicle.Color,
        DateTime.SpecifyKind(vehicle.CreatedAt, DateTimeKind.Utc),
        DateTime.SpecifyKind(vehicle.UpdatedAt, DateTimeKind.Utc));
}
