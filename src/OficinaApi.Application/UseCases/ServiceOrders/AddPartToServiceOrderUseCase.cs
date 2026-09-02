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
    public class AddPartToServiceOrderUseCase : IUseCase<AddPartToServiceOrderRequest, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly IPartRepository _partRepository;
        private readonly ILogger<AddPartToServiceOrderUseCase> _logger;

        public AddPartToServiceOrderUseCase(
            IServiceOrderRepository serviceOrderRepository,
            IPartRepository partRepository,
            ILogger<AddPartToServiceOrderUseCase> logger)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _partRepository = partRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(AddPartToServiceOrderRequest input)
        {
            try
            {
                if (input.Dto.Quantity <= 0)
                {
                    _logger.LogInformation("Quantity validation failed. Quantity must be greater than zero. Provided: {Quantity}", input.Dto.Quantity);
                    throw new DomainException("A quantidade deve ser maior que zero.");
                }

                ServiceOrder? serviceOrder = await _serviceOrderRepository.GetByIdForUpdateAsync(input.Id);
                if (serviceOrder == null)
                {
                    _logger.LogInformation("Service Order not found. Id: {Id}", input.Id);
                    throw new DomainException("Ordem de serviço não encontrada.");
                }

                Part? part = await _partRepository.GetByIdAsync(input.Dto.PartId);
                if (part == null)
                {
                    _logger.LogInformation("Part not found. PartId: {PartId}", input.Dto.PartId);
                    throw new DomainException("Peça não encontrada.");
                }

                serviceOrder.AddPart(part, input.Dto.Quantity);
                await _serviceOrderRepository.SaveChangesAsync(serviceOrder);

                return UseCaseResponse<ServiceOrderDto>.Success(new ServiceOrderDto(serviceOrder));
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error adding part to service order");
                return UseCaseResponse<ServiceOrderDto>.Failure(ex.Message);
            }
        }
    }
}
