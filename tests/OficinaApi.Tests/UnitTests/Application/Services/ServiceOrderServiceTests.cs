using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Interfaces;
using Xunit;
using AppServiceOrderService = OficinaApi.Application.Services.ServiceOrderService;

namespace OficinaApi.Tests.UnitTests.Application.Services;

public class ServiceOrderServiceTests
{
    private readonly Mock<IServiceOrderRepository> _serviceOrderRepoMock;
    private readonly Mock<IVehicleRepository> _vehicleRepoMock;
    private readonly Mock<IServiceRepository> _serviceRepoMock;
    private readonly Mock<IPartRepository> _partRepoMock;
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly AppServiceOrderService _sut;

    public ServiceOrderServiceTests()
    {
        _serviceOrderRepoMock = new Mock<IServiceOrderRepository>();
        _vehicleRepoMock      = new Mock<IVehicleRepository>();
        _serviceRepoMock      = new Mock<IServiceRepository>();
        _partRepoMock         = new Mock<IPartRepository>();
        _customerRepoMock     = new Mock<ICustomerRepository>();

        _sut = new AppServiceOrderService(
            _serviceOrderRepoMock.Object,
            _vehicleRepoMock.Object,
            _serviceRepoMock.Object,
            _partRepoMock.Object,
            _customerRepoMock.Object
        );
    }

    private static Customer CreateCustomer()
        => new("João Silva", PersonType.Individual, "529.982.247-25", new DateTime(1990, 1, 1));

    private static Vehicle CreateVehicle(Customer customer)
        => new(customer, "ABC1234", "Toyota", "Corolla", 2020);

    private static Service CreateService()
        => new("Troca de óleo", "Troca completa de óleo do motor", 150m);

    private static ServiceOrder CreateServiceOrder(Customer? customer = null, Vehicle? vehicle = null, Service? service = null)
    {
        customer ??= CreateCustomer();
        vehicle  ??= CreateVehicle(customer);
        service  ??= CreateService();
        return new ServiceOrder(customer, vehicle, new[] { service });
    }

    [Fact]
    public async Task CreateServiceOrderAsync_ShouldThrow_WhenVehicleIdIsEmpty()
    {
        // Arrange
        var dto = new CreateServiceOrderDto { VehicleId = Guid.Empty, ServicesUsed = [Guid.NewGuid()] };

        // Act
        Func<Task> act = () => _sut.CreateServiceOrderAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Veículo da Ordem de Serviço não informado*");
    }

    [Fact]
    public async Task CreateServiceOrderAsync_ShouldThrow_WhenServicesIsEmpty()
    {
        // Arrange
        var dto = new CreateServiceOrderDto { VehicleId = Guid.NewGuid(), ServicesUsed = [] };

        // Act
        Func<Task> act = () => _sut.CreateServiceOrderAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Serviços que serão feitos não foram informados*");
    }

    [Fact]
    public async Task CreateServiceOrderAsync_ShouldThrow_WhenVehicleNotFound()
    {
        // Arrange
        var dto = new CreateServiceOrderDto { VehicleId = Guid.NewGuid(), ServicesUsed = [Guid.NewGuid()] };
        _vehicleRepoMock.Setup(r => r.GetByIdAsync(dto.VehicleId)).ReturnsAsync((Vehicle?)null);

        // Act
        Func<Task> act = () => _sut.CreateServiceOrderAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Veículo não encontrado*");
    }

    [Fact]
    public async Task CreateServiceOrderAsync_ShouldThrow_WhenCustomerNotFound()
    {
        // Arrange
        var customer = CreateCustomer();
        var vehicle  = CreateVehicle(customer);
        var dto = new CreateServiceOrderDto
        {
            VehicleId  = vehicle.Id,
            CustomerId = Guid.NewGuid(),
            ServicesUsed = [Guid.NewGuid()]
        };
        _vehicleRepoMock.Setup(r => r.GetByIdAsync(dto.VehicleId)).ReturnsAsync(vehicle);
        _customerRepoMock.Setup(r => r.GetByIdAsync(dto.CustomerId)).ReturnsAsync((Customer?)null);

        // Act
        Func<Task> act = () => _sut.CreateServiceOrderAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Cliente não encontrado*");
    }

    [Fact]
    public async Task CreateServiceOrderAsync_ShouldThrow_WhenSomeServicesNotFound()
    {
        // Arrange
        var customer   = CreateCustomer();
        var vehicle    = CreateVehicle(customer);
        var serviceIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var dto = new CreateServiceOrderDto
        {
            VehicleId    = vehicle.Id,
            CustomerId   = customer.Id,
            ServicesUsed = serviceIds
        };
        _vehicleRepoMock.Setup(r => r.GetByIdAsync(dto.VehicleId)).ReturnsAsync(vehicle);
        _customerRepoMock.Setup(r => r.GetByIdAsync(dto.CustomerId)).ReturnsAsync(customer);
        _serviceRepoMock.Setup(r => r.GetByIdListAsync(serviceIds))
                        .ReturnsAsync(new[] { CreateService() }); // apenas 1 dos 2 encontrado

        // Act
        Func<Task> act = () => _sut.CreateServiceOrderAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Algum dos serviços informados não foi encontrado*");
    }

