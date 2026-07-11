using OficinaApi.Domain.Enums;

namespace OficinaApi.Domain.Entities;

public class ServiceOrderStatus : BaseEntity
{
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ServiceOrder ServiceOrder { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public bool Inactive { get; private set; } = false;


    // For EF Core
    protected ServiceOrderStatus()
    {
        
    }

    public ServiceOrderStatus(ServiceOrder serviceOrder, OrderStatus status)
    {
        ServiceOrder = serviceOrder;
        ServiceOrderId = serviceOrder.Id;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }
}
