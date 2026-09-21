namespace AutoserviceVehicle.BLL.DTO;

public sealed record VehicleDto(
    Guid Id, string Vin, string Make, string Model, int Year,
    int? MileageKm, string? LicensePlate, string? Color,
    DateTime CreatedAt, DateTime UpdatedAt);
