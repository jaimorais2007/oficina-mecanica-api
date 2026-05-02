namespace OficinaApi.Domain.Events;

public class ServiceOrderApprovedEvent : DomainEvent
{
    public Guid ServiceOrderId { get; set; }
}
