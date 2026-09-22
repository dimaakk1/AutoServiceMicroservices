using AutoserviceVehicle.DAL.DB;
using AutoserviceVehicle.DAL.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AutoserviceVehicle.DAL.Repositories;

public sealed class VehicleRepository(VehicleDbContext db) : IVehicleRepository
{
    public async Task<IReadOnlyList<Vehicle>> GetAllAsync(string userId, CancellationToken cancellationToken) =>
        await db.Vehicles.AsNoTracking().Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id).ToListAsync(cancellationToken);

    public Task<Vehicle?> GetAsync(string userId, Guid id, CancellationToken cancellationToken) =>
        db.Vehicles.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

    public Task<bool> VinExistsAsync(string userId, string vin, Guid? exceptId, CancellationToken cancellationToken) =>
        db.Vehicles.AnyAsync(x => x.UserId == userId && x.Vin == vin && x.Id != exceptId, cancellationToken);

    public void Add(Vehicle vehicle) => db.Vehicles.Add(vehicle);
    public void Remove(Vehicle vehicle) => db.Vehicles.Remove(vehicle);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // The unique index also protects against simultaneous requests.
            throw new DuplicateVehicleException();
        }
    }
}
