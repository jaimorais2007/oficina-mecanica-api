using FluentAssertions;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Exceptions;
using Xunit;

namespace Unit.Tests;

public class ServiceOrderPartTests
{
    private static Customer CreateCustomer()
        => new("João Silva", PersonType.Individual, "529.982.247-25", new DateTime(1990, 1, 1), "teste@gmail.com");

    private static Vehicle CreateVehicle(Customer customer)
        => new(customer, "ABC1234", "Toyota", "Corolla", 2020);

    private static Service CreateService()
        => new("Troca de óleo", "Troca de óleo do motor", 150m);

    private static Part CreatePart(int stock = 10)
        => new("Filtro de óleo", "FO-001", stock, 45m);

    private static ServiceOrder CreateServiceOrder(OrderStatus targetStatus = OrderStatus.Received)
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var service = CreateService();
        var order = new ServiceOrder(customer, vehicle, [service]);

        if (targetStatus >= OrderStatus.InDiagnostics)
            order.StartDiagnostics();
        if (targetStatus >= OrderStatus.WaitingApproval)
            order.FinishAnalysis();
        if (targetStatus >= OrderStatus.Executing)
            order.ApproveServiceOrder();

        return order;
    }

    [Fact]
    public void Constructor_ComArgumentosValidos_DevePreencherPropriedadesCorretamente()
    {
        // Arrange
        var serviceOrder = CreateServiceOrder();
        var part = CreatePart();
        var quantity = 3;

        // Act
        var sut = new ServiceOrderPart(serviceOrder, part, quantity);

        // Assert
        sut.ServiceOrderId.Should().Be(serviceOrder.Id);
        sut.ServiceOrder.Should().Be(serviceOrder);
        sut.PartId.Should().Be(part.Id);
        sut.Part.Should().Be(part);
        sut.Quantity.Should().Be(quantity);
        sut.StockQuantityWasEnsured.Should().BeFalse();
    }

    [Fact]
    public void EnsureStockQuantity_QuandoNaoFoiGarantido_DeveReduzirEstoqueEDefinirFlag()
    {
        // Arrange
        var part = CreatePart(stock: 10);
        var serviceOrder = CreateServiceOrder();
        var sut = new ServiceOrderPart(serviceOrder, part, 3);

        // Act
        sut.EnsureStockQuantity();

        // Assert
        sut.StockQuantityWasEnsured.Should().BeTrue();
        part.QuantityInStock.Should().Be(7);
    }

    [Fact]
    public void EnsureStockQuantity_QuandoJaFoiGarantido_DeveLancarDomainException()
    {
        // Arrange
        var part = CreatePart(stock: 10);
        var serviceOrder = CreateServiceOrder();
        var sut = new ServiceOrderPart(serviceOrder, part, 2);
        sut.EnsureStockQuantity();

        // Act
        var act = () => sut.EnsureStockQuantity();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*já foi garantida*");
    }

    [Fact]
    public void EnsureStockQuantity_QuandoEstoqueInsuficiente_DeveLancarDomainException()
    {
        // Arrange
        var part = CreatePart(stock: 1);
        var serviceOrder = CreateServiceOrder();
        var sut = new ServiceOrderPart(serviceOrder, part, 5);

        // Act
        var act = () => sut.EnsureStockQuantity();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Estoque insuficiente*");
    }

    [Fact]
    public void StockQuantityShouldBeEnsured_QuandoNaoGarantidoEStatusExecuting_DeveRetornarTrue()
    {
        // Arrange
        var part = CreatePart();
        var serviceOrder = CreateServiceOrder(OrderStatus.Executing);
        var sut = new ServiceOrderPart(serviceOrder, part, 1);

        // Act
        var result = sut.StockQuantityShouldBeEnsured();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void StockQuantityShouldBeEnsured_QuandoJaFoiGarantido_DeveRetornarFalse()
    {
        // Arrange
        var part = CreatePart(stock: 10);
        var serviceOrder = CreateServiceOrder(OrderStatus.Executing);
        var sut = new ServiceOrderPart(serviceOrder, part, 1);
        sut.EnsureStockQuantity();

        // Act
        var result = sut.StockQuantityShouldBeEnsured();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(OrderStatus.Received)]
    [InlineData(OrderStatus.InDiagnostics)]
    [InlineData(OrderStatus.WaitingApproval)]
    public void StockQuantityShouldBeEnsured_QuandoStatusNaoEhExecuting_DeveRetornarFalse(OrderStatus status)
    {
        // Arrange
        var part = CreatePart();
        var serviceOrder = CreateServiceOrder(status);
        var sut = new ServiceOrderPart(serviceOrder, part, 1);

        // Act
        var result = sut.StockQuantityShouldBeEnsured();

        // Assert
        result.Should().BeFalse();
    }
}
