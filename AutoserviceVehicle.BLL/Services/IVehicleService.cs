using AutoserviceVehicle.BLL.DTO;

namespace AutoserviceVehicle.BLL.Services;

public interface IVehicleService
{
    Task<IReadOnlyList<VehicleDto>> GetAllAsync(string userId, CancellationToken cancellationToken = default);
    Task<VehicleDto?> GetAsync(string userId, Guid id, CancellationToken cancellationToken = default);
    Task<VehicleDto> CreateAsync(string userId, SaveVehicleDto request, CancellationToken cancellationToken = default);
    Task<VehicleDto?> UpdateAsync(string userId, Guid id, SaveVehicleDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string userId, Guid id, CancellationToken cancellationToken = default);
}
