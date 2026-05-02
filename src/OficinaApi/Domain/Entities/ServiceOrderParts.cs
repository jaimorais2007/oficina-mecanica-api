using System;

namespace OficinaApi.Domain.Entities;

public class ServiceOrderParts
{
    public Guid Id { get; private set; }
    public ServiceOrder ServiceOrder { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public Part Part { get; private set; }
    public Guid PartId { get; private set; }
    public int Quantity { get; private set; }

    // For EF Core
    protected ServiceOrderParts() { }

    public ServiceOrderParts(ServiceOrder serviceOrder, Part part, int quantity)
    {
        Id = Guid.NewGuid();
        ServiceOrderId = serviceOrder.Id;
        ServiceOrder = serviceOrder;
        PartId = part.Id;
        Part = part;
        Quantity = quantity;
    }
}
