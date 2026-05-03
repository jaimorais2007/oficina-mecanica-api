using System;

namespace OficinaApi.Domain.Events;

public class PartStockAddedEvent : DomainEvent
{
    public Guid PartId { get; private set; }

    public PartStockAddedEvent(Guid partId)
    {
        PartId = partId;
    }
}
