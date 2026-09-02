using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.ValueObjects;

namespace OficinaApi.Domain.Entities;

public class Vehicle : BaseEntity
{
    public Plate Plate { get; private set; }
    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; } = default;
    public DateTime CreatedAt { get; private set; } = default;
    public Customer Customer { get; set; }
    public Guid CustomerId { get; set; }
    public ICollection<ServiceOrder> ServiceOrders { get; private set; } = [];
    public bool Inactive { get; private set; } = false;

    protected Vehicle() { }

    public Vehicle(Customer customer, string plate, string brand, string model, int year)
    {
        Id = Guid.NewGuid();
        Customer = customer;
        CustomerId = customer.Id;
        Plate = new Plate(plate);
        Brand = brand;
        Model = model;
        Year = year;
        CreatedAt = DateTime.UtcNow;

        Validate();
    }

    public void Update(string plate, string brand, string model, int year)
    {
        Plate = new Plate(plate);
        Brand = brand;
        Model = model;
        Year = year;

        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Brand))
            throw new DomainException("Marca é obrigatória.");

        if (string.IsNullOrWhiteSpace(Model))
            throw new DomainException("Modelo é obrigatório.");

        if (Year < 1900 || Year > DateTime.UtcNow.Year + 1)
            throw new DomainException("Ano inválido.");
    }

    public void SetInactive(bool inactive)
    {
        Inactive = inactive;
    }
}
