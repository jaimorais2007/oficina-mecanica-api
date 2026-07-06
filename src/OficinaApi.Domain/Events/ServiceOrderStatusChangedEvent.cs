using OficinaApi.Domain.Entities;

namespace OficinaApi.Domain.Events;

public class ServiceOrderStatusChangedEvent : DomainEvent
{
    public ServiceOrder ServiceOrder { get; }

    public ServiceOrderStatusChangedEvent(ServiceOrder serviceOrder)
    {
        ServiceOrder = serviceOrder;
    }
}