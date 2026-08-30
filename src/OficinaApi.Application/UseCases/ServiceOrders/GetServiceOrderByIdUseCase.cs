using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class GetServiceOrderByIdUseCase : IUseCase<Guid, ServiceOrderDto?>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly ILogger<GetServiceOrderByIdUseCase> _logger;

        public GetServiceOrderByIdUseCase(
            IServiceOrderRepository serviceOrderRepository,
            ILogger<GetServiceOrderByIdUseCase> logger)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<ServiceOrderDto?>> ExecuteAsync(Guid input)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdAsync(input);
            if (serviceOrder == null)
            {
                _logger.LogInformation("Service Order not found by id: {Id}", input);
                return UseCaseResponse<ServiceOrderDto?>.Failure("Ordem de serviço não encontrada.");
            }

            return UseCaseResponse<ServiceOrderDto?>.Success(new ServiceOrderDto(serviceOrder));
        }
    }
}
