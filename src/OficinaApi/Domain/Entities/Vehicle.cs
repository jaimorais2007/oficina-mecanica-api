using System.Xml.Linq;
using OficinaApi.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; private set; }
    public Plate Plate { get; private set; }
    public string Brand { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Customer Customer { get; set; }
    public Guid CustomerId { get; set; }
    public ICollection<ServiceOrder> ServiceOrders { get; private set; } = [];

    //et; }

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
            throw new ArgumentException("Marca é obrigatória.");

        if (string.IsNullOrWhiteSpace(Model))
            throw new ArgumentException("Modelo é obrigatório.");

        if (Year < 1900 || Year > DateTime.UtcNow.Year + 1)
            throw new ArgumentException("Ano inválido.");
    }
}