using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Infrastructure.Data;
using OficinaApi.Infrastructure.Repositories;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Unit.Tests;

public class ServiceOrderRepositoryTests
{
    private DbTestContext CreateDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseSqlite(connection)
            .Options;

        var dispatcher = new Mock<IDomainEventDispatcher>();
        var context = new OficinaDbContext(options, dispatcher.Object);
        context.Database.EnsureCreated();
        return new DbTestContext(context, connection);
    }

    private sealed class DbTestContext(OficinaDbContext context, SqliteConnection connection) : IDisposable
    {
        public OficinaDbContext Context { get; } = context;
        public SqliteConnection Connection { get; } = connection;

        public void Deconstruct(out OficinaDbContext ctx, out SqliteConnection conn)
            => (ctx, conn) = (Context, Connection);

        public void Dispose()
        {
            Context.Dispose();
            Connection.Dispose();
        }
    }

    private Customer CreateCustomer()
        => new("João Silva", PersonType.Individual, "529.982.247-25", new DateTime(1990, 1, 1), "teste@gmail.com");

    private Vehicle CreateVehicle(Customer customer)
        => new(customer, "ABC1234", "Toyota", "Corolla", 2020);

    private Service CreateService()
        => new("Troca de óleo", "Substituição do óleo mineral", 150.00m);

    private Part CreatePart()
        => new("Filtro de óleo", "FO-001", 10, 50.00m);

    private ServiceOrder CreateServiceOrder(Customer customer, Vehicle vehicle, Service service)
        => new(customer, vehicle, new[] { service });

    private async Task SeedBaseEntitiesAsync(OficinaDbContext context, Customer customer, Vehicle vehicle, Service service)
    {
        context.Customers.Add(customer);
        context.Vehicles.Add(vehicle);
        context.Services.Add(service);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoDatabaseRecords()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = await sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllServiceOrders_WhenRecordsExist()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        await SeedBaseEntitiesAsync(context, customer, vehicle, service);

        context.ServiceOrders.Add(CreateServiceOrder(customer, vehicle, service));
        context.ServiceOrders.Add(CreateServiceOrder(customer, vehicle, service));
        await context.SaveChangesAsync();

        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = await sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenServiceOrderDoesNotExist()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = await sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnServiceOrderWithRelations_WhenItExists()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        var serviceOrder = CreateServiceOrder(customer, vehicle, service);
        await SeedBaseEntitiesAsync(context, customer, vehicle, service);
        context.ServiceOrders.Add(serviceOrder);
        await context.SaveChangesAsync();

        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = await sut.GetByIdAsync(serviceOrder.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(serviceOrder.Id);
        result.Customer.Should().NotBeNull();
        result.Vehicle.Should().NotBeNull();
        result.StatusHistory.Should().NotBeEmpty();
        result.ServicesUsed.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistServiceOrder_InDatabase()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        await SeedBaseEntitiesAsync(context, customer, vehicle, service);

        var serviceOrder = CreateServiceOrder(customer, vehicle, service);
        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        await sut.AddAsync(serviceOrder);

        // Assert
        var persisted = await context.ServiceOrders.FindAsync(serviceOrder.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistServiceOrderWithReceivedStatus()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        await SeedBaseEntitiesAsync(context, customer, vehicle, service);

        var serviceOrder = CreateServiceOrder(customer, vehicle, service);
        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        await sut.AddAsync(serviceOrder);

        // Assert
        var persisted = await context.ServiceOrders
            .Include(so => so.StatusHistory)
            .FirstAsync(so => so.Id == serviceOrder.Id);
        persisted.GetLastStatusHistory().Status.Should().Be(OrderStatus.Received);
    }

    [Fact]
    public async Task GetByIdWithPartsDetailsAsync_ShouldReturnNull_WhenServiceOrderDoesNotExist()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = await sut.GetByIdWithPartsDetailsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithPartsDetailsAsync_ShouldReturnServiceOrderWithPartsPopulated_WhenItExists()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        var part = CreatePart();
        var serviceOrder = CreateServiceOrder(customer, vehicle, service);
        serviceOrder.AddPart(part, 2);

        await SeedBaseEntitiesAsync(context, customer, vehicle, service);
        context.Parts.Add(part);
        context.ServiceOrders.Add(serviceOrder);
        await context.SaveChangesAsync();

        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = await sut.GetByIdWithPartsDetailsAsync(serviceOrder.Id);

        // Assert
        result.Should().NotBeNull();
        result!.PartsUsed.Should().HaveCount(1);
        result.PartsUsed.First().Part.Should().NotBeNull();
        result.PartsUsed.First().Part.Name.Should().Be("Filtro de óleo");
    }

    // ─── GetByIdForUpdateAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdForUpdateAsync_ShouldReturnNull_WhenServiceOrderDoesNotExist()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = await sut.GetByIdForUpdateAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdForUpdateAsync_ShouldReturnServiceOrderWithAllRelations_WhenItExists()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        var serviceOrder = CreateServiceOrder(customer, vehicle, service);

        await SeedBaseEntitiesAsync(context, customer, vehicle, service);
        context.ServiceOrders.Add(serviceOrder);
        await context.SaveChangesAsync();

        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = await sut.GetByIdForUpdateAsync(serviceOrder.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Customer.Should().NotBeNull();
        result.Vehicle.Should().NotBeNull();
        result.StatusHistory.Should().NotBeEmpty();
        result.ServicesUsed.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetServiceOrderByIdToGetPeddingStocksAsync_ShouldReturnNull_WhenServiceOrderDoesNotExist()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = await sut.GetServiceOrderByIdToGetPeddingStocksAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetServiceOrderByIdToGetPeddingStocksAsync_ShouldReturnServiceOrderWithParts_WhenItExists()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        var part = CreatePart();
        var serviceOrder = CreateServiceOrder(customer, vehicle, service);
        serviceOrder.AddPart(part, 1);

        await SeedBaseEntitiesAsync(context, customer, vehicle, service);
        context.Parts.Add(part);
        context.ServiceOrders.Add(serviceOrder);
        await context.SaveChangesAsync();

        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = await sut.GetServiceOrderByIdToGetPeddingStocksAsync(serviceOrder.Id);

        // Assert
        result.Should().NotBeNull();
        result!.PartsUsed.Should().HaveCount(1);
        result.PartsUsed.First().Part.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistEntityChanges_WhenStatusIsUpdated()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        var serviceOrder = CreateServiceOrder(customer, vehicle, service);

        await SeedBaseEntitiesAsync(context, customer, vehicle, service);
        context.ServiceOrders.Add(serviceOrder);
        await context.SaveChangesAsync();

        serviceOrder.StartDiagnostics();

        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        await sut.SaveChangesAsync(serviceOrder);

        // Assert
        var updated = await context.ServiceOrders
            .Include(so => so.StatusHistory)
            .FirstAsync(so => so.Id == serviceOrder.Id);
        updated.GetLastStatusHistory().Status.Should().Be(OrderStatus.InDiagnostics);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistAddedPart_WhenPartIsAttachedToOrder()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        var part = CreatePart();
        var serviceOrder = CreateServiceOrder(customer, vehicle, service);

        await SeedBaseEntitiesAsync(context, customer, vehicle, service);
        context.Parts.Add(part);
        context.ServiceOrders.Add(serviceOrder);
        await context.SaveChangesAsync();

        serviceOrder.AddPart(part, 3);

        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        await sut.SaveChangesAsync(serviceOrder);

        // Assert
        var updated = await context.ServiceOrders
            .Include(so => so.PartsUsed)
            .FirstAsync(so => so.Id == serviceOrder.Id);
        updated.PartsUsed.Should().HaveCount(1);
        updated.PartsUsed.First().Quantity.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnServiceOrdersWithServicesAndPartsLoaded_WhenTheyExist()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        var part = CreatePart();
        var serviceOrder = CreateServiceOrder(customer, vehicle, service);
        serviceOrder.AddPart(part, 1);

        await SeedBaseEntitiesAsync(context, customer, vehicle, service);
        context.Parts.Add(part);
        context.ServiceOrders.Add(serviceOrder);
        await context.SaveChangesAsync();

        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = (await sut.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].ServicesUsed.Should().NotBeEmpty();
        result[0].PartsUsed.Should().NotBeEmpty();
        result[0].Customer.Should().NotBeNull();
        result[0].Vehicle.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdWithPartsDetailsAsync_ShouldReturnCorrectPartQuantity_WhenPartIsAdded()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        var part = CreatePart();
        var serviceOrder = CreateServiceOrder(customer, vehicle, service);
        serviceOrder.AddPart(part, 5);

        await SeedBaseEntitiesAsync(context, customer, vehicle, service);
        context.Parts.Add(part);
        context.ServiceOrders.Add(serviceOrder);
        await context.SaveChangesAsync();

        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = await sut.GetByIdWithPartsDetailsAsync(serviceOrder.Id);

        // Assert
        result.Should().NotBeNull();
        result!.PartsUsed.First().Quantity.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdForUpdateAsync_ShouldReturnServiceOrderWithPartsLoaded_WhenPartsExist()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        var part = CreatePart();
        var serviceOrder = CreateServiceOrder(customer, vehicle, service);
        serviceOrder.AddPart(part, 2);

        await SeedBaseEntitiesAsync(context, customer, vehicle, service);
        context.Parts.Add(part);
        context.ServiceOrders.Add(serviceOrder);
        await context.SaveChangesAsync();

        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = await sut.GetByIdForUpdateAsync(serviceOrder.Id);

        // Assert
        result.Should().NotBeNull();
        result!.PartsUsed.Should().HaveCount(1);
        result.PartsUsed.First().Part.Should().NotBeNull();
        result.PartsUsed.First().Part.Name.Should().Be("Filtro de óleo");
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeFinishedAndDeliveredOrders_AndOrderRemainingByStatusThenAge()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        await SeedBaseEntitiesAsync(context, customer, vehicle, service);

        var received = CreateServiceOrder(customer, vehicle, service);

        var inDiagnostics = CreateServiceOrder(customer, vehicle, service);
        inDiagnostics.StartDiagnostics();

        var waitingApproval = CreateServiceOrder(customer, vehicle, service);
        waitingApproval.StartDiagnostics();
        waitingApproval.FinishAnalysis();

        var executing = CreateServiceOrder(customer, vehicle, service);
        executing.StartDiagnostics();
        executing.FinishAnalysis();
        executing.ApproveServiceOrder();

        var finished = CreateServiceOrder(customer, vehicle, service);
        finished.StartDiagnostics();
        finished.FinishAnalysis();
        finished.ApproveServiceOrder();
        finished.FinishExecution();

        var delivered = CreateServiceOrder(customer, vehicle, service);
        delivered.StartDiagnostics();
        delivered.FinishAnalysis();
        delivered.ApproveServiceOrder();
        delivered.FinishExecution();
        delivered.Deliver();

        context.ServiceOrders.AddRange(received, inDiagnostics, waitingApproval, executing, finished, delivered);
        await context.SaveChangesAsync();

        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        var result = (await sut.GetAllAsync()).ToList();

        // Assert
        result.Select(so => so.Id).Should().NotContain(finished.Id);
        result.Select(so => so.Id).Should().NotContain(delivered.Id);
        result.Select(so => so.Id).Should().ContainInOrder(executing.Id, waitingApproval.Id, inDiagnostics.Id, received.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistServicesUsed_WhenOrderHasServices()
    {
        // Arrange
        using var db = CreateDbContext();
        var (context, connection) = db;
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        await SeedBaseEntitiesAsync(context, customer, vehicle, service);

        var serviceOrder = CreateServiceOrder(customer, vehicle, service);
        var sut = new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>());

        // Act
        await sut.AddAsync(serviceOrder);

        // Assert
        var persisted = await context.ServiceOrders
            .Include(so => so.ServicesUsed)
            .FirstAsync(so => so.Id == serviceOrder.Id);
        persisted.ServicesUsed.Should().HaveCount(1);
    }
}
