using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.UseCases.ServiceOrders;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Interfaces;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Unit.Tests.Application.UseCases.ServiceOrders
{
    public class ServiceOrderUseCaseTests
    {
        private readonly Mock<IServiceOrderRepository> _serviceOrderRepoMock;
        private readonly Mock<IVehicleRepository> _vehicleRepoMock;
        private readonly Mock<IServiceRepository> _serviceRepoMock;
        private readonly Mock<IPartRepository> _partRepoMock;
        private readonly Mock<ICustomerRepository> _customerRepoMock;
        private readonly Mock<IApplicationMetrics> _applicationMetricsMock;

        public ServiceOrderUseCaseTests()
        {
            _serviceOrderRepoMock = new Mock<IServiceOrderRepository>();
            _vehicleRepoMock      = new Mock<IVehicleRepository>();
            _serviceRepoMock      = new Mock<IServiceRepository>();
            _partRepoMock         = new Mock<IPartRepository>();
            _customerRepoMock     = new Mock<ICustomerRepository>();
            _applicationMetricsMock = new Mock<IApplicationMetrics>();
        }

        private static Customer CreateCustomer()
            => new("João Silva", PersonType.Individual, "529.982.247-25", new DateTime(1990, 1, 1), "teste@gmail.com");

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
        public async Task CreateServiceOrderUseCase_ShouldReturnFailure_WhenVehicleIdIsEmpty()
        {
            var dto = new CreateServiceOrderDto { VehicleId = Guid.Empty, ServicesUsed = [Guid.NewGuid()] };
            var useCase = new CreateServiceOrderUseCase(_serviceOrderRepoMock.Object, _vehicleRepoMock.Object, _serviceRepoMock.Object, _customerRepoMock.Object, Mock.Of<ILogger<CreateServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Veículo da Ordem de Serviço não informado"));
        }

        [Fact]
        public async Task CreateServiceOrderUseCase_ShouldReturnFailure_WhenServicesIsEmpty()
        {
            var dto = new CreateServiceOrderDto { VehicleId = Guid.NewGuid(), ServicesUsed = [] };
            var useCase = new CreateServiceOrderUseCase(_serviceOrderRepoMock.Object, _vehicleRepoMock.Object, _serviceRepoMock.Object, _customerRepoMock.Object, Mock.Of<ILogger<CreateServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Serviços que serão feitos não foram informados"));
        }

        [Fact]
        public async Task CreateServiceOrderUseCase_ShouldReturnFailure_WhenVehicleNotFound()
        {
            var dto = new CreateServiceOrderDto { VehicleId = Guid.NewGuid(), ServicesUsed = [Guid.NewGuid()] };
            _vehicleRepoMock.Setup(r => r.GetByIdAsync(dto.VehicleId)).ReturnsAsync((Vehicle?)null);
            var useCase = new CreateServiceOrderUseCase(_serviceOrderRepoMock.Object, _vehicleRepoMock.Object, _serviceRepoMock.Object, _customerRepoMock.Object, Mock.Of<ILogger<CreateServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Veículo não encontrado"));
        }

        [Fact]
        public async Task CreateServiceOrderUseCase_ShouldReturnFailure_WhenCustomerNotFound()
        {
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
            var useCase = new CreateServiceOrderUseCase(_serviceOrderRepoMock.Object, _vehicleRepoMock.Object, _serviceRepoMock.Object, _customerRepoMock.Object, Mock.Of<ILogger<CreateServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Cliente não encontrado"));
        }

        [Fact]
        public async Task CreateServiceOrderUseCase_ShouldReturnFailure_WhenSomeServicesNotFound()
        {
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
                            .ReturnsAsync(new[] { CreateService() });

            var useCase = new CreateServiceOrderUseCase(_serviceOrderRepoMock.Object, _vehicleRepoMock.Object, _serviceRepoMock.Object, _customerRepoMock.Object, Mock.Of<ILogger<CreateServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Algum dos serviços informados não foi encontrado"));
        }

        [Fact]
        public async Task CreateServiceOrderUseCase_ShouldReturnDto_WhenSuccessful()
        {
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

            var useCase = new CreateServiceOrderUseCase(_serviceOrderRepoMock.Object, _vehicleRepoMock.Object, _serviceRepoMock.Object, _customerRepoMock.Object, Mock.Of<ILogger<CreateServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(dto);

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().NotBeNull();
            result.Response.CustomerId.Should().Be(customer.Id);
            result.Response.VehicleId.Should().Be(vehicle.Id);
            _serviceOrderRepoMock.Verify(r => r.AddAsync(It.IsAny<ServiceOrder>()), Times.Once);
        }

        [Fact]
        public async Task GetServiceOrderByIdUseCase_ShouldReturnFailure_WhenOrderNotFound()
        {
            var id = Guid.NewGuid();
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);
            var useCase = new GetServiceOrderByIdUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<GetServiceOrderByIdUseCase>>());

            var result = await useCase.ExecuteAsync(id);

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Ordem de serviço não encontrada"));
        }

        [Fact]
        public async Task GetServiceOrderByIdUseCase_ShouldReturnDto_WhenFound()
        {
            var order = CreateServiceOrder();
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
            var useCase = new GetServiceOrderByIdUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<GetServiceOrderByIdUseCase>>());

            var result = await useCase.ExecuteAsync(order.Id);

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().NotBeNull();
            result.Response!.Id.Should().Be(order.Id);
        }

        [Fact]
        public async Task GetAllServiceOrdersUseCase_ShouldReturnAllOrders()
        {
            var orders = new[] { CreateServiceOrder(), CreateServiceOrder() };
            _serviceOrderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
            var useCase = new GetAllServiceOrdersUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<GetAllServiceOrdersUseCase>>());

            var result = await useCase.ExecuteAsync(new NoInput());

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().HaveCount(2);
        }

        [Fact]
        public async Task StartDiagnosticsUseCase_ShouldReturnFailure_WhenOrderNotFound()
        {
            var id = Guid.NewGuid();
            _serviceOrderRepoMock.Setup(r => r.GetByIdForUpdateAsync(id)).ReturnsAsync((ServiceOrder?)null);
            var useCase = new StartDiagnosticsUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<StartDiagnosticsUseCase>>(), _applicationMetricsMock.Object);

            var result = await useCase.ExecuteAsync(new StartDiagnosticsRequest(id));

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Ordem de serviço não encontrada"));
        }

        [Fact]
        public async Task StartDiagnosticsUseCase_ShouldTransitionToInDiagnostics_WhenOrderIsReceived()
        {
            var order = CreateServiceOrder();
            _serviceOrderRepoMock.Setup(r => r.GetByIdForUpdateAsync(order.Id)).ReturnsAsync(order);
            var useCase = new StartDiagnosticsUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<StartDiagnosticsUseCase>>(), _applicationMetricsMock.Object);

            var result = await useCase.ExecuteAsync(new StartDiagnosticsRequest(order.Id));

            result.IsSuccess.Should().BeTrue();
            order.GetLastStatusHistory().Status.Should().Be(OrderStatus.InDiagnostics);
            _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task AddPartToServiceOrderUseCase_ShouldReturnFailure_WhenQuantityIsNotPositive(int quantity)
        {
            var dto = new AddPartDto { PartId = Guid.NewGuid(), Quantity = quantity };
            var useCase = new AddPartToServiceOrderUseCase(_serviceOrderRepoMock.Object, _partRepoMock.Object, Mock.Of<ILogger<AddPartToServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(new AddPartToServiceOrderRequest(Guid.NewGuid(), dto));

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("quantidade deve ser maior que zero"));
        }

        [Fact]
        public async Task AddPartToServiceOrderUseCase_ShouldReturnFailure_WhenOrderNotFound()
        {
            var id  = Guid.NewGuid();
            var dto = new AddPartDto { PartId = Guid.NewGuid(), Quantity = 1 };
            _serviceOrderRepoMock.Setup(r => r.GetByIdForUpdateAsync(id)).ReturnsAsync((ServiceOrder?)null);
            var useCase = new AddPartToServiceOrderUseCase(_serviceOrderRepoMock.Object, _partRepoMock.Object, Mock.Of<ILogger<AddPartToServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(new AddPartToServiceOrderRequest(id, dto));

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Ordem de serviço não encontrada"));
        }

        [Fact]
        public async Task AddPartToServiceOrderUseCase_ShouldReturnFailure_WhenPartNotFound()
        {
            var order = CreateServiceOrder();
            var dto   = new AddPartDto { PartId = Guid.NewGuid(), Quantity = 1 };
            _serviceOrderRepoMock.Setup(r => r.GetByIdForUpdateAsync(order.Id)).ReturnsAsync(order);
            _partRepoMock.Setup(r => r.GetByIdAsync(dto.PartId)).ReturnsAsync((Part?)null);
            var useCase = new AddPartToServiceOrderUseCase(_serviceOrderRepoMock.Object, _partRepoMock.Object, Mock.Of<ILogger<AddPartToServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(new AddPartToServiceOrderRequest(order.Id, dto));

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Peça não encontrada"));
        }

        [Fact]
        public async Task AddPartToServiceOrderUseCase_ShouldAddPart_WhenSuccessful()
        {
            var order = CreateServiceOrder();
            var part  = new Part("Filtro de ar", "FA-01", 50, 30m);
            var dto   = new AddPartDto { PartId = part.Id, Quantity = 2 };
            _serviceOrderRepoMock.Setup(r => r.GetByIdForUpdateAsync(order.Id)).ReturnsAsync(order);
            _partRepoMock.Setup(r => r.GetByIdAsync(part.Id)).ReturnsAsync(part);
            var useCase = new AddPartToServiceOrderUseCase(_serviceOrderRepoMock.Object, _partRepoMock.Object, Mock.Of<ILogger<AddPartToServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(new AddPartToServiceOrderRequest(order.Id, dto));

            result.IsSuccess.Should().BeTrue();
            order.PartsUsed.Should().HaveCount(1);
            _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
        }

        [Fact]
        public async Task AddServiceToServiceOrderUseCase_ShouldReturnFailure_WhenOrderNotFound()
        {
            var id  = Guid.NewGuid();
            var dto = new AddServiceDto { ServiceId = Guid.NewGuid() };
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);
            var useCase = new AddServiceToServiceOrderUseCase(_serviceOrderRepoMock.Object, _serviceRepoMock.Object, Mock.Of<ILogger<AddServiceToServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(new AddServiceToServiceOrderRequest(id, dto));

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Ordem de serviço não encontrada"));
        }

        [Fact]
        public async Task AddServiceToServiceOrderUseCase_ShouldReturnFailure_WhenServiceNotFound()
        {
            var order = CreateServiceOrder();
            var dto   = new AddServiceDto { ServiceId = Guid.NewGuid() };
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
            _serviceRepoMock.Setup(r => r.GetByIdAsync(dto.ServiceId)).ReturnsAsync((Service?)null);
            var useCase = new AddServiceToServiceOrderUseCase(_serviceOrderRepoMock.Object, _serviceRepoMock.Object, Mock.Of<ILogger<AddServiceToServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(new AddServiceToServiceOrderRequest(order.Id, dto));

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Serviço não encontrado"));
        }

        [Fact]
        public async Task AddServiceToServiceOrderUseCase_ShouldAddService_WhenSuccessful()
        {
            var order   = CreateServiceOrder();
            var service = new Service("Alinhamento", "Alinhamento de direção", 80m);
            var dto     = new AddServiceDto { ServiceId = service.Id };
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
            _serviceRepoMock.Setup(r => r.GetByIdAsync(service.Id)).ReturnsAsync(service);
            var useCase = new AddServiceToServiceOrderUseCase(_serviceOrderRepoMock.Object, _serviceRepoMock.Object, Mock.Of<ILogger<AddServiceToServiceOrderUseCase>>());

            var result = await useCase.ExecuteAsync(new AddServiceToServiceOrderRequest(order.Id, dto));

            result.IsSuccess.Should().BeTrue();
            order.ServicesUsed.Should().HaveCount(2);
            _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
        }

        [Fact]
        public async Task FinishAnalysisUseCase_ShouldReturnFailure_WhenOrderNotFound()
        {
            var id = Guid.NewGuid();
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);
            var useCase = new FinishAnalysisUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<FinishAnalysisUseCase>>(), _applicationMetricsMock.Object);

            var result = await useCase.ExecuteAsync(new FinishAnalysisRequest(id));

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Ordem de serviço não encontrada"));
        }

        [Fact]
        public async Task FinishAnalysisUseCase_ShouldTransitionToWaitingApproval_WhenOrderIsInDiagnostics()
        {
            var order = CreateServiceOrder();
            order.StartDiagnostics();
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
            var useCase = new FinishAnalysisUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<FinishAnalysisUseCase>>(), _applicationMetricsMock.Object);

            var result = await useCase.ExecuteAsync(new FinishAnalysisRequest(order.Id));

            result.IsSuccess.Should().BeTrue();
            order.GetLastStatusHistory().Status.Should().Be(OrderStatus.WaitingApproval);
            _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
        }

        [Fact]
        public async Task ApproveServiceOrderUseCase_ShouldReturnFailure_WhenOrderNotFound()
        {
            var id = Guid.NewGuid();
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);
            var useCase = new ApproveServiceOrderUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<ApproveServiceOrderUseCase>>(), _applicationMetricsMock.Object);

            var result = await useCase.ExecuteAsync(new ApproveServiceOrderRequest(id));

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Ordem de serviço não encontrada"));
        }

        [Fact]
        public async Task ApproveServiceOrderUseCase_ShouldTransitionToExecuting_WhenOrderIsWaitingApproval()
        {
            var order = CreateServiceOrder();
            order.StartDiagnostics();
            order.FinishAnalysis();
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
            var useCase = new ApproveServiceOrderUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<ApproveServiceOrderUseCase>>(), _applicationMetricsMock.Object);

            var result = await useCase.ExecuteAsync(new ApproveServiceOrderRequest(order.Id));

            result.IsSuccess.Should().BeTrue();
            order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Executing);
            _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
        }

        [Fact]
        public async Task FinishExecutionUseCase_ShouldReturnFailure_WhenOrderNotFound()
        {
            var id = Guid.NewGuid();
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);
            var useCase = new FinishExecutionUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<FinishExecutionUseCase>>(), _applicationMetricsMock.Object);

            var result = await useCase.ExecuteAsync(new FinishExecutionRequest(id));

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Ordem de serviço não encontrada"));
        }

        [Fact]
        public async Task FinishExecutionUseCase_ShouldTransitionToFinished_WhenOrderIsExecuting()
        {
            var order = CreateServiceOrder();
            order.StartDiagnostics();
            order.FinishAnalysis();
            order.ApproveServiceOrder();
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
            var useCase = new FinishExecutionUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<FinishExecutionUseCase>>(), _applicationMetricsMock.Object);

            var result = await useCase.ExecuteAsync(new FinishExecutionRequest(order.Id));

            result.IsSuccess.Should().BeTrue();
            order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Finished);
            _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
            _applicationMetricsMock.Verify(m => m.CalculateServiceOrderStatusMeanTimeMetric(order), Times.Once);
        }

        [Fact]
        public async Task DeliverServiceOrderUseCase_ShouldReturnFailure_WhenOrderNotFound()
        {
            var id = Guid.NewGuid();
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);
            var useCase = new DeliverServiceOrderUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<DeliverServiceOrderUseCase>>(), _applicationMetricsMock.Object);

            var result = await useCase.ExecuteAsync(new DeliverServiceOrderRequest(id));

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Ordem de serviço não encontrada"));
        }

        [Fact]
        public async Task DeliverServiceOrderUseCase_ShouldTransitionToDelivered_WhenOrderIsFinished()
        {
            var order = CreateServiceOrder();
            order.StartDiagnostics();
            order.FinishAnalysis();
            order.ApproveServiceOrder();
            order.FinishExecution();
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
            var useCase = new DeliverServiceOrderUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<DeliverServiceOrderUseCase>>(), _applicationMetricsMock.Object);

            var result = await useCase.ExecuteAsync(new DeliverServiceOrderRequest(order.Id));

            result.IsSuccess.Should().BeTrue();
            order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Delivered);
            _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
            _applicationMetricsMock.Verify(m => m.CalculateServiceOrderStatusMeanTimeMetric(order), Times.Once);
        }

        [Fact]
        public async Task RefuseServiceOrderUseCase_ShouldReturnFailure_WhenOrderNotFound()
        {
            var id = Guid.NewGuid();
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ServiceOrder?)null);
            var useCase = new RefuseServiceOrderUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<RefuseServiceOrderUseCase>>(), _applicationMetricsMock.Object);

            var result = await useCase.ExecuteAsync(new RefuseServiceOrderRequest(id));

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Ordem de serviço não encontrada"));
        }

        [Fact]
        public async Task RefuseServiceOrderUseCase_ShouldTransitionToRefused_WhenOrderIsWaitingApproval()
        {
            var order = CreateServiceOrder();
            order.StartDiagnostics();
            order.FinishAnalysis();
            _serviceOrderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
            var useCase = new RefuseServiceOrderUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<RefuseServiceOrderUseCase>>(), _applicationMetricsMock.Object);

            var result = await useCase.ExecuteAsync(new RefuseServiceOrderRequest(order.Id));

            result.IsSuccess.Should().BeTrue();
            order.GetLastStatusHistory().Status.Should().Be(OrderStatus.Refused);
            _serviceOrderRepoMock.Verify(r => r.SaveChangesAsync(order), Times.Once);
        }

        [Fact]
        public async Task GetServiceOrderPendingStocksUseCase_ShouldReturnFailure_WhenOrderNotFound()
        {
            var id = Guid.NewGuid();
            _serviceOrderRepoMock.Setup(r => r.GetServiceOrderByIdToGetPeddingStocksAsync(id))
                                 .ReturnsAsync((ServiceOrder?)null);
            var useCase = new GetServiceOrderPendingStocksUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<GetServiceOrderPendingStocksUseCase>>());

            var result = await useCase.ExecuteAsync(id);

            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("Ordem de serviço não encontrada"));
        }

        [Fact]
        public async Task GetServiceOrderPendingStocksUseCase_ShouldReturnPendingStocks_WhenOrderFound()
        {
            var order = CreateServiceOrder();
            var part = new Part("Filtro de óleo", "FO-001", 10, 50m);
            order.AddPart(part, 2);
            order.StartDiagnostics();
            order.FinishAnalysis();
            order.ApproveServiceOrder();
            _serviceOrderRepoMock.Setup(r => r.GetServiceOrderByIdToGetPeddingStocksAsync(order.Id)).ReturnsAsync(order);
            var useCase = new GetServiceOrderPendingStocksUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<GetServiceOrderPendingStocksUseCase>>());

            var result = await useCase.ExecuteAsync(order.Id);

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().NotBeNull();
            result.Response.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAverageDurationUseCase_ShouldReturnValueFromRepository()
        {
            const double expected = 4.5;
            _serviceOrderRepoMock.Setup(r => r.GetAverageDurationInDaysAsync()).ReturnsAsync(expected);
            var useCase = new GetAverageDurationUseCase(_serviceOrderRepoMock.Object, Mock.Of<ILogger<GetAverageDurationUseCase>>());

            var result = await useCase.ExecuteAsync(new NoInput());

            result.IsSuccess.Should().BeTrue();
            result.Response.Should().Be(expected);
        }
    }
}
