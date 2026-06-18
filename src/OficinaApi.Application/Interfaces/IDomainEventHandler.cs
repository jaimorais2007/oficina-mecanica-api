using System;
using OficinaApi.Domain.Events;

namespace OficinaApi.Application.Interfaces;

public interface IDomainEventHandler<in T> where T : DomainEvent
{
    Task HandleAsync(T domainEvent, CancellationToken cancellationToken);
}
