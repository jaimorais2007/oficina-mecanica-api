using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class FinishExecutionUseCase : IUseCase<FinishExecutionRequest, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly ILogger<FinishExecutionUseCase> _logger;

        public FinishExecutionUseCase(
            IServiceOrderRepository serviceOrderRepository,
            ILogger<FinishExecutionUseCase> logger)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(FinishExecutionRequest input)
        {
            try
            {
                var serviceOrder = await _serviceOrderRepository.GetByIdAsync(input.Id);
                if (serviceOrder == null)
                {
                    _logger.LogInformation("Service Order not found for finishing execution. Id: {Id}", input.Id);
                    throw new ArgumentException("Ordem de serviço não encontrada.");
                }

                serviceOrder.FinishExecution();
                await _serviceOrderRepository.SaveChangesAsync(serviceOrder);

                return UseCaseResponse<ServiceOrderDto>.Success(new ServiceOrderDto(serviceOrder));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finishing execution of service order");
                return UseCaseResponse<ServiceOrderDto>.Failure(ex.Message);
            }
        }
    }
}
