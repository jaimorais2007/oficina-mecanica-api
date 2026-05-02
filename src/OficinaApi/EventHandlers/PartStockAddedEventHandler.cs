using System;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.EventHandlers;

public class PartStockAddedEventHandler : IDomainEventHandler<PartStockAddedEvent>
{
    private readonly ILogger<PartStockAddedEventHandler> _logger;
    private readonly IPartRepository _partRepository;

    public PartStockAddedEventHandler(ILogger<PartStockAddedEventHandler> logger, IPartRepository partRepository)
    {
        _logger = logger;
        _partRepository = partRepository;
    }

    public async Task HandleAsync(PartStockAddedEvent domainEvent, CancellationToken cancellationToken)
    {
        var part = await _partRepository.GetByIdWithServiceOrderDetailsAsync(domainEvent.PartId);
        if (part == null)
        {
            _logger.LogWarning("Peça com ID {PartId} não encontrada.", domainEvent.PartId);
            return;
        }

        var serviceOrderPartsToEnsure = part.ServiceOrdersParts.Where(sop => sop.StockQuantityShouldBeEnsured()).ToList();
        if (!serviceOrderPartsToEnsure.Any())
        {
            _logger.LogDebug("Nenhuma ordem de serviço pendente encontrada para a peça com ID {PartId}.", domainEvent.PartId);
            return;
        }

        foreach (ServiceOrderPart serviceOrderPart in serviceOrderPartsToEnsure)
        {
            try
            {
                serviceOrderPart.EnsureStockQuantity();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao garantir a quantidade de estoque para a peça com ID {PartId}.", domainEvent.PartId);
            }
        }

        await _partRepository.UpdateAsync(part);
        _logger.LogInformation("Estoque descontado automaticamente para a peça com ID {PartId}.", domainEvent.PartId);
    }
}
