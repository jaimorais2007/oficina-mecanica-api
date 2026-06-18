using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Infrastructure.Data;
using OficinaApi.Infrastructure.Repositories;
using Xunit;
using AppServiceOrderService = OficinaApi.Application.Services.ServiceOrderService;

namespace Integration.Tests;

public class ServiceOrderIntegrationTests
{
    private OficinaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dispatcherMock = new Mock<IDomainEventDispatcher>();

        return new OficinaDbContext(options, dispatcherMock.Object);
    }

    private AppServiceOrderService CreateService(OficinaDbContext context)
    {
        var serviceOrderRepository = new ServiceOrderRepository(context);
        var vehicleRepository = new VehicleRepository(context);
        var serviceRepository = new ServiceRepository(context);
        var partRepository = new PartRepository(context);
        var customerRepository = new CustomerRepository(context);
        var emailServiceMock = new Mock<IEmailService>();


        return new AppServiceOrderService(
            serviceOrderRepository,
            vehicleRepository,
            serviceRepository,
            partRepository,
            customerRepository,
            emailServiceMock.Object

        );
    }

    #region Mocks

    private Customer CreateCustomer() =>
        new("João", PersonType.Individual, "76331521097", new DateTime(1990, 1, 1), "teste@gmail.com");

    private Vehicle CreateVehicle(Customer customer) =>
        new(customer, "ABC1234", "Toyota", "Corolla", 2020);

    private Service CreateServiceEntity(decimal price = 100m) =>
        new("Troca de óleo", "Descrição", price);

    private Part CreatePart(int stock = 10) =>
        new("Filtro", "F1", stock, 50m);

    #endregion

    [Fact]
    public async Task CreateServiceOrder()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var serviceEntity = CreateServiceEntity();

        context.AddRange(customer, vehicle, serviceEntity);
        await context.SaveChangesAsync();

        var dto = new CreateServiceOrderDto
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            ServicesUsed = new() { serviceEntity.Id }
        };

        var result = await service.CreateServiceOrderAsync(dto);

        result.Should().NotBeNull();
        result.LastStatus.Should().Be("Received");
    }

    [Fact]
    public async Task DiagnosticsServiceOrderStatus()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var (orderId, _) = await CreateBaseOrder(context, service);

        var result = await service.StartDiagnosticsAsync(orderId);

        result.LastStatus.Should().Be("InDiagnostics");
    }

    [Fact]
    public async Task FinishAnalysisServiceOrderStatus()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var (orderId, serviceEntity) = await CreateBaseOrder(context, service, 200m);

        await service.StartDiagnosticsAsync(orderId);
        var result = await service.FinishAnalysisAsync(orderId);

        result.LastStatus.Should().Be("WaitingApproval");
        result.Budget.Should().Be(200m);
    }

    [Fact]
    public async Task ApproveServiceOrderStatus()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var (orderId, _) = await CreateBaseOrder(context, service);

        await service.StartDiagnosticsAsync(orderId);
        await service.FinishAnalysisAsync(orderId);

        var result = await service.ApproveServiceOrderAsync(orderId);

        result.LastStatus.Should().Be("Executing");
    }

    [Fact]
    public async Task FinishExecutionServiceOrderStatus()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var (orderId, _) = await CreateBaseOrder(context, service);

        await service.StartDiagnosticsAsync(orderId);
        await service.FinishAnalysisAsync(orderId);
        await service.ApproveServiceOrderAsync(orderId);

        var result = await service.FinishExecutionAsync(orderId);

        result.LastStatus.Should().Be("Finished");
    }

    [Fact]
    public async Task DeliverServiceOrderStatus()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var (orderId, _) = await CreateBaseOrder(context, service);

        await service.StartDiagnosticsAsync(orderId);
        await service.FinishAnalysisAsync(orderId);
        await service.ApproveServiceOrderAsync(orderId);
        await service.FinishExecutionAsync(orderId);

        var result = await service.DeliverServiceOrderAsync(orderId);

        result.LastStatus.Should().Be("Delivered");
    }

    [Fact]
    public async Task AddService()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var (orderId, _) = await CreateBaseOrder(context, service);

        var extraService = CreateServiceEntity(300m);
        context.Services.Add(extraService);
        await context.SaveChangesAsync();

        await service.AddServiceToServiceOrderAsync(orderId, new AddServiceDto
        {
            ServiceId = extraService.Id
        });

        var order = await context.ServiceOrders
            .Include(o => o.ServicesUsed)
            .FirstAsync();

        order.ServicesUsed.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddPart()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var (orderId, _) = await CreateBaseOrder(context, service);

        var part = CreatePart(stock: 5);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        await service.AddPartToServiceOrderAsync(orderId, new AddPartDto
        {
            PartId = part.Id,
            Quantity = 2
        });

        await service.StartDiagnosticsAsync(orderId);
        await service.FinishAnalysisAsync(orderId);
        await service.ApproveServiceOrderAsync(orderId);

        var pending = await service.GetServiceOrderPeddingStocksAsync(orderId);

        pending.Should().HaveCount(1);
        pending.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task ServiceOrderComplete()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var (orderId, _) = await CreateBaseOrder(context, service);

        await service.StartDiagnosticsAsync(orderId);
        await service.FinishAnalysisAsync(orderId);
        await service.ApproveServiceOrderAsync(orderId);
        await service.FinishExecutionAsync(orderId);

        var result = await service.DeliverServiceOrderAsync(orderId);

        result.LastStatus.Should().Be("Delivered");
    }

    #region Helper

    private async Task<(Guid orderId, Service serviceEntity)> CreateBaseOrder(
        OficinaDbContext context,
        AppServiceOrderService service,
        decimal price = 100m)
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var serviceEntity = CreateServiceEntity(price);

        context.AddRange(customer, vehicle, serviceEntity);
        await context.SaveChangesAsync();

        var dto = new CreateServiceOrderDto
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            ServicesUsed = new() { serviceEntity.Id }
        };

        var created = await service.CreateServiceOrderAsync(dto);

        return (created.Id, serviceEntity);
    }

    #endregion
}