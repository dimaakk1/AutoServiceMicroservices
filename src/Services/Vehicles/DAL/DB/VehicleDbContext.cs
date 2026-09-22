using AutoserviceVehicle.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoserviceVehicle.DAL.DB;

public sealed class VehicleDbContext(DbContextOptions<VehicleDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var vehicle = modelBuilder.Entity<Vehicle>();
        vehicle.ToTable("Vehicles", table =>
        {
            table.HasCheckConstraint("CK_Vehicles_Year", "[Year] >= 1886 AND [Year] <= 2100");
            table.HasCheckConstraint("CK_Vehicles_MileageKm", "[MileageKm] IS NULL OR [MileageKm] >= 0");
        });
        vehicle.HasKey(x => x.Id);
        vehicle.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        vehicle.Property(x => x.Vin).HasMaxLength(17).IsRequired();
        vehicle.Property(x => x.Make).HasMaxLength(100).IsRequired();
        vehicle.Property(x => x.Model).HasMaxLength(100).IsRequired();
        vehicle.Property(x => x.LicensePlate).HasMaxLength(20);
        vehicle.Property(x => x.Color).HasMaxLength(50);
        // Users belong to UsersService; there is no cross-database foreign key.
        vehicle.HasIndex(x => new { x.UserId, x.Vin }).IsUnique();
    }
}
