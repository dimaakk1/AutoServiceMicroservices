using System.ComponentModel.DataAnnotations;

namespace AutoserviceVehicle.BLL.Validation;

public static class VinValidator
{
    public const string ErrorMessage = "VIN має містити 17 латинських літер або цифр, без I, O та Q.";

    public static string Normalize(string? vin) => vin?.Trim().ToUpperInvariant() ?? string.Empty;

    public static bool IsValid(string? vin)
    {
        var normalized = Normalize(vin);
        return normalized.Length == 17 && normalized.All(c =>
            ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')) &&
            c != 'I' && c != 'O' && c != 'Q');
    }

    public static string NormalizeAndValidate(string? vin)
    {
        var normalized = Normalize(vin);
        if (!IsValid(normalized)) throw new ValidationException(ErrorMessage);
        return normalized;
    }
}
