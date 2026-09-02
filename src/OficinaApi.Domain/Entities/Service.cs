using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Exceptions;

namespace OficinaApi.Domain.Entities;

public class Service : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal DefaultPrice { get; private set; } = default;
    public DateTime CreatedAt { get; private set; } = default;
    public ICollection<ServiceOrderService> ServiceOrdersServices { get; set; } = [];
    public bool Inactive { get; private set; } = false;

    protected Service() { }

    public Service(string name, string description, decimal defaultPrice)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        DefaultPrice = defaultPrice;
        CreatedAt = DateTime.UtcNow;

        Validate();
    }

    public void Update(string name, string description, decimal price)
    {
        Name = name;
        Description = description;
        DefaultPrice = price;

        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new DomainException("Nome do serviço é obrigatório.");

        if (DefaultPrice < 0)
            throw new DomainException("Preço informado é inválido.");
    }

    public void SetInactive(bool inactive)
    {
        Inactive = inactive;
    }
}
