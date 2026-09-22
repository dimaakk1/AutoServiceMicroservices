using AutoserviceVehicle.DAL.Entities;

namespace AutoserviceVehicle.DAL.Repositories;

public interface IVehicleRepository
{
    Task<IReadOnlyList<Vehicle>> GetAllAsync(string userId, CancellationToken cancellationToken);
    Task<Vehicle?> GetAsync(string userId, Guid id, CancellationToken cancellationToken);
    Task<bool> VinExistsAsync(string userId, string vin, Guid? exceptId, CancellationToken cancellationToken);
    void Add(Vehicle vehicle);
    void Remove(Vehicle vehicle);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
