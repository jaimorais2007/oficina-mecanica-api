using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OficinaApi.Application.UseCases.ServiceOrders;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Interfaces;
using Xunit;

namespace Unit.Tests;

public class ServiceOrderApprovedUseCaseTests
{
    private readonly Mock<IServiceOrderRepository> _repositoryMock;
    private readonly Mock<ILogger<ServiceOrderApprovedUseCase>> _loggerMock;
    private readonly ServiceOrderApprovedUseCase _sut;

    public ServiceOrderApprovedUseCaseTests()
    {
        _repositoryMock = new Mock<IServiceOrderRepository>();
        _loggerMock = new Mock<ILogger<ServiceOrderApprovedUseCase>>();
        _sut = new ServiceOrderApprovedUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    // --- Helpers ---

    private static Customer CreateCustomer()
        => new("João Silva", PersonType.Individual, "529.982.247-25", new DateTime(1990, 1, 1), "teste@gmail.com");

    private static Vehicle CreateVehicle(Customer customer)
        => new(customer, "ABC1234", "Toyota", "Corolla", 2020);

    private static Service CreateService()
        => new("Troca de óleo", "Troca de óleo do motor", 150m);

    private static Part CreatePart(int stock = 10)
        => new("Filtro de óleo", "FO-001", stock, 45m);

    private static ServiceOrder CreateServiceOrderAtExecuting()
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var order = new ServiceOrder(customer, vehicle, [CreateService()]);
        order.StartDiagnostics();
        order.FinishAnalysis();
        order.ApproveServiceOrder();
        return order;
    }

    private static ServiceOrder CreateServiceOrderWithPart(int stock = 10, int quantity = 2)
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var order = new ServiceOrder(customer, vehicle, [CreateService()]);
        order.AddPart(CreatePart(stock), quantity);
        order.StartDiagnostics();
        order.FinishAnalysis();
        order.ApproveServiceOrder();
        return order;
    }

    private void VerifyLogCalled(LogLevel level, Times times)
    {
        _loggerMock.Verify(
            l => l.Log(
                It.Is<LogLevel>(ll => ll == level),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoOrdemDeServicoNaoEncontrada_DeveLogarAvisoERetornarFalha()
    {
        // Arrange
        var evento = new ServiceOrderApprovedEvent(Guid.NewGuid());
        _repositoryMock
            .Setup(r => r.GetByIdWithPartsDetailsAsync(evento.ServiceOrderId))
            .ReturnsAsync((ServiceOrder?)null);

        // Act
        var result = await _sut.ExecuteAsync(evento);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<ServiceOrder>()), Times.Never);
        VerifyLogCalled(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_QuandoOrdemDeServicoSemPecas_DeveChamarSaveChangesELogarInformacaoERetornarSucesso()
    {
        // Arrange
        var serviceOrder = CreateServiceOrderAtExecuting();
        var evento = new ServiceOrderApprovedEvent(serviceOrder.Id);
        _repositoryMock
            .Setup(r => r.GetByIdWithPartsDetailsAsync(evento.ServiceOrderId))
            .ReturnsAsync(serviceOrder);

        // Act
        var result = await _sut.ExecuteAsync(evento);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.SaveChangesAsync(serviceOrder), Times.Once);
        VerifyLogCalled(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_QuandoPecasComEstoqueSuficiente_DeveGarantirEstoqueEmTodasAsPecasESalvarERetornarSucesso()
    {
        // Arrange
        var serviceOrder = CreateServiceOrderWithPart(stock: 10, quantity: 3);
        var evento = new ServiceOrderApprovedEvent(serviceOrder.Id);
        _repositoryMock
            .Setup(r => r.GetByIdWithPartsDetailsAsync(evento.ServiceOrderId))
            .ReturnsAsync(serviceOrder);

        // Act
        var result = await _sut.ExecuteAsync(evento);

        // Assert
        result.IsSuccess.Should().BeTrue();
        serviceOrder.PartsUsed.Should().AllSatisfy(p => p.StockQuantityWasEnsured.Should().BeTrue());
        _repositoryMock.Verify(r => r.SaveChangesAsync(serviceOrder), Times.Once);
        VerifyLogCalled(LogLevel.Error, Times.Never());
    }

    [Fact]
    public async Task ExecuteAsync_QuandoEnsureStockQuantityLancaInvalidOperationException_DeveLogarErroESalvarMesmoAssimERetornarSucesso()
    {
        // Arrange
        var serviceOrder = CreateServiceOrderWithPart(stock: 10, quantity: 2);
        serviceOrder.PartsUsed.First().EnsureStockQuantity(); // StockQuantityWasEnsured = true

        var evento = new ServiceOrderApprovedEvent(serviceOrder.Id);
        _repositoryMock
            .Setup(r => r.GetByIdWithPartsDetailsAsync(evento.ServiceOrderId))
            .ReturnsAsync(serviceOrder);

        // Act
        var result = await _sut.ExecuteAsync(evento);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.SaveChangesAsync(serviceOrder), Times.Once);
        VerifyLogCalled(LogLevel.Error, Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_QuandoEstoqueInsuficiente_DeveLogarErroESalvarMesmoAssimERetornarSucesso()
    {
        // Arrange
        var serviceOrder = CreateServiceOrderWithPart(stock: 1, quantity: 5);
        var evento = new ServiceOrderApprovedEvent(serviceOrder.Id);
        _repositoryMock
            .Setup(r => r.GetByIdWithPartsDetailsAsync(evento.ServiceOrderId))
            .ReturnsAsync(serviceOrder);

        // Act
        var result = await _sut.ExecuteAsync(evento);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.SaveChangesAsync(serviceOrder), Times.Once);
        VerifyLogCalled(LogLevel.Error, Times.Once());
    }
}
