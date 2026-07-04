using OficinaApi.Domain.Events;

namespace OficinaApi.Application.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
}
