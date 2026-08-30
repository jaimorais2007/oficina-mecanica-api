using System;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Exceptions;

namespace OficinaApi.Domain.Entities;

public class ServiceOrderPart : BaseEntity
{
    public ServiceOrder ServiceOrder { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public Part Part { get; private set; }
    public Guid PartId { get; private set; }
    public int Quantity { get; private set; } = default;
    public bool StockQuantityWasEnsured { get; private set; }
    public bool Inactive { get; private set; } = false;

    // For EF Core
    protected ServiceOrderPart() { }

    public ServiceOrderPart(ServiceOrder serviceOrder, Part part, int quantity)
    {
        ServiceOrderId = serviceOrder.Id;
        ServiceOrder = serviceOrder;
        PartId = part.Id;
        Part = part;
        Quantity = quantity;
        StockQuantityWasEnsured = false;
    }

    public void EnsureStockQuantity()
    {
        if (StockQuantityWasEnsured)
            throw new DomainException("A quantidade em estoque já foi garantida para esta peça nesta ordem de serviço.");

        Part.RemoveStock(Quantity);
        StockQuantityWasEnsured = true;
    }

    public bool StockQuantityShouldBeEnsured()
    {
        return !StockQuantityWasEnsured && ServiceOrder.GetLastStatusHistory().Status == OrderStatus.Executing;
    }
}
