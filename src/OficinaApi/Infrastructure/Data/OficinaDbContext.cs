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
    public DbSet<ServiceOrderStatus> ServiceOrderStatuses { get; set; }

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
            entity.Property(e => e.PersonType).HasConversion<string>().IsRequired();
            entity.HasMany(c => c.ServiceOrders)
                  .WithOne(so => so.Customer)
                  .HasForeignKey(so => so.CustomerId);
            entity.HasMany(c => c.Vehicles)
                  .WithOne(v => v.Customer)
                  .HasForeignKey(v => v.CustomerId);
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
            entity.HasOne(v => v.Customer)
                  .WithMany(c => c.Vehicles)
                  .HasForeignKey(v => v.CustomerId);
            entity.HasMany(v => v.ServiceOrders)
                  .WithOne(so => so.Vehicle)
                  .HasForeignKey(so => so.VehicleId);
        });

        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(14);
            entity.Property(e => e.VehicleId).IsRequired().HasMaxLength(14);
            entity.Property(e => e.Budget).HasColumnType("decimal(18,2)");
            entity.HasMany(so => so.StatusHistory)
                  .WithOne(sos => sos.ServiceOrder)
                  .HasForeignKey("ServiceOrderId")
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(so => so.ServicesUsed)
                .WithOne(s => s.ServiceOrder)
                .HasForeignKey(s => s.ServiceOrderId);
            entity.HasMany(so => so.PartsUsed)
                .WithOne(p => p.ServiceOrder)
                .HasForeignKey(p => p.ServiceOrderId);
            entity.HasOne(so => so.Customer)
                  .WithMany(c => c.ServiceOrders)
                  .HasForeignKey(so => so.CustomerId);
            entity.HasOne(so => so.Vehicle)
                  .WithMany(v => v.ServiceOrders)
                  .HasForeignKey(so => so.VehicleId);
            entity.HasMany(so => so.Alerts)
                .WithOne(a => a.ServiceOrder)
                .HasForeignKey(a => a.ServiceOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DefaultPrice).HasColumnType("decimal(18,2)").IsRequired();
        });

        modelBuilder.Entity<ServiceOrderStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone").IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().IsRequired();
            entity.HasOne(sos => sos.ServiceOrder)
                  .WithMany(so => so.StatusHistory)
                  .HasForeignKey(sos => sos.ServiceOrderId);
        });

        modelBuilder.Entity<ServiceOrderServices>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(sos => sos.ServiceOrder)
                  .WithMany(so => so.ServicesUsed)
                  .HasForeignKey(sos => sos.ServiceOrderId);
            entity.HasOne(sos => sos.Service)
                  .WithMany(s => s.ServiceOrders)
                  .HasForeignKey(sos => sos.ServiceId);
        });

        modelBuilder.Entity<ServiceOrderParts>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(sop => sop.ServiceOrder)
                  .WithMany(so => so.PartsUsed)
                  .HasForeignKey(sop => sop.ServiceOrderId);
            entity.HasOne(sop => sop.Part)
                  .WithMany(p => p.ServiceOrders)
                  .HasForeignKey(sop => sop.PartId);
            entity.Property(sop => sop.Quantity).IsRequired();
        });

        modelBuilder.Entity<ServiceOrderAlerts>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone").IsRequired();
            entity.HasOne(soa => soa.ServiceOrder)
                  .WithMany(so => so.Alerts)
                  .HasForeignKey(soa => soa.ServiceOrderId);
        });
    }
}
