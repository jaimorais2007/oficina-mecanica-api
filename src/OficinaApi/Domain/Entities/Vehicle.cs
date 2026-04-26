using System.Xml.Linq;

public class Vehicle
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Plate Plate { get; private set; }
    public string Brand { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Vehicle() { }

    public Vehicle(string name, string plate, string brand, string model, int year)
    {
        Id = Guid.NewGuid();
        Name = name;
        Plate = new Plate(plate);
        Brand = brand;
        Model = model;
        Year = year;
        CreatedAt = DateTime.UtcNow;

        Validate();
    }

    public void Update(string name, string plate, string brand, string model, int year)
    {
        Name = name;
        Plate = new Plate(plate);
        Brand = brand;
        Model = model;
        Year = year;

        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Nome do carro é obrigatório.");

        if (string.IsNullOrWhiteSpace(Brand))
            throw new ArgumentException("Marca é obrigatória.");

        if (string.IsNullOrWhiteSpace(Model))
            throw new ArgumentException("Modelo é obrigatório.");

        if (Year < 1900 || Year > DateTime.UtcNow.Year + 1)
            throw new ArgumentException("Ano inválido.");
    }
}