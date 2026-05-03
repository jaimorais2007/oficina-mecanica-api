using System;
using OficinaApi.Domain.Events;

namespace OficinaApi.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

}
