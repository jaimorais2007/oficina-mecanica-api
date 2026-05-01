using Microsoft.EntityFrameworkCore;
using OficinaApi.Domain.Entities;

namespace OficinaApi.Infrastructure.Data;

public class OficinaDbContext : DbContext
{
    public OficinaDbContext(DbContextOptions<OficinaDbContext> options) : base(options) { }

    public DbSet<Part> Parts { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Service> Services { get; set; }

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

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.OwnsOne(e => e.Document, doc =>
            {
                doc.Property(d => d.Value)
                   .IsRequired()
                   .HasMaxLength(50);
            });
            entity.Property(e => e.DateOfBirth).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Brand).IsRequired().HasMaxLength(80);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(80);
            entity.Property(e => e.Year).IsRequired();
            entity.OwnsOne(e => e.Plate, plate =>
            {
                plate.Property(p => p.Value)
                     .HasColumnName("Plate")
                     .IsRequired()
                     .HasMaxLength(10);
            });
        });

        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientCpf).IsRequired().HasMaxLength(14);
            entity.Property(e => e.VehiclePlate).IsRequired().HasMaxLength(10);
            // Storing enum as string for better readability in DB
            entity.Property(e => e.Status).HasConversion<string>();
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DefaultPrice).HasColumnType("decimal(10,2)").IsRequired();
        });
    }
}
