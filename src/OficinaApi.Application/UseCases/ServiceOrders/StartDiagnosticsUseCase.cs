using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class StartDiagnosticsUseCase : IUseCase<StartDiagnosticsRequest, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly IApplicationMetrics _applicationMetrics;

        private readonly ILogger<StartDiagnosticsUseCase> _logger;

        public StartDiagnosticsUseCase(
            IServiceOrderRepository serviceOrderRepository,
            ILogger<StartDiagnosticsUseCase> logger,
            IApplicationMetrics applicationMetrics)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _logger = logger;
            _applicationMetrics = applicationMetrics;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(StartDiagnosticsRequest input)
        {
            try
            {
                var serviceOrder = await _serviceOrderRepository.GetByIdForUpdateAsync(input.Id);
                if (serviceOrder == null)
                {
                    _logger.LogInformation("Service Order not found to start diagnostics. Id: {Id}", input.Id);
                    throw new DomainException("Ordem de serviço não encontrada.");
                }

                serviceOrder.StartDiagnostics();
                await _serviceOrderRepository.SaveChangesAsync(serviceOrder);

                _applicationMetrics.CalculateServiceOrderStatusMeanTimeMetric(serviceOrder);
                return UseCaseResponse<ServiceOrderDto>.Success(new ServiceOrderDto(serviceOrder));
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error starting diagnostics for service order");
                return UseCaseResponse<ServiceOrderDto>.Failure(ex.Message);
            }
        }
    }
}
