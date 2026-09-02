using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.UseCases.ServiceOrders;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Infrastructure.Data;
using OficinaApi.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;

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

        var useCase = new CreateServiceOrderUseCase(
            new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()),
            new VehicleRepository(context, Mock.Of<ILogger<VehicleRepository>>()),
            new ServiceRepository(context, Mock.Of<ILogger<ServiceRepository>>()),
            new CustomerRepository(context, Mock.Of<ILogger<CustomerRepository>>()),
            Mock.Of<ILogger<CreateServiceOrderUseCase>>());

        var result = await useCase.ExecuteAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();
        result.Response.LastStatus.Should().Be("Received");
    }

    [Fact]
    public async Task DiagnosticsServiceOrderStatus()
    {
        var context = CreateContext();

        var orderId = await CreateBaseOrder(context);

        var useCase = new StartDiagnosticsUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<StartDiagnosticsUseCase>>(), Mock.Of<IApplicationMetrics>());
        var result = await useCase.ExecuteAsync(new StartDiagnosticsRequest(orderId));

        result.IsSuccess.Should().BeTrue();
        result.Response.LastStatus.Should().Be("InDiagnostics");
    }

    [Fact]
    public async Task FinishAnalysisServiceOrderStatus()
    {
        var context = CreateContext();

        var orderId = await CreateBaseOrder(context, 200m);

        var startDiagUseCase = new StartDiagnosticsUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<StartDiagnosticsUseCase>>(), Mock.Of<IApplicationMetrics>());
        await startDiagUseCase.ExecuteAsync(new StartDiagnosticsRequest(orderId));

        var useCase = new FinishAnalysisUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<FinishAnalysisUseCase>>(), Mock.Of<IApplicationMetrics>());
        var result = await useCase.ExecuteAsync(new FinishAnalysisRequest(orderId));

        result.IsSuccess.Should().BeTrue();
        result.Response.LastStatus.Should().Be("WaitingApproval");
        result.Response.Budget.Should().Be(200m);
    }

    [Fact]
    public async Task ApproveServiceOrderStatus()
    {
        var context = CreateContext();

        var orderId = await CreateBaseOrder(context);

        var startDiagUseCase = new StartDiagnosticsUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<StartDiagnosticsUseCase>>(), Mock.Of<IApplicationMetrics>());
        await startDiagUseCase.ExecuteAsync(new StartDiagnosticsRequest(orderId));

        var finishAnalysisUseCase = new FinishAnalysisUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<FinishAnalysisUseCase>>(), Mock.Of<IApplicationMetrics>());
        await finishAnalysisUseCase.ExecuteAsync(new FinishAnalysisRequest(orderId));

        var useCase = new ApproveServiceOrderUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<ApproveServiceOrderUseCase>>(), Mock.Of<IApplicationMetrics>());
        var result = await useCase.ExecuteAsync(new ApproveServiceOrderRequest(orderId));

        result.IsSuccess.Should().BeTrue();
        result.Response.LastStatus.Should().Be("Executing");
    }

    [Fact]
    public async Task FinishExecutionServiceOrderStatus()
    {
        var context = CreateContext();

        var orderId = await CreateBaseOrder(context);

        var startDiagUseCase = new StartDiagnosticsUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<StartDiagnosticsUseCase>>(), Mock.Of<IApplicationMetrics>());
        await startDiagUseCase.ExecuteAsync(new StartDiagnosticsRequest(orderId));

        var finishAnalysisUseCase = new FinishAnalysisUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<FinishAnalysisUseCase>>(), Mock.Of<IApplicationMetrics>());
        await finishAnalysisUseCase.ExecuteAsync(new FinishAnalysisRequest(orderId));

        var approveUseCase = new ApproveServiceOrderUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<ApproveServiceOrderUseCase>>(), Mock.Of<IApplicationMetrics>());
        await approveUseCase.ExecuteAsync(new ApproveServiceOrderRequest(orderId));

        var useCase = new FinishExecutionUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<FinishExecutionUseCase>>(), Mock.Of<IApplicationMetrics>());
        var result = await useCase.ExecuteAsync(new FinishExecutionRequest(orderId));

        result.IsSuccess.Should().BeTrue();
        result.Response.LastStatus.Should().Be("Finished");
    }

    [Fact]
    public async Task DeliverServiceOrderStatus()
    {
        var context = CreateContext();

        var orderId = await CreateBaseOrder(context);

        var startDiagUseCase = new StartDiagnosticsUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<StartDiagnosticsUseCase>>(), Mock.Of<IApplicationMetrics>());
        await startDiagUseCase.ExecuteAsync(new StartDiagnosticsRequest(orderId));

        var finishAnalysisUseCase = new FinishAnalysisUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<FinishAnalysisUseCase>>(), Mock.Of<IApplicationMetrics>());
        await finishAnalysisUseCase.ExecuteAsync(new FinishAnalysisRequest(orderId));

        var approveUseCase = new ApproveServiceOrderUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<ApproveServiceOrderUseCase>>(), Mock.Of<IApplicationMetrics>());
        await approveUseCase.ExecuteAsync(new ApproveServiceOrderRequest(orderId));

        var finishExecUseCase = new FinishExecutionUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<FinishExecutionUseCase>>(), Mock.Of<IApplicationMetrics>());
        await finishExecUseCase.ExecuteAsync(new FinishExecutionRequest(orderId));

        var useCase = new DeliverServiceOrderUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<DeliverServiceOrderUseCase>>(), Mock.Of<IApplicationMetrics>());
        var result = await useCase.ExecuteAsync(new DeliverServiceOrderRequest(orderId));

        result.IsSuccess.Should().BeTrue();
        result.Response.LastStatus.Should().Be("Delivered");
    }

    [Fact]
    public async Task AddService()
    {
        var context = CreateContext();

        var orderId = await CreateBaseOrder(context);

        var extraService = CreateServiceEntity(300m);
        context.Services.Add(extraService);
        await context.SaveChangesAsync();

        var useCase = new AddServiceToServiceOrderUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), new ServiceRepository(context, Mock.Of<ILogger<ServiceRepository>>()), Mock.Of<ILogger<AddServiceToServiceOrderUseCase>>());
        var result = await useCase.ExecuteAsync(new AddServiceToServiceOrderRequest(orderId, new AddServiceDto
        {
            ServiceId = extraService.Id
        }));

        result.IsSuccess.Should().BeTrue();

        var order = await context.ServiceOrders
            .Include(o => o.ServicesUsed)
            .FirstAsync();

        order.ServicesUsed.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddPart()
    {
        var context = CreateContext();

        var orderId = await CreateBaseOrder(context);

        var part = CreatePart(stock: 5);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        var addPartUseCase = new AddPartToServiceOrderUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), new PartRepository(context, Mock.Of<ILogger<PartRepository>>()), Mock.Of<ILogger<AddPartToServiceOrderUseCase>>());
        var resultPart = await addPartUseCase.ExecuteAsync(new AddPartToServiceOrderRequest(orderId, new AddPartDto
        {
            PartId = part.Id,
            Quantity = 2
        }));
        resultPart.IsSuccess.Should().BeTrue();

        var startDiagUseCase = new StartDiagnosticsUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<StartDiagnosticsUseCase>>(), Mock.Of<IApplicationMetrics>());
        await startDiagUseCase.ExecuteAsync(new StartDiagnosticsRequest(orderId));

        var finishAnalysisUseCase = new FinishAnalysisUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<FinishAnalysisUseCase>>(), Mock.Of<IApplicationMetrics>());
        await finishAnalysisUseCase.ExecuteAsync(new FinishAnalysisRequest(orderId));

        var approveUseCase = new ApproveServiceOrderUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<ApproveServiceOrderUseCase>>(), Mock.Of<IApplicationMetrics>());
        await approveUseCase.ExecuteAsync(new ApproveServiceOrderRequest(orderId));

        var useCase = new GetServiceOrderPendingStocksUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<GetServiceOrderPendingStocksUseCase>>());
        var resultPending = await useCase.ExecuteAsync(orderId);

        resultPending.IsSuccess.Should().BeTrue();
        resultPending.Response.Should().HaveCount(1);
        resultPending.Response.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task ServiceOrderComplete()
    {
        var context = CreateContext();

        var orderId = await CreateBaseOrder(context);

        var startDiagUseCase = new StartDiagnosticsUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<StartDiagnosticsUseCase>>(), Mock.Of<IApplicationMetrics>());
        await startDiagUseCase.ExecuteAsync(new StartDiagnosticsRequest(orderId));

        var finishAnalysisUseCase = new FinishAnalysisUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<FinishAnalysisUseCase>>(), Mock.Of<IApplicationMetrics>());
        await finishAnalysisUseCase.ExecuteAsync(new FinishAnalysisRequest(orderId));

        var approveUseCase = new ApproveServiceOrderUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<ApproveServiceOrderUseCase>>(), Mock.Of<IApplicationMetrics>());
        await approveUseCase.ExecuteAsync(new ApproveServiceOrderRequest(orderId));

        var finishExecUseCase = new FinishExecutionUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<FinishExecutionUseCase>>(), Mock.Of<IApplicationMetrics>());
        await finishExecUseCase.ExecuteAsync(new FinishExecutionRequest(orderId));

        var useCase = new DeliverServiceOrderUseCase(new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()), Mock.Of<ILogger<DeliverServiceOrderUseCase>>(), Mock.Of<IApplicationMetrics>());
        var result = await useCase.ExecuteAsync(new DeliverServiceOrderRequest(orderId));

        result.IsSuccess.Should().BeTrue();
        result.Response.LastStatus.Should().Be("Delivered");
    }

    #region Helper

    private async Task<Guid> CreateBaseOrder(OficinaDbContext context, decimal price = 100m)
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

        var useCase = new CreateServiceOrderUseCase(
            new ServiceOrderRepository(context, Mock.Of<ILogger<ServiceOrderRepository>>()),
            new VehicleRepository(context, Mock.Of<ILogger<VehicleRepository>>()),
            new ServiceRepository(context, Mock.Of<ILogger<ServiceRepository>>()),
            new CustomerRepository(context, Mock.Of<ILogger<CustomerRepository>>()),
            Mock.Of<ILogger<CreateServiceOrderUseCase>>());

        var created = await useCase.ExecuteAsync(dto);

        return created.Response.Id;
    }

    #endregion
}