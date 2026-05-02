using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using Xunit;

namespace OficinaApi.Tests.UnitTests.Domain.Entities;

public class ServiceOrderTests
{
    private static Customer CreateCustomer() =>
        new("João Silva", PersonType.Individual, "529.982.247-25", new DateTime(1990, 1, 1));

    private static Vehicle CreateVehicle(Customer customer) =>
        new(customer, "ABC-1234", "Toyota", "Corolla", 2020);

    private static Service CreateService(decimal price = 100m) =>
        new("Troca de óleo", "Substituição do óleo do motor", price);

    private static Part CreatePart(int stock = 10) =>
        new("Filtro de óleo", "FO-001", stock, 50m);

    private static ServiceOrder CreateServiceOrder(IEnumerable<Service>? services = null)
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var serviceList = services ?? [CreateService()];
        return new ServiceOrder(customer, vehicle, serviceList);
    }

    [Fact]
    public void Constructor_ShouldCreateServiceOrder_WithReceivedStatus()
    {
        // Arrange
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();

        // Act
        var order = new ServiceOrder(customer, vehicle, [service]);

        // Assert
        order.Id.Should().NotBeEmpty();
        order.Customer.Should().Be(customer);
        order.Vehicle.Should().Be(vehicle);
        order.ServicesUsed.Should().HaveCount(1);
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Received);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
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

    [Fact]
    public void StartDiagnostics_ShouldThrow_WhenStatusIsNotReceived()
    {
        // Arrange
        var order = CreateServiceOrder();
        order.StartDiagnostics(); // InDiagnostics

        // Act
        Action act = () => order.StartDiagnostics();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*'Recebida'*");
    }

    [Fact]
    public void FinishAnalysis_ShouldTransitionToWaitingApproval_WhenStatusIsInDiagnostics()
    {
        // Arrange
        var order = CreateServiceOrder();
        order.StartDiagnostics();

        // Act
        order.FinishAnalysis();

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.WaitingApproval);
    }

    [Fact]
    public void FinishAnalysis_ShouldCalculateBudget()
    {
        // Arrange
        var service = CreateService(price: 200m);
        var order = CreateServiceOrder([service]);
        order.StartDiagnostics();

        // Act
        order.FinishAnalysis();

        // Assert
        order.Budget.Should().Be(200m);
    }

    [Fact]
    public void FinishAnalysis_ShouldThrow_WhenStatusIsNotInDiagnostics()
    {
        // Arrange
        var order = CreateServiceOrder(); // Received

        // Act
        Action act = () => order.FinishAnalysis();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*'Em Análise'*");
    }

    [Fact]
    public void ApproveServiceOrder_ShouldTransitionToExecuting_WhenStatusIsWaitingApproval()
    {
        // Arrange
        var order = CreateServiceOrder();
        order.StartDiagnostics();
        order.FinishAnalysis();

        // Act
        order.ApproveServiceOrder();

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Executing);
    }

    [Fact]
    public void ApproveServiceOrder_ShouldAddDomainEvent_WhenApproved()
    {
        // Arrange
        var order = CreateServiceOrder();
        order.StartDiagnostics();
        order.FinishAnalysis();

        // Act
        order.ApproveServiceOrder();

        // Assert
        order.DomainEvents.Should().ContainSingle(e =>
            e.GetType().Name == "ServiceOrderApprovedEvent");
    }

    [Fact]
    public void ApproveServiceOrder_ShouldThrow_WhenStatusIsNotWaitingApproval()
    {
        // Arrange
        var order = CreateServiceOrder(); // Received

        // Act
        Action act = () => order.ApproveServiceOrder();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*'Aguardando Aprovação'*");
    }

    [Fact]
    public void FinishExecution_ShouldTransitionToFinished_WhenStatusIsExecutingAndNoPendingStocks()
    {
        // Arrange
        var order = CreateServiceOrder();
        order.StartDiagnostics();
        order.FinishAnalysis();
        order.ApproveServiceOrder();

        // Act
        order.FinishExecution();

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Finished);
    }

    [Fact]
    public void FinishExecution_ShouldThrow_WhenStatusIsNotExecuting()
    {
        // Arrange
        var order = CreateServiceOrder(); // Received

        // Act
        Action act = () => order.FinishExecution();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*'Em Execução'*");
    }

    [Fact]
    public void FinishExecution_ShouldThrow_WhenThereArePendingStocks()
    {
        // Arrange
        var order = CreateServiceOrder();
        order.StartDiagnostics();
        var part = CreatePart(stock: 5);
        order.AddPart(part, 2); // estoque ainda não garantido
        order.FinishAnalysis();
        order.ApproveServiceOrder();

        // Act
        Action act = () => order.FinishExecution();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*peças pendentes*");
    }

    [Fact]
    public void Deliver_ShouldTransitionToDelivered_WhenStatusIsFinished()
    {
        // Arrange
        var order = CreateServiceOrder();
        order.StartDiagnostics();
        order.FinishAnalysis();
        order.ApproveServiceOrder();
        order.FinishExecution();

        // Act
        order.Deliver();

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void Deliver_ShouldThrow_WhenStatusIsNotFinished()
    {
        // Arrange
        var order = CreateServiceOrder(); // Received

        // Act
        Action act = () => order.Deliver();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*'Finalizada'*");
    }

    [Fact]
    public void AddPart_ShouldAddPartToOrder_WhenStatusAllows()
    {
        // Arrange
        var order = CreateServiceOrder(); // Received
        var part = CreatePart();

        // Act
        order.AddPart(part, 3);

        // Assert
        order.PartsUsed.Should().HaveCount(1);
        order.PartsUsed.First().Quantity.Should().Be(3);
    }

    [Fact]
    public void AddPart_ShouldThrow_WhenQuantityIsZeroOrNegative()
    {
        // Arrange
        var order = CreateServiceOrder();
        var part = CreatePart();

        // Act
        Action act = () => order.AddPart(part, 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*quantidade*");
    }

    [Fact]
    public void AddPart_ShouldThrow_WhenStatusDoesNotAllowModification()
    {
        // Arrange
        var order = CreateServiceOrder();
        order.StartDiagnostics();
        order.FinishAnalysis(); // WaitingApproval
        var part = CreatePart();

        // Act
        Action act = () => order.AddPart(part, 1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*adicionar peças*");
    }

    [Fact]
    public void AddService_ShouldAddServiceToOrder_WhenStatusAllows()
    {
        // Arrange
        var order = CreateServiceOrder(); // Received
        var extraService = CreateService(price: 300m);

        // Act
        order.AddService(extraService);

        // Assert
        order.ServicesUsed.Should().HaveCount(2);
    }

    [Fact]
    public void AddService_ShouldThrow_WhenStatusDoesNotAllowModification()
    {
        // Arrange
        var order = CreateServiceOrder();
        order.StartDiagnostics();
        order.FinishAnalysis(); // WaitingApproval
        var extraService = CreateService();

        // Act
        Action act = () => order.AddService(extraService);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*adicionar serviços*");
    }

    [Fact]
    public void CalculateBudget_ShouldSumServicesAndParts()
    {
        // Arrange
        var service = CreateService(price: 150m);
        var order = CreateServiceOrder([service]);
        var part = CreatePart();
        order.AddPart(part, 2); // 2 × 50 = 100

        // Act
        order.CalculateBudget();

        // Assert
        order.Budget.Should().Be(250m); // 150 + 100
    }

    [Fact]
    public void GetPendingStocks_ShouldReturnPartsNotYetEnsured()
    {
        // Arrange
        var order = CreateServiceOrder();
        var part = CreatePart(stock: 5);
        order.AddPart(part, 2);

        // Act
        var pending = order.GetPendingStocks();

        // Assert
        pending.Should().HaveCount(1);
        pending.First().Part.Should().Be(part);
    }
}
