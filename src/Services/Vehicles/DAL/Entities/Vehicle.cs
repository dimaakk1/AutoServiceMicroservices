namespace AutoserviceVehicle.DAL.Entities;

public sealed class Vehicle
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int? MileageKm { get; set; }
    public string? LicensePlate { get; set; }
    public string? Color { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
