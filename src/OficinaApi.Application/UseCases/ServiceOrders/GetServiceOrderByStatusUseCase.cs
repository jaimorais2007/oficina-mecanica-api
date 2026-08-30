using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class GetServiceOrderByStatusUseCase : IUseCase<Guid, ServiceOrderStatusDto?>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly ILogger<GetServiceOrderByStatusUseCase> _logger;

        public GetServiceOrderByStatusUseCase(
            IServiceOrderRepository serviceOrderRepository,
            ILogger<GetServiceOrderByStatusUseCase> logger)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<ServiceOrderStatusDto?>> ExecuteAsync(Guid input)
        {
            var serviceOrder = await _serviceOrderRepository.GetByStatus(input);

            if (serviceOrder == null)
            {
                _logger.LogInformation("Service Order not found by id: {Id}", input);
                return UseCaseResponse<ServiceOrderStatusDto?>.Failure("Ordem de serviço não encontrada.");
            }

            var dto = new ServiceOrderStatusDto
            {
                Id = serviceOrder.Id,
                Status = serviceOrder.GetLastStatusHistory().Status
            };

            return UseCaseResponse<ServiceOrderStatusDto?>.Success(dto);
        }
    }
}