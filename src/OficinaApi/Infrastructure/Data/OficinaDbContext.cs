using Microsoft.EntityFrameworkCore;
using OficinaApi.Domain.Entities;

namespace OficinaApi.Infrastructure.Data;

public class OficinaDbContext : DbContext
{
    public OficinaDbContext(DbContextOptions<OficinaDbContext> options) : base(options) { }

    public DbSet<Part> Parts { get; set; }
    public DbSet<ServiceOrder> ServiceOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientCpf).IsRequired().HasMaxLength(14);
            entity.Property(e => e.VehiclePlate).IsRequired().HasMaxLength(10);
            // Storing enum as string for better readability in DB
            entity.Property(e => e.Status).HasConversion<string>();
        });
    }
}
