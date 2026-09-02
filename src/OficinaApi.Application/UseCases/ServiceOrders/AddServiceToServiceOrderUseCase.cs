using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class AddServiceToServiceOrderUseCase : IUseCase<AddServiceToServiceOrderRequest, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<AddServiceToServiceOrderUseCase> _logger;

        public AddServiceToServiceOrderUseCase(
            IServiceOrderRepository serviceOrderRepository,
            IServiceRepository serviceRepository,
            ILogger<AddServiceToServiceOrderUseCase> logger)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(AddServiceToServiceOrderRequest input)
        {
            try
            {
                ServiceOrder? serviceOrder = await _serviceOrderRepository.GetByIdAsync(input.Id);
                if (serviceOrder == null)
                {
                    _logger.LogInformation("Service Order not found. Id: {Id}", input.Id);
                    throw new DomainException("Ordem de serviço não encontrada.");
                }

                Service? service = await _serviceRepository.GetByIdAsync(input.Dto.ServiceId);
                if (service == null)
                {
                    _logger.LogInformation("Service not found. ServiceId: {ServiceId}", input.Dto.ServiceId);
                    throw new DomainException("Serviço não encontrado.");
                }

                serviceOrder.AddService(service);
                await _serviceOrderRepository.SaveChangesAsync(serviceOrder);

                return UseCaseResponse<ServiceOrderDto>.Success(new ServiceOrderDto(serviceOrder));
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error adding service to service order");
                return UseCaseResponse<ServiceOrderDto>.Failure(ex.Message);
            }
        }
    }
}
