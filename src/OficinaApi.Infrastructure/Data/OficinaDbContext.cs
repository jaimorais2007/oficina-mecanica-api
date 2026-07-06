using Microsoft.EntityFrameworkCore;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.ValueObjects;

namespace OficinaApi.Infrastructure.Data;

public class OficinaDbContext : DbContext
{

    public DbSet<Part> Parts { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ServiceOrderStatus> ServiceOrderStatuses { get; set; }
    public DbSet<ServiceOrder> ServiceOrders { get; set; }
    public DbSet<ServiceOrderService> ServiceOrderServices { get; set; }
    public DbSet<ServiceOrderPart> ServiceOrderParts { get; set; }
    private readonly IDomainEventDispatcher _domainEventDispatcher;


    public OficinaDbContext(
        DbContextOptions<OficinaDbContext> options,
        IDomainEventDispatcher domainEventDispatcher
        ) : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Inactive);
            entity.Ignore(e => e.DomainEvents);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Email).HasMaxLength(80);
            entity.Property(e => e.Inactive);
            entity.OwnsOne(e => e.Document, doc =>
            {
                doc.Property(d => d.Value)
                   .IsRequired()
                   .HasMaxLength(50);
            });
            entity.Property(e => e.DateOfBirth).HasColumnType("timestamp with time zone");
            entity.Property(e => e.PersonType).HasConversion<string>().IsRequired();
            entity.HasMany(c => c.ServiceOrders)
                  .WithOne(so => so.Customer)
                  .HasForeignKey(so => so.CustomerId);
            entity.HasMany(c => c.Vehicles)
                  .WithOne(v => v.Customer)
                  .HasForeignKey(v => v.CustomerId);
            entity.Ignore(e => e.DomainEvents);

        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Brand).IsRequired().HasMaxLength(80);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(80);
            entity.Property(e => e.Inactive);
            entity.Property(e => e.Year).IsRequired();
            entity.OwnsOne(e => e.Plate, plate =>
            {
                plate.Property(e => e.Value)
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
            entity.Ignore(e => e.DomainEvents);
        });

        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(14);
            entity.Property(e => e.VehicleId).IsRequired().HasMaxLength(14);
            entity.Property(e => e.Inactive);
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
            entity.Ignore(e => e.DomainEvents);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Inactive);
            entity.Property(e => e.DefaultPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Ignore(e => e.DomainEvents);
        });

        modelBuilder.Entity<ServiceOrderStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().IsRequired();
            entity.Property(e => e.Inactive);
            entity.HasOne(sos => sos.ServiceOrder)
                  .WithMany(so => so.StatusHistory)
                  .HasForeignKey(sos => sos.ServiceOrderId);
            entity.Ignore(e => e.DomainEvents);
        });

        modelBuilder.Entity<ServiceOrderService>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Inactive);
            entity.HasOne(sos => sos.ServiceOrder)
                  .WithMany(so => so.ServicesUsed)
                  .HasForeignKey(sos => sos.ServiceOrderId);
            entity.HasOne(sos => sos.Service)
                  .WithMany(s => s.ServiceOrdersServices)
                  .HasForeignKey(sos => sos.ServiceId);
            entity.Ignore(e => e.DomainEvents);
        });

        modelBuilder.Entity<ServiceOrderPart>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Inactive);
            entity.HasOne(sop => sop.ServiceOrder)
                  .WithMany(so => so.PartsUsed)
                  .HasForeignKey(sop => sop.ServiceOrderId);
            entity.HasOne(sop => sop.Part)
                  .WithMany(p => p.ServiceOrdersParts)
                  .HasForeignKey(sop => sop.PartId);
            entity.Property(sop => sop.Quantity).IsRequired();
            entity.Property(so => so.StockQuantityWasEnsured).IsRequired();
            entity.Ignore(e => e.DomainEvents);
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50);
        });
    }

    private List<BaseEntity> CollectEntitiesWithEvents()
    {
        return ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents != null && e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();
    }

    public override int SaveChanges()
    {
        var entitiesWithEvents = CollectEntitiesWithEvents();
        var result = base.SaveChanges();
        _ = DispatchEventsAsync(entitiesWithEvents);
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = CollectEntitiesWithEvents();
        var result = await base.SaveChangesAsync(cancellationToken);
        _ = DispatchEventsAsync(entitiesWithEvents, cancellationToken);
        return result;
    }

    private async Task DispatchEventsAsync(List<BaseEntity> entitiesWithEvents, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToArray();
            entity.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await _domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);
            }
        }
    }
}
