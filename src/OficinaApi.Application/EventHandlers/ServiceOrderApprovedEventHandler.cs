using System;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace OficinaApi.Application.EventHandlers;

public class ServiceOrderApprovedEventHandler : IDomainEventHandler<ServiceOrderApprovedEvent>
{
    private readonly IUseCase<ServiceOrderApprovedEvent, bool> _useCase;

    public ServiceOrderApprovedEventHandler(IUseCase<ServiceOrderApprovedEvent, bool> useCase)
    {
        _useCase = useCase;
    }

    public async Task HandleAsync(ServiceOrderApprovedEvent domainEvent, CancellationToken cancellationToken)
    {
        await _useCase.ExecuteAsync(domainEvent);
    }
}
