using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class ApproveServiceOrderUseCase : IUseCase<ApproveServiceOrderRequest, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly ILogger<ApproveServiceOrderUseCase> _logger;
        private readonly IApplicationMetrics _applicationMetrics;

        public ApproveServiceOrderUseCase(
            IServiceOrderRepository serviceOrderRepository,
            ILogger<ApproveServiceOrderUseCase> logger,
            IApplicationMetrics applicationMetrics)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _logger = logger;
            _applicationMetrics = applicationMetrics;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(ApproveServiceOrderRequest input)
        {
            try
            {
                var serviceOrder = await _serviceOrderRepository.GetByIdAsync(input.Id);
                if (serviceOrder == null)
                {
                    _logger.LogInformation("Service Order not found for approval. Id: {Id}", input.Id);
                    throw new DomainException("Ordem de serviço não encontrada.");
                }

                serviceOrder.ApproveServiceOrder();
                await _serviceOrderRepository.SaveChangesAsync(serviceOrder);

                _applicationMetrics.CalculateServiceOrderStatusMeanTimeMetric(serviceOrder);
                return UseCaseResponse<ServiceOrderDto>.Success(new ServiceOrderDto(serviceOrder));
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error approving service order");
                return UseCaseResponse<ServiceOrderDto>.Failure(ex.Message);
            }
        }
    }
}
