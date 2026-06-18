using System;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.EventHandlers;

public class PartStockAddedEventHandler : IDomainEventHandler<PartStockAddedEvent>
{
    private readonly IUseCase<PartStockAddedEvent, bool> _useCase;

    public PartStockAddedEventHandler(IUseCase<PartStockAddedEvent, bool> useCase)
    {
        _useCase = useCase;
    }

    public async Task HandleAsync(PartStockAddedEvent domainEvent, CancellationToken cancellationToken)
    {
        await _useCase.ExecuteAsync(domainEvent);
    }
}
