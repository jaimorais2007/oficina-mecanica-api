using System;

namespace OficinaApi.Domain.Entities;

public class ServiceOrderAlert : BaseEntity
{
    public string Message { get; private set; } = string.Empty;
    public bool Concluded { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ServiceOrder ServiceOrder { get; private set; }
    public Guid ServiceOrderId { get; private set; }

    // For EF Core
    protected ServiceOrderAlert() { }

    public ServiceOrderAlert(ServiceOrder serviceOrder, string message)
    {
        Id = Guid.NewGuid();
        Message = message;
        CreatedAt = DateTime.UtcNow;
        Concluded = false;
        ServiceOrder = serviceOrder;
        ServiceOrderId = serviceOrder.Id;
    }

    public void Conclude()
    {
        Concluded = true;
    }
}
