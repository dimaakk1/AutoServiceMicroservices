using System.ComponentModel.DataAnnotations;

namespace AutoserviceVehicle.BLL.DTO;

public sealed class SaveVehicleDto : IValidatableObject
{
    [Required, StringLength(17)]
    public string Vin { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string Make { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string Model { get; set; } = string.Empty;
    [Range(1886, 2100)]
    public int Year { get; set; }
    [Range(0, int.MaxValue)]
    public int? MileageKm { get; set; }
    [StringLength(20)]
    public string? LicensePlate { get; set; }
    [StringLength(50)]
    public string? Color { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var vin = Vin?.Trim().ToUpperInvariant() ?? string.Empty;
        if (vin.Length != 17 || vin.Any(c => !(c is >= 'A' and <= 'Z' or >= '0' and <= '9') || c is 'I' or 'O' or 'Q'))
            yield return new ValidationResult("VIN має містити 17 латинських літер або цифр, без I, O та Q.", [nameof(Vin)]);
        if (Year > DateTime.UtcNow.Year + 1)
            yield return new ValidationResult("Рік авто не може бути більшим за наступний календарний рік.", [nameof(Year)]);
    }
}
