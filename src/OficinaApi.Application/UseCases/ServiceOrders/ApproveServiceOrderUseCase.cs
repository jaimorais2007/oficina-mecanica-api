using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class ApproveServiceOrderUseCase : IUseCase<ApproveServiceOrderRequest, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly ILogger<ApproveServiceOrderUseCase> _logger;

        public ApproveServiceOrderUseCase(
            IServiceOrderRepository serviceOrderRepository,
            ILogger<ApproveServiceOrderUseCase> logger)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(ApproveServiceOrderRequest input)
        {
            try
            {
                var serviceOrder = await _serviceOrderRepository.GetByIdAsync(input.Id);
                if (serviceOrder == null)
                {
                    _logger.LogInformation("Service Order not found for approval. Id: {Id}", input.Id);
                    throw new ArgumentException("Ordem de serviço não encontrada.");
                }

                serviceOrder.ApproveServiceOrder();
                await _serviceOrderRepository.SaveChangesAsync(serviceOrder);

                return UseCaseResponse<ServiceOrderDto>.Success(new ServiceOrderDto(serviceOrder));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving service order");
                return UseCaseResponse<ServiceOrderDto>.Failure(ex.Message);
            }
        }
    }
}
