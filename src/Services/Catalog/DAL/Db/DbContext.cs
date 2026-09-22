using AutoServiceCatalog.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Db
{
    public class CarServiceContext : DbContext
    {
        public CarServiceContext(DbContextOptions<CarServiceContext> options) : base(options) { }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceDetail> ServiceDetails { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ServiceSupplier> ServiceSupplier { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Service>()
                .HasOne(s => s.ServiceDetail)
                .WithOne(d => d.Service)
                .HasForeignKey<ServiceDetail>(d => d.ServiceId);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Services)
                .WithOne(s => s.Category)
                .HasForeignKey(s => s.CategoryId);

            modelBuilder.Entity<ServiceSupplier>()
                .HasKey(ss => new { ss.ServiceId, ss.SupplierId });

            modelBuilder.Entity<ServiceSupplier>()
                .HasOne(ss => ss.Service)
                .WithMany(s => s.ServiceSuppliers)
                .HasForeignKey(ss => ss.ServiceId);

            modelBuilder.Entity<ServiceSupplier>()
                .HasOne(ss => ss.Supplier)
                .WithMany(sup => sup.ServiceSuppliers)
                .HasForeignKey(ss => ss.SupplierId);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Service>()
                .HasIndex(s => s.Name);
        }
    }
}
