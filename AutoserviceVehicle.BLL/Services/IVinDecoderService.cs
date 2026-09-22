using AutoserviceVehicle.BLL.DTO;

namespace AutoserviceVehicle.BLL.Services;

public interface IVinDecoderService
{
    Task<VinDecodeDto> DecodeAsync(string vin, CancellationToken cancellationToken = default);
}
