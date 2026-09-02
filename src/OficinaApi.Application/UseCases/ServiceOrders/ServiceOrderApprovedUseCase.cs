using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders;

public class ServiceOrderApprovedUseCase : IUseCase<ServiceOrderApprovedEvent, bool>
{
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly ILogger<ServiceOrderApprovedUseCase> _logger;

    public ServiceOrderApprovedUseCase(
        IServiceOrderRepository serviceOrderRepository,
        ILogger<ServiceOrderApprovedUseCase> logger)
    {
        _serviceOrderRepository = serviceOrderRepository;
        _logger = logger;
    }

    public async Task<UseCaseResponse<bool>> ExecuteAsync(ServiceOrderApprovedEvent domainEvent)
    {
        var serviceOrder = await _serviceOrderRepository.GetByIdWithPartsDetailsAsync(domainEvent.ServiceOrderId);
        if (serviceOrder == null)
        {
            _logger.LogWarning("Service order with ID {ServiceOrderId} not found for approval event.", domainEvent.ServiceOrderId);
            return UseCaseResponse<bool>.Failure($"Service order with ID {domainEvent.ServiceOrderId} not found for approval event.");
        }

        foreach (var partUsed in serviceOrder.PartsUsed)
        {
            try
            {
                partUsed.EnsureStockQuantity();
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error updating stock for part ID {PartId} used in service order ID {ServiceOrderId}.", partUsed.PartId, domainEvent.ServiceOrderId);
            }
        }

        await _serviceOrderRepository.SaveChangesAsync(serviceOrder);
        
        _logger.LogInformation("Service order with ID {ServiceOrderId} approved. Stock levels updated for used parts.", domainEvent.ServiceOrderId);
        
        return UseCaseResponse<bool>.Success(true);
    }
}
