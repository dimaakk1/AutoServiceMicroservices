using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoserviceVehicle.BLL.DTO;
using AutoserviceVehicle.BLL.Validation;

namespace AutoserviceVehicle.BLL.Services;

public sealed class NhtsaVinDecoderService(HttpClient httpClient) : IVinDecoderService
{
    public async Task<VinDecodeDto> DecodeAsync(string vin, CancellationToken cancellationToken = default)
    {
        var normalizedVin = VinValidator.NormalizeAndValidate(vin);

        try
        {
            var response = await httpClient.GetAsync(
                $"vehicles/DecodeVinValuesExtended/{Uri.EscapeDataString(normalizedVin)}?format=json",
                cancellationToken);
            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<NhtsaResponse>(cancellationToken: cancellationToken);
            var result = payload?.Results?.FirstOrDefault();
            if (result is null || (string.IsNullOrWhiteSpace(result.Make) && string.IsNullOrWhiteSpace(result.Manufacturer)))
                throw new VinNotFoundException();

            var warnings = new List<string>();
            var cleanDecode = result.ErrorCode?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .All(code => code == "0") ?? true;
            if (!cleanDecode)
                warnings.Add("NHTSA не надала всі можливі характеристики для цього VIN.");
            if (string.IsNullOrWhiteSpace(result.Make))
                warnings.Add("Марку не визначено — перевірте її у документах на авто.");
            if (string.IsNullOrWhiteSpace(result.Model))
                warnings.Add("Модель не визначено — перевірте її у документах на авто.");
            if (string.IsNullOrWhiteSpace(result.ModelYear))
                warnings.Add("Рік випуску не визначено — перевірте його у документах на авто.");

            return new VinDecodeDto(
                normalizedVin, Text(result.Make), Text(result.Model), Int(result.ModelYear),
                Text(result.Manufacturer), Text(result.VehicleType), Text(result.BodyClass), Int(result.Doors),
                Text(result.FuelTypePrimary), Decimal(result.DisplacementL), Int(result.EngineCylinders),
                Text(result.EngineModel), Text(result.TransmissionStyle), Int(result.TransmissionSpeeds),
                Text(result.DriveType), Text(result.Series), Text(result.Trim), Text(result.PlantCountry),
                Text(result.PlantCity), Text(result.PlantCompanyName), cleanDecode && warnings.Count == 0, warnings);
        }
        catch (VinNotFoundException)
        {
            throw;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new VinProviderUnavailableException();
        }
        catch (HttpRequestException ex)
        {
            throw new VinProviderUnavailableException(ex);
        }
        catch (JsonException ex)
        {
            throw new VinProviderUnavailableException(ex);
        }
    }

    private static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static int? Int(string? value) => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    private static decimal? Decimal(string? value) => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private sealed class NhtsaResponse
    {
        public List<NhtsaResult>? Results { get; init; }
    }

    private sealed class NhtsaResult
    {
        public string? Make { get; init; }
        public string? Model { get; init; }
        public string? ModelYear { get; init; }
        public string? Manufacturer { get; init; }
        public string? VehicleType { get; init; }
        public string? BodyClass { get; init; }
        public string? Doors { get; init; }
        public string? FuelTypePrimary { get; init; }
        public string? DisplacementL { get; init; }
        public string? EngineCylinders { get; init; }
        public string? EngineModel { get; init; }
        public string? TransmissionStyle { get; init; }
        public string? TransmissionSpeeds { get; init; }
        public string? DriveType { get; init; }
        public string? Series { get; init; }
        public string? Trim { get; init; }
        public string? PlantCountry { get; init; }
        public string? PlantCity { get; init; }
        public string? PlantCompanyName { get; init; }
        public string? ErrorCode { get; init; }
    }
}
