using System;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.EventHandlers;

public class ServiceOrderApprovedEventHandler : IDomainEventHandler<ServiceOrderApprovedEvent>
{
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly ILogger<ServiceOrderApprovedEventHandler> _logger;
    public ServiceOrderApprovedEventHandler(
        IServiceOrderRepository serviceOrderRepository,
        ILogger<ServiceOrderApprovedEventHandler> logger)
    {
        _serviceOrderRepository = serviceOrderRepository;
        _logger = logger;
    }

    public async Task HandleAsync(ServiceOrderApprovedEvent domainEvent, CancellationToken cancellationToken)
    {
        var serviceOrder = await _serviceOrderRepository.GetByIdWithPartsDetailsAsync(domainEvent.ServiceOrderId);
        if (serviceOrder == null)
        {
            _logger.LogWarning("Service order with ID {ServiceOrderId} not found for approval event.", domainEvent.ServiceOrderId);
            return;
        }

        foreach (var partUsed in serviceOrder.PartsUsed)
        {
            try
            {
                partUsed.EnsureStockQuantity();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                _logger.LogError(ex, "Error updating stock for part ID {PartId} used in service order ID {ServiceOrderId}.", partUsed.PartId, domainEvent.ServiceOrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating stock for part ID {PartId} used in service order ID {ServiceOrderId}.", partUsed.PartId, domainEvent.ServiceOrderId);
            }
        }

        await _serviceOrderRepository.UpdateAsync(serviceOrder);
        
        _logger.LogInformation("Service order with ID {ServiceOrderId} approved. Stock levels updated for used parts.", domainEvent.ServiceOrderId);
    }
}
