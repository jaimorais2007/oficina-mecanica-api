namespace OficinaApi.Domain.Events;

public class ServiceOrderApprovedEvent : DomainEvent
{
    public Guid ServiceOrderId { get; private set; }
    public ServiceOrderApprovedEvent(Guid serviceOrderId)
    {
        ServiceOrderId = serviceOrderId;
    }

}
