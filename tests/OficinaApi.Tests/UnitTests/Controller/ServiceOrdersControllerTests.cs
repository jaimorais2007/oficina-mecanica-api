using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Presentation.Controllers;
using Xunit;

namespace OficinaApi.Tests.UnitTests.Controller;

public class ServiceOrdersControllerTests
{
    private readonly Mock<IUseCase<NoInput, IEnumerable<ServiceOrderDto>>> _getAllServiceOrdersUseCaseMock;
    private readonly Mock<IUseCase<Guid, ServiceOrderDto?>> _getServiceOrderByIdUseCaseMock;
    private readonly Mock<IUseCase<Guid, ServiceOrderStatusDto?>> _getServiceOrderByStatusUseCaseMock;
    private readonly Mock<IUseCase<CreateServiceOrderDto, ServiceOrderDto>> _createServiceOrderUseCaseMock;
    private readonly Mock<IUseCase<StartDiagnosticsRequest, ServiceOrderDto>> _startDiagnosticsUseCaseMock;
    private readonly Mock<IUseCase<FinishAnalysisRequest, ServiceOrderDto>> _finishAnalysisUseCaseMock;
    private readonly Mock<IUseCase<AddPartToServiceOrderRequest, ServiceOrderDto>> _addPartToServiceOrderUseCaseMock;
    private readonly Mock<IUseCase<AddServiceToServiceOrderRequest, ServiceOrderDto>> _addServiceToServiceOrderUseCaseMock;
    private readonly Mock<IUseCase<ApproveServiceOrderRequest, ServiceOrderDto>> _approveServiceOrderUseCaseMock;
    private readonly Mock<IUseCase<FinishExecutionRequest, ServiceOrderDto>> _finishExecutionUseCaseMock;
    private readonly Mock<IUseCase<DeliverServiceOrderRequest, ServiceOrderDto>> _deliverServiceOrderUseCaseMock;
    private readonly Mock<IUseCase<RefuseServiceOrderRequest, ServiceOrderDto>> _refuseServiceOrderUseCaseMock;
    private readonly Mock<IUseCase<Guid, IEnumerable<ServiceOrderPeddingStockDto>>> _getServiceOrderPendingStocksUseCaseMock;
    private readonly Mock<IUseCase<NoInput, double>> _getAverageDurationUseCaseMock;

    private readonly ServiceOrdersController _controller;

    public ServiceOrdersControllerTests()
    {
        _getAllServiceOrdersUseCaseMock = new Mock<IUseCase<NoInput, IEnumerable<ServiceOrderDto>>>();
        _getServiceOrderByIdUseCaseMock = new Mock<IUseCase<Guid, ServiceOrderDto?>>();
        _getServiceOrderByStatusUseCaseMock = new Mock<IUseCase<Guid, ServiceOrderStatusDto?>>();
        _createServiceOrderUseCaseMock = new Mock<IUseCase<CreateServiceOrderDto, ServiceOrderDto>>();
        _startDiagnosticsUseCaseMock = new Mock<IUseCase<StartDiagnosticsRequest, ServiceOrderDto>>();
        _finishAnalysisUseCaseMock = new Mock<IUseCase<FinishAnalysisRequest, ServiceOrderDto>>();
        _addPartToServiceOrderUseCaseMock = new Mock<IUseCase<AddPartToServiceOrderRequest, ServiceOrderDto>>();
        _addServiceToServiceOrderUseCaseMock = new Mock<IUseCase<AddServiceToServiceOrderRequest, ServiceOrderDto>>();
        _approveServiceOrderUseCaseMock = new Mock<IUseCase<ApproveServiceOrderRequest, ServiceOrderDto>>();
        _finishExecutionUseCaseMock = new Mock<IUseCase<FinishExecutionRequest, ServiceOrderDto>>();
        _deliverServiceOrderUseCaseMock = new Mock<IUseCase<DeliverServiceOrderRequest, ServiceOrderDto>>();
        _refuseServiceOrderUseCaseMock = new Mock<IUseCase<RefuseServiceOrderRequest, ServiceOrderDto>>();
        _getServiceOrderPendingStocksUseCaseMock = new Mock<IUseCase<Guid, IEnumerable<ServiceOrderPeddingStockDto>>>();
        _getAverageDurationUseCaseMock = new Mock<IUseCase<NoInput, double>>();

        _controller = new ServiceOrdersController(
            _getAllServiceOrdersUseCaseMock.Object,
            _getServiceOrderByIdUseCaseMock.Object,
            _getServiceOrderByStatusUseCaseMock.Object,
            _createServiceOrderUseCaseMock.Object,
            _startDiagnosticsUseCaseMock.Object,
            _finishAnalysisUseCaseMock.Object,
            _addPartToServiceOrderUseCaseMock.Object,
            _addServiceToServiceOrderUseCaseMock.Object,
            _approveServiceOrderUseCaseMock.Object,
            _finishExecutionUseCaseMock.Object,
            _deliverServiceOrderUseCaseMock.Object,
            _getServiceOrderPendingStocksUseCaseMock.Object,
            _getAverageDurationUseCaseMock.Object,
            _refuseServiceOrderUseCaseMock.Object);
    }