    [Fact]
    public async Task CreateServiceOrderAsync_ShouldReturnDto_WhenSuccessful()
    {
        // Arrange
        var customer  = CreateCustomer();
        var vehicle   = CreateVehicle(customer);
        var service   = CreateService();
        var serviceId = service.Id;
        var dto = new CreateServiceOrderDto
        {
            VehicleId    = vehicle.Id,
            CustomerId   = customer.Id,
            ServicesUsed = [serviceId]
        };
        _vehicleRepoMock.Setup(r => r.GetByIdAsync(dto.VehicleId)).ReturnsAsync(vehicle);
        _customerRepoMock.Setup(r => r.GetByIdAsync(dto.CustomerId)).ReturnsAsync(customer);
        _serviceRepoMock.Setup(r => r.GetByIdListAsync(dto.ServicesUsed)).ReturnsAsync(new[] { service });

        // Act
        var result = await _sut.CreateServiceOrderAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customer.Id);
        result.VehicleId.Should().Be(vehicle.Id);
        _serviceOrderRepoMock.Verify(r => r.AddAsync(It.IsAny<ServiceOrder>()), Times.Once);
    }

    [Fact]
    public async Task GetServiceOrderByIdAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);

        // Act
        Func<Task> act = () => _sut.GetServiceOrderByIdAsync(id);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ordem de serviço não encontrada*");
    }

    [Fact]
    public async Task GetServiceOrderByIdAsync_ShouldReturnDto_WhenFound()
    {
        // Arrange
        var order = CreateServiceOrder();
        _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        // Act
        var result = await _sut.GetServiceOrderByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetAllServiceOrdersAsync_ShouldReturnAllOrders()
    {
        // Arrange
        var orders = new[] { CreateServiceOrder(), CreateServiceOrder() };
        _serviceOrderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

        // Act
        var result = await _sut.GetAllServiceOrdersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task StartDiagnosticsAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceOrderRepoMock.Setup(r => r.GetByIdForUpdateAsync(id)).ReturnsAsync((ServiceOrder?)null);

        // Act
        Func<Task> act = () => _sut.StartDiagnosticsAsync(id);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ordem de serviço não encontrada*");
    }

    [Fact]
    public async Task StartDiagnosticsAsync_ShouldTransitionToInDiagnostics_WhenOrderIsReceived()
    {
        // Arrange
        var order = CreateServiceOrder(); // status inicial: Received
        _serviceOrderRepoMock.Setup(r => r.GetByIdForUpdateAsync(order.Id)).ReturnsAsync(order);

        // Act
        var result = await _sut.StartDiagnosticsAsync(order.Id);

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.InDiagnostics);
        _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddPartToServiceOrderAsync_ShouldThrow_WhenQuantityIsNotPositive(int quantity)
    {
        // Arrange
        var dto = new AddPartDto { PartId = Guid.NewGuid(), Quantity = quantity };

        // Act
        Func<Task> act = () => _sut.AddPartToServiceOrderAsync(Guid.NewGuid(), dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*quantidade deve ser maior que zero*");
    }

    [Fact]
    public async Task AddPartToServiceOrderAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Arrange
        var id  = Guid.NewGuid();
        var dto = new AddPartDto { PartId = Guid.NewGuid(), Quantity = 1 };
        _serviceOrderRepoMock.Setup(r => r.GetByIdForUpdateAsync(id)).ReturnsAsync((ServiceOrder?)null);

        // Act
        Func<Task> act = () => _sut.AddPartToServiceOrderAsync(id, dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ordem de serviço não encontrada*");
    }

    [Fact]
    public async Task AddPartToServiceOrderAsync_ShouldThrow_WhenPartNotFound()
    {
        // Arrange
        var order = CreateServiceOrder();
        var dto   = new AddPartDto { PartId = Guid.NewGuid(), Quantity = 1 };
        _serviceOrderRepoMock.Setup(r => r.GetByIdForUpdateAsync(order.Id)).ReturnsAsync(order);
        _partRepoMock.Setup(r => r.GetByIdAsync(dto.PartId)).ReturnsAsync((Part?)null);

        // Act
        Func<Task> act = () => _sut.AddPartToServiceOrderAsync(order.Id, dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Peça não encontrada*");
    }

    [Fact]
    public async Task AddPartToServiceOrderAsync_ShouldAddPart_WhenSuccessful()
    {
        // Arrange
        var order = CreateServiceOrder();
        var part  = new Part("Filtro de ar", "FA-01", 50, 30m);
        var dto   = new AddPartDto { PartId = part.Id, Quantity = 2 };
        _serviceOrderRepoMock.Setup(r => r.GetByIdForUpdateAsync(order.Id)).ReturnsAsync(order);
        _partRepoMock.Setup(r => r.GetByIdAsync(part.Id)).ReturnsAsync(part);

        // Act
        var result = await _sut.AddPartToServiceOrderAsync(order.Id, dto);

        // Assert
        result.Should().NotBeNull();
        order.PartsUsed.Should().HaveCount(1);
        _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
    }

    [Fact]
    public async Task AddServiceToServiceOrderAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Arrange
        var id  = Guid.NewGuid();
        var dto = new AddServiceDto { ServiceId = Guid.NewGuid() };
        _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);

        // Act
        Func<Task> act = () => _sut.AddServiceToServiceOrderAsync(id, dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ordem de serviço não encontrada*");
    }

    [Fact]
    public async Task AddServiceToServiceOrderAsync_ShouldThrow_WhenServiceNotFound()
    {
        // Arrange
        var order = CreateServiceOrder();
        var dto   = new AddServiceDto { ServiceId = Guid.NewGuid() };
        _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _serviceRepoMock.Setup(r => r.GetByIdAsync(dto.ServiceId)).ReturnsAsync((Service?)null);

        // Act
        Func<Task> act = () => _sut.AddServiceToServiceOrderAsync(order.Id, dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Serviço não encontrado*");
    }

    [Fact]
    public async Task AddServiceToServiceOrderAsync_ShouldAddService_WhenSuccessful()
    {
        // Arrange
        var order   = CreateServiceOrder();
        var service = new Service("Alinhamento", "Alinhamento de direção", 80m);
        var dto     = new AddServiceDto { ServiceId = service.Id };
        _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _serviceRepoMock.Setup(r => r.GetByIdAsync(service.Id)).ReturnsAsync(service);

        // Act
        var result = await _sut.AddServiceToServiceOrderAsync(order.Id, dto);

        // Assert
        result.Should().NotBeNull();
        order.ServicesUsed.Should().HaveCount(2); // 1 do construtor + 1 adicionado
        _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
    }

    [Fact]
    public async Task FinishAnalysisAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);

        // Act
        Func<Task> act = () => _sut.FinishAnalysisAsync(id);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ordem de serviço não encontrada*");
    }

    [Fact]
    public async Task FinishAnalysisAsync_ShouldTransitionToWaitingApproval_WhenOrderIsInDiagnostics()
    {
        // Arrange
        var order = CreateServiceOrder();
        order.StartDiagnostics(); // Received → InDiagnostics
        _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        // Act
        var result = await _sut.FinishAnalysisAsync(order.Id);

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.WaitingApproval);
        _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
    }

    [Fact]
    public async Task ApproveServiceOrderAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);

        // Act
        Func<Task> act = () => _sut.ApproveServiceOrderAsync(id);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ordem de serviço não encontrada*");
    }

    [Fact]
    public async Task ApproveServiceOrderAsync_ShouldTransitionToExecuting_WhenOrderIsWaitingApproval()
    {
        // Arrange
        var order = CreateServiceOrder();
        order.StartDiagnostics(); // Received → InDiagnostics
        order.FinishAnalysis();   // InDiagnostics → WaitingApproval
        _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        // Act
        var result = await _sut.ApproveServiceOrderAsync(order.Id);

        // Assert
        order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Executing);
        _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
    }

    [Fact]
    public async Task FinishExecutionAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);

        // Act
        Func<Task> act = () => _sut.FinishExecutionAsync(id);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ordem de serviço não encontrada*");
    }

    [Fact]
    public async Task DeliverServiceOrderAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);

        // Act
        Func<Task> act = () => _sut.DeliverServiceOrderAsync(id);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ordem de serviço não encontrada*");
    }

    [Fact]
    public async Task GetServiceOrderPeddingStocksAsync_ShouldThrow_WhenOrderNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceOrderRepoMock.Setup(r => r.GetServiceOrderByIdToGetPeddingStocksAsync(id))
                             .ReturnsAsync((ServiceOrder?)null);

        // Act
        Func<Task> act = () => _sut.GetServiceOrderPeddingStocksAsync(id);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ordem de serviço não encontrada*");
    }

    [Fact]
    public async Task GetAverageDurationInDaysAsync_ShouldReturnValueFromRepository()
    {
        // Arrange
        const double expected = 4.5;
        _serviceOrderRepoMock.Setup(r => r.GetAverageDurationInDaysAsync()).ReturnsAsync(expected);

        // Act
        var result = await _sut.GetAverageDurationInDaysAsync();

        // Assert
        result.Should().Be(expected);
    }
}
