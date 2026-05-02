using OficinaApi.Domain.Enums;

namespace OficinaApi.Domain.Entities;

public class ServiceOrderStatus
{
    public Guid Id { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ServiceOrder ServiceOrder { get; set; }

    // For EF Core
    protected ServiceOrderStatus()
    {
        
    }

    public ServiceOrderStatus(ServiceOrder serviceOrder, OrderStatus status)
    {
        Id = Guid.NewGuid();
        ServiceOrder = serviceOrder;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }
}
