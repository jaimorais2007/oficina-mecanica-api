using FluentAssertions;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Exceptions;
using Xunit;

namespace Unit.Tests;

public class ServiceOrderEntityTests
{
    private static Customer CreateCustomer()
        => new("João Silva", PersonType.Individual, "529.982.247-25", new DateTime(1990, 1, 1),"teste@gmail.com");

    private static Vehicle CreateVehicle(Customer? customer = null)
    {
        customer ??= CreateCustomer();
        return new Vehicle(customer, "ABC1D23", "Toyota", "Corolla", 2020);
    }

    private static Service CreateService(decimal price = 150m)
        => new("Troca de óleo", "Troca completa de óleo do motor", price);

    private static Part CreatePart(int stock = 10, decimal price = 50m)
        => new("Filtro de óleo", "FO-001", stock, price);

    private static ServiceOrder CreateServiceOrder(Service? service = null)
    {
        var customer = CreateCustomer();
        var vehicle  = CreateVehicle(customer);
        service    ??= CreateService();
        return new ServiceOrder(customer, vehicle, new[] { service });
    }

    private static ServiceOrder CreateServiceOrderInStatus(OrderStatus targetStatus)
    {
        var order = CreateServiceOrder();
        if (targetStatus == OrderStatus.Received) return order;

        order.StartDiagnostics();
        if (targetStatus == OrderStatus.InDiagnostics) return order;

        order.FinishAnalysis();
        if (targetStatus == OrderStatus.WaitingApproval) return order;

        order.ApproveServiceOrder();
        if (targetStatus == OrderStatus.Executing) return order;

        order.FinishExecution();
        if (targetStatus == OrderStatus.Finished) return order;

        order.Deliver();
        return order;
    }

