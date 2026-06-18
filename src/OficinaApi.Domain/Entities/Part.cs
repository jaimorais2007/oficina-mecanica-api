using System;
using OficinaApi.Domain.Events;

namespace OficinaApi.Domain.Entities;

public class Part : BaseEntity
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public int QuantityInStock { get; private set; }
    public decimal Price { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<ServiceOrderPart> ServiceOrdersParts { get; set; } = [];

    // For EF Core
    protected Part() 
    { 
        Name = string.Empty;
        Code = string.Empty;
    }

    public Part(string name, string code, int initialQuantity, decimal price)
    {
        Id = Guid.NewGuid();
        Name = name;
        Code = code;
        QuantityInStock = initialQuantity;
        Price = price;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("A quantidade a adicionar deve ser maior que zero.");
            
        QuantityInStock += quantity;
        AddDomainEvent(new PartStockAddedEvent(Id));
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("A quantidade a remover deve ser maior que zero.");
            
        if (QuantityInStock < quantity)
            throw new InvalidOperationException("Estoque insuficiente para remover essa quantidade.");
            
        QuantityInStock -= quantity;
    }

    public void UpdateDetails(string name, string code, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nome invalido.");
        if (price < 0) throw new ArgumentException("Preco nao pode ser negativo.");

        Name = name;
        Code = code;
        Price = price;
    }
}
