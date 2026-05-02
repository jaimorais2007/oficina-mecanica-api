using System;

namespace OficinaApi.Domain.Entities;

public class ServiceOrderServices
{
    public Guid Id { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public Guid ServiceId { get; private set; }
    public ServiceOrder ServiceOrder { get; private set; }
    public Service Service { get; private set; }

    // For EF Core
    protected ServiceOrderServices() { }

    public ServiceOrderServices(ServiceOrder serviceOrder, Service service)
    {
        Id = Guid.NewGuid();
        ServiceOrderId = serviceOrder.Id;
        ServiceOrder = serviceOrder;
        ServiceId = service.Id;
        Service = service;
    }
}
