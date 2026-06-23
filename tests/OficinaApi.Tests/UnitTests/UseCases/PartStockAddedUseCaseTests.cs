using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OficinaApi.Application.UseCases.Parts;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Interfaces;
using Xunit;

namespace Unit.Tests;

public class PartStockAddedUseCaseTests
{
    private readonly Mock<IPartRepository> _partRepositoryMock;
    private readonly Mock<IServiceOrderPartRepository> _serviceOrderPartRepositoryMock;
    private readonly Mock<ILogger<PartStockAddedUseCase>> _loggerMock;
    private readonly PartStockAddedUseCase _sut;

    public PartStockAddedUseCaseTests()
    {
        _partRepositoryMock = new Mock<IPartRepository>();
        _serviceOrderPartRepositoryMock = new Mock<IServiceOrderPartRepository>();
        _loggerMock = new Mock<ILogger<PartStockAddedUseCase>>();
        
        _sut = new PartStockAddedUseCase(
            _loggerMock.Object,
            _partRepositoryMock.Object,
            _serviceOrderPartRepositoryMock.Object);
    }

    // --- Helpers ---
    private static Part CreatePart(Guid id, int stock = 10)
    {
        var part = new Part("Filtro", "FO-001", stock, 45m);
        typeof(Part).GetProperty("Id")?.SetValue(part, id);
        return part;
    }

    private static ServiceOrder CreateServiceOrder()
    {
        var customer = new Customer("João", PersonType.Individual, "529.982.247-25", new DateTime(1990, 1, 1), "teste@teste.com");
        var vehicle = new Vehicle(customer, "ABC1234", "Toyota", "Corolla", 2020);
        return new ServiceOrder(customer, vehicle, new List<Service>());
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
    public async Task ExecuteAsync_QuandoPecaNaoEncontrada_DeveRetornarFalhaELogarAviso()
    {
        // Arrange
        var partId = Guid.NewGuid();
        var evento = new PartStockAddedEvent(partId);
        
        _partRepositoryMock
            .Setup(r => r.GetByIdWithServiceOrderDetailsAsync(partId))
            .ReturnsAsync((Part?)null);

        // Act
        var result = await _sut.ExecuteAsync(evento);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Messages.Should().Contain($"Peça com ID {partId} não encontrada.");
        _serviceOrderPartRepositoryMock.Verify(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<ServiceOrderPart>>()), Times.Never);
        VerifyLogCalled(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_QuandoNenhumaOSPrecisaDeEstoque_DeveRetornarSucessoELogarDebug()
    {
        // Arrange
        var partId = Guid.NewGuid();
        var part = CreatePart(partId, 10);
        var evento = new PartStockAddedEvent(partId);
        
        _partRepositoryMock
            .Setup(r => r.GetByIdWithServiceOrderDetailsAsync(partId))
            .ReturnsAsync(part);

        // Act
        var result = await _sut.ExecuteAsync(evento);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _serviceOrderPartRepositoryMock.Verify(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<ServiceOrderPart>>()), Times.Never);
        VerifyLogCalled(LogLevel.Debug, Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_QuandoPecasPrecisamDeEstoque_DeveGarantirEAtualizarRepositório()
    {
        // Arrange
        var partId = Guid.NewGuid();
        var part = CreatePart(partId, 10);
        var serviceOrder = CreateServiceOrder();
        serviceOrder.StartDiagnostics();
        serviceOrder.FinishAnalysis();
        serviceOrder.ApproveServiceOrder(); // Agora o status é Executing

        var serviceOrderPart = new ServiceOrderPart(serviceOrder, part, 2);
        
        // Simulating that stock was NOT ensured previously and OS needs it
        part.ServiceOrdersParts.Add(serviceOrderPart);
        
        var evento = new PartStockAddedEvent(partId);
        
        _partRepositoryMock
            .Setup(r => r.GetByIdWithServiceOrderDetailsAsync(partId))
            .ReturnsAsync(part);

        // Act
        var result = await _sut.ExecuteAsync(evento);

        // Assert
        result.IsSuccess.Should().BeTrue();
        serviceOrderPart.StockQuantityWasEnsured.Should().BeTrue();
        _serviceOrderPartRepositoryMock.Verify(r => r.UpdateRangeAsync(It.Is<IEnumerable<ServiceOrderPart>>(parts => parts.Contains(serviceOrderPart))), Times.Once);
        VerifyLogCalled(LogLevel.Information, Times.Once());
    }
}