    [Fact]
    public void Constructor_ShouldInitialize_WithReceivedStatus()
    {
        // Arrange
        var customer = CreateCustomer();
        var vehicle  = CreateVehicle(customer);
        var service  = CreateService();

        // Act
        var order = new ServiceOrder(customer, vehicle, new[] { service });

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Received);
    }

    [Fact]
    public void Constructor_ShouldAssociate_CustomerVehicleAndServices()
    {
        // Arrange
        var customer = CreateCustomer();
        var vehicle  = CreateVehicle(customer);
        var service  = CreateService();

        // Act
        var order = new ServiceOrder(customer, vehicle, new[] { service });

        // Assert
        order.CustomerId.Should().Be(customer.Id);
        order.VehicleId.Should().Be(vehicle.Id);
        order.ServicesUsed.Should().HaveCount(1);
        order.ServicesUsed.First().ServiceId.Should().Be(service.Id);
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAt_ToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var order = CreateServiceOrder();

        // Assert
        order.CreatedAt.Should().BeAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void StartDiagnostics_ShouldTransitionToInDiagnostics_WhenStatusIsReceived()
    {
        // Arrange
        var order = CreateServiceOrder();

        // Act
        order.StartDiagnostics();

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.InDiagnostics);
    }

    [Theory]
    [InlineData(OrderStatus.InDiagnostics)]
    [InlineData(OrderStatus.WaitingApproval)]
    [InlineData(OrderStatus.Executing)]
    public void StartDiagnostics_ShouldThrow_WhenStatusIsNotReceived(OrderStatus currentStatus)
    {
        // Arrange
        var order = CreateServiceOrderInStatus(currentStatus);

        // Act
        Action act = () => order.StartDiagnostics();

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("*'Recebida'*");
    }

    [Fact]
    public void FinishAnalysis_ShouldTransitionToWaitingApproval_WhenStatusIsInDiagnostics()
    {
        // Arrange
        var order = CreateServiceOrderInStatus(OrderStatus.InDiagnostics);

        // Act
        order.FinishAnalysis();

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.WaitingApproval);
    }

    [Fact]
    public void FinishAnalysis_ShouldCalculateBudget_BasedOnServices()
    {
        // Arrange
        var service = CreateService(price: 200m);
        var order   = CreateServiceOrder(service);
        order.StartDiagnostics();

        // Act
        order.FinishAnalysis();

        // Assert
        order.Budget.Should().Be(200m);
    }

    [Fact]
    public void FinishAnalysis_ShouldCalculateBudget_IncludingParts()
    {
        // Arrange
        var service = CreateService(price: 200m);
        var part    = CreatePart(price: 50m);
        var order   = CreateServiceOrder(service);
        order.AddPart(part, 2);
        order.StartDiagnostics();

        // Act
        order.FinishAnalysis();

        order.Budget.Should().Be(250m);
    }

    [Theory]
    [InlineData(OrderStatus.Received)]
    [InlineData(OrderStatus.WaitingApproval)]
    public void FinishAnalysis_ShouldThrow_WhenStatusIsNotInDiagnostics(OrderStatus currentStatus)
    {
        // Arrange
        var order = CreateServiceOrderInStatus(currentStatus);

        // Act
        Action act = () => order.FinishAnalysis();

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("*'Em Análise'*");
    }

    [Fact]
    public void ApproveServiceOrder_ShouldTransitionToExecuting_WhenStatusIsWaitingApproval()
    {
        // Arrange
        var order = CreateServiceOrderInStatus(OrderStatus.WaitingApproval);

        // Act
        order.ApproveServiceOrder();

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Executing);
    }

    [Fact]
    public void ApproveServiceOrder_ShouldRaise_ServiceOrderApprovedEvent()
    {
        // Arrange
        var order = CreateServiceOrderInStatus(OrderStatus.WaitingApproval);

        // Act
        order.ApproveServiceOrder();

        // Assert
        order.DomainEvents.Should().ContainSingle(e => e is ServiceOrderApprovedEvent);
    }

    [Theory]
    [InlineData(OrderStatus.Received)]
    [InlineData(OrderStatus.InDiagnostics)]
    [InlineData(OrderStatus.Executing)]
    public void ApproveServiceOrder_ShouldThrow_WhenStatusIsNotWaitingApproval(OrderStatus currentStatus)
    {
        // Arrange
        var order = CreateServiceOrderInStatus(currentStatus);

        // Act
        Action act = () => order.ApproveServiceOrder();

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("*'Aguardando Aprovação'*");
    }

    [Fact]
    public void FinishExecution_ShouldTransitionToFinished_WhenNoPendingStocks()
    {
        // Arrange
        var order = CreateServiceOrderInStatus(OrderStatus.Executing);

        // Act
        order.FinishExecution();

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Finished);
    }

    [Fact]
    public void FinishExecution_ShouldThrow_WhenPartsHavePendingStock()
    {
        // Arrange
        var part  = CreatePart(stock: 10);
        var order = CreateServiceOrder();
        order.AddPart(part, 2);
        order.StartDiagnostics();
        order.FinishAnalysis();
        order.ApproveServiceOrder();

        // Act
        Action act = () => order.FinishExecution();

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("*peças pendentes*");
    }

    [Theory]
    [InlineData(OrderStatus.Received)]
    [InlineData(OrderStatus.InDiagnostics)]
    [InlineData(OrderStatus.WaitingApproval)]
    public void FinishExecution_ShouldThrow_WhenStatusIsNotExecuting(OrderStatus currentStatus)
    {
        // Arrange
        var order = CreateServiceOrderInStatus(currentStatus);

        // Act
        Action act = () => order.FinishExecution();

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("*'Em Execução'*");
    }

    [Fact]
    public void Deliver_ShouldTransitionToDelivered_WhenStatusIsFinished()
    {
        // Arrange
        var order = CreateServiceOrderInStatus(OrderStatus.Finished);

        // Act
        order.Deliver();

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Delivered);
    }

    [Theory]
    [InlineData(OrderStatus.Received)]
    [InlineData(OrderStatus.InDiagnostics)]
    [InlineData(OrderStatus.Executing)]
    public void Deliver_ShouldThrow_WhenStatusIsNotFinished(OrderStatus currentStatus)
    {
        // Arrange
        var order = CreateServiceOrderInStatus(currentStatus);

        // Act
        Action act = () => order.Deliver();

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("*'Finalizada'*");
    }

    [Theory]
    [InlineData(OrderStatus.Received)]
    [InlineData(OrderStatus.InDiagnostics)]
    public void AddPart_ShouldAddPart_WhenStatusAllows(OrderStatus currentStatus)
    {
        // Arrange
        var part  = CreatePart();
        var order = CreateServiceOrderInStatus(currentStatus);

        // Act
        order.AddPart(part, 3);

        // Assert
        order.PartsUsed.Should().ContainSingle(p => p.PartId == part.Id && p.Quantity == 3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AddPart_ShouldThrow_WhenQuantityIsNotPositive(int quantity)
    {
        // Arrange
        var part  = CreatePart();
        var order = CreateServiceOrder();

        // Act
        Action act = () => order.AddPart(part, quantity);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("*quantidade deve ser maior que zero*");
    }

    [Theory]
    [InlineData(OrderStatus.WaitingApproval)]
    [InlineData(OrderStatus.Executing)]
    public void AddPart_ShouldThrow_WhenStatusDoesNotAllow(OrderStatus currentStatus)
    {
        // Arrange
        var part  = CreatePart();
        var order = CreateServiceOrderInStatus(currentStatus);

        // Act
        Action act = () => order.AddPart(part, 1);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("*Não é permitido adicionar peças*");
    }

    [Theory]
    [InlineData(OrderStatus.Received)]
    [InlineData(OrderStatus.InDiagnostics)]
    public void AddService_ShouldAddService_WhenStatusAllows(OrderStatus currentStatus)
    {
        // Arrange
        var newService = CreateService(price: 80m);
        var order      = CreateServiceOrderInStatus(currentStatus);
        var initialCount = order.ServicesUsed.Count;

        // Act
        order.AddService(newService);

        // Assert
        order.ServicesUsed.Should().HaveCount(initialCount + 1);
    }

    [Theory]
    [InlineData(OrderStatus.WaitingApproval)]
    [InlineData(OrderStatus.Executing)]
    public void AddService_ShouldThrow_WhenStatusDoesNotAllow(OrderStatus currentStatus)
    {
        // Arrange
        var newService = CreateService();
        var order      = CreateServiceOrderInStatus(currentStatus);

        // Act
        Action act = () => order.AddService(newService);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("*Não é permitido adicionar serviços*");
    }

    [Fact]
    public void GetPendingStocks_ShouldReturnOnlyParts_WhereStockWasNotEnsured()
    {
        // Arrange
        var part1 = CreatePart(stock: 10);
        var part2 = CreatePart(stock: 10);
        var order = CreateServiceOrder();
        order.AddPart(part1, 1);
        order.AddPart(part2, 1);
        order.StartDiagnostics();
        order.FinishAnalysis();
        order.ApproveServiceOrder();

        // Garante o estoque apenas da primeira peça
        order.PartsUsed.First().EnsureStockQuantity();

        // Act
        var pendingStocks = order.GetPendingStocks();

        // Assert
        pendingStocks.Should().ContainSingle()
                     .Which.PartId.Should().Be(part2.Id);
    }

    [Fact]
    public void GetPendingStocks_ShouldReturnEmpty_WhenAllStocksAreEnsured()
    {
        // Arrange
        var part  = CreatePart(stock: 10);
        var order = CreateServiceOrder();
        order.AddPart(part, 1);
        order.PartsUsed.First().EnsureStockQuantity();
        order.StartDiagnostics();
        order.FinishAnalysis();
        order.ApproveServiceOrder();
        
        // Act
        var pendingStocks = order.GetPendingStocks();

        // Assert
        pendingStocks.Should().BeEmpty();
    }
}
