using System;

namespace OficinaApi.Domain.Entities;

public class ServiceOrderService : BaseEntity
{
    public Guid ServiceOrderId { get; private set; }
    public Guid ServiceId { get; private set; }
    public ServiceOrder ServiceOrder { get; private set; }
    public Service Service { get; private set; }
    public bool Inactive { get; private set; } = false;

    protected ServiceOrderService() { }

    public ServiceOrderService(ServiceOrder serviceOrder, Service service)
    {
        ServiceOrderId = serviceOrder.Id;
        ServiceOrder = serviceOrder;
        ServiceId = service.Id;
        Service = service;
    }
}