    private static ServiceOrder CreateServiceOrder(Guid? id = null)
    {
        var customer = new Customer("João Silva", PersonType.Individual, "529.982.247-25", new DateTime(1990, 1, 1), "teste@gmail.com");
        var vehicle = new Vehicle(customer, "ABC1234", "Toyota", "Corolla", 2020);
        var service = new Service("Troca de óleo", "Descrição", 100m);
        var order = new ServiceOrder(customer, vehicle, new[] { service });
        if (id.HasValue)
        {
            typeof(ServiceOrder).GetProperty("Id")?.SetValue(order, id.Value);
        }
        return order;
    }

    private static ServiceOrderDto CreateSampleDto(Guid? id = null) => new(CreateServiceOrder(id));

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenSuccessful()
    {
        _getAllServiceOrdersUseCaseMock
            .Setup(u => u.ExecuteAsync(It.IsAny<NoInput>()))
            .ReturnsAsync(UseCaseResponse<IEnumerable<ServiceOrderDto>>.Success(new[] { CreateSampleDto() }));

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnBadRequest_WhenFails()
    {
        _getAllServiceOrdersUseCaseMock
            .Setup(u => u.ExecuteAsync(It.IsAny<NoInput>()))
            .ReturnsAsync(UseCaseResponse<IEnumerable<ServiceOrderDto>>.Failure("Erro ao buscar ordens"));

        var result = await _controller.GetAll();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenOrderExists()
    {
        var orderId = Guid.NewGuid();
        var dto = CreateSampleDto(orderId);
        _getServiceOrderByIdUseCaseMock
            .Setup(u => u.ExecuteAsync(orderId))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto?>.Success(dto));

        var result = await _controller.GetById(orderId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenOrderNotFound()
    {
        var orderId = Guid.NewGuid();
        _getServiceOrderByIdUseCaseMock
            .Setup(u => u.ExecuteAsync(orderId))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto?>.Failure("Ordem de serviço não encontrada"));

        var result = await _controller.GetById(orderId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByIdForStatus_ShouldReturnOk_WhenOrderExists()
    {
        var orderId = Guid.NewGuid();
        var statusDto = new ServiceOrderStatusDto
        {
            Id = orderId,
            Status = OficinaApi.Domain.Enums.OrderStatus.Received
        };
        _getServiceOrderByStatusUseCaseMock
            .Setup(u => u.ExecuteAsync(orderId))
            .ReturnsAsync(UseCaseResponse<ServiceOrderStatusDto?>.Success(statusDto));

        var result = await _controller.GetByIdForStatus(orderId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdForStatus_ShouldReturnNotFound_WhenOrderNotFound()
    {
        var orderId = Guid.NewGuid();
        _getServiceOrderByStatusUseCaseMock
            .Setup(u => u.ExecuteAsync(orderId))
            .ReturnsAsync(UseCaseResponse<ServiceOrderStatusDto?>.Failure("Ordem de serviço não encontrada"));

        var result = await _controller.GetByIdForStatus(orderId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenSuccessful()
    {
        var dto = new CreateServiceOrderDto
        {
            CustomerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            ServicesUsed = new List<Guid> { Guid.NewGuid() }
        };
        var responseDto = CreateSampleDto();
        _createServiceOrderUseCaseMock
            .Setup(u => u.ExecuteAsync(dto))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto>.Success(responseDto));

        var result = await _controller.Create(dto);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenFails()
    {
        var dto = new CreateServiceOrderDto();
        _createServiceOrderUseCaseMock
            .Setup(u => u.ExecuteAsync(dto))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto>.Failure("Dados inválidos"));

        var result = await _controller.Create(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MoveToAnalysis_ShouldReturnOk_WhenSuccessful()
    {
        var orderId = Guid.NewGuid();
        var responseDto = CreateSampleDto(orderId);
        _startDiagnosticsUseCaseMock
            .Setup(u => u.ExecuteAsync(It.Is<StartDiagnosticsRequest>(r => r.Id == orderId)))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto>.Success(responseDto));

        var result = await _controller.MoveToAnalysis(orderId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task MoveToAnalysis_ShouldReturnNotFound_WhenOrderNotFound()
    {
        var orderId = Guid.NewGuid();
        _startDiagnosticsUseCaseMock
            .Setup(u => u.ExecuteAsync(It.Is<StartDiagnosticsRequest>(r => r.Id == orderId)))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto>.Failure("Ordem de serviço não encontrada"));

        var result = await _controller.MoveToAnalysis(orderId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task FinishAnalysis_ShouldReturnOk_WhenSuccessful()
    {
        var orderId = Guid.NewGuid();
        var responseDto = CreateSampleDto(orderId);
        _finishAnalysisUseCaseMock
            .Setup(u => u.ExecuteAsync(It.Is<FinishAnalysisRequest>(r => r.Id == orderId)))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto>.Success(responseDto));

        var result = await _controller.FinishAnalysis(orderId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AddPartToServiceOrder_ShouldReturnOk_WhenSuccessful()
    {
        var orderId = Guid.NewGuid();
        var partDto = new AddPartDto { PartId = Guid.NewGuid(), Quantity = 2 };
        var responseDto = CreateSampleDto(orderId);
        _addPartToServiceOrderUseCaseMock
            .Setup(u => u.ExecuteAsync(It.Is<AddPartToServiceOrderRequest>(r => r.Id == orderId)))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto>.Success(responseDto));

        var result = await _controller.AddPartToServiceOrder(orderId, partDto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AddServiceToServiceOrder_ShouldReturnOk_WhenSuccessful()
    {
        var orderId = Guid.NewGuid();
        var serviceDto = new AddServiceDto { ServiceId = Guid.NewGuid() };
        var responseDto = CreateSampleDto(orderId);
        _addServiceToServiceOrderUseCaseMock
            .Setup(u => u.ExecuteAsync(It.Is<AddServiceToServiceOrderRequest>(r => r.Id == orderId)))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto>.Success(responseDto));

        var result = await _controller.AddServiceToServiceOrder(orderId, serviceDto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ApproveServiceOrder_ShouldReturnOk_WhenSuccessful()
    {
        var orderId = Guid.NewGuid();
        var responseDto = CreateSampleDto(orderId);
        _approveServiceOrderUseCaseMock
            .Setup(u => u.ExecuteAsync(It.Is<ApproveServiceOrderRequest>(r => r.Id == orderId)))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto>.Success(responseDto));

        var result = await _controller.ApproveServiceOrder(orderId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task FinishExecution_ShouldReturnOk_WhenSuccessful()
    {
        var orderId = Guid.NewGuid();
        var responseDto = CreateSampleDto(orderId);
        _finishExecutionUseCaseMock
            .Setup(u => u.ExecuteAsync(It.Is<FinishExecutionRequest>(r => r.Id == orderId)))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto>.Success(responseDto));

        var result = await _controller.FinishExecution(orderId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeliverServiceOrder_ShouldReturnOk_WhenSuccessful()
    {
        var orderId = Guid.NewGuid();
        var responseDto = CreateSampleDto(orderId);
        _deliverServiceOrderUseCaseMock
            .Setup(u => u.ExecuteAsync(It.Is<DeliverServiceOrderRequest>(r => r.Id == orderId)))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto>.Success(responseDto));

        var result = await _controller.DeliverServiceOrder(orderId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RefuseServiceOrder_ShouldReturnOk_WhenSuccessful()
    {
        var orderId = Guid.NewGuid();
        var responseDto = CreateSampleDto(orderId);
        _refuseServiceOrderUseCaseMock
            .Setup(u => u.ExecuteAsync(It.Is<RefuseServiceOrderRequest>(r => r.Id == orderId)))
            .ReturnsAsync(UseCaseResponse<ServiceOrderDto>.Success(responseDto));

        var result = await _controller.RefuseServiceOrder(orderId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetServiceOrderPendingStocks_ShouldReturnOk_WhenSuccessful()
    {
        var orderId = Guid.NewGuid();
        _getServiceOrderPendingStocksUseCaseMock
            .Setup(u => u.ExecuteAsync(orderId))
            .ReturnsAsync(UseCaseResponse<IEnumerable<ServiceOrderPeddingStockDto>>.Success(new List<ServiceOrderPeddingStockDto>()));

        var result = await _controller.GetServiceOrderPendingStocks(orderId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAverageDuration_ShouldReturnOk_WhenSuccessful()
    {
        _getAverageDurationUseCaseMock
            .Setup(u => u.ExecuteAsync(It.IsAny<NoInput>()))
            .ReturnsAsync(UseCaseResponse<double>.Success(3.5));

        var result = await _controller.GetAverageDuration();

        result.Should().BeOfType<OkObjectResult>();
    }
}
