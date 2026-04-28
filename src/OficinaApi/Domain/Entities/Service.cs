public class Service
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal DefaultPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }

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
            throw new ArgumentException("Nome do serviço é obrigatório.");

        if (DefaultPrice < 0)
            throw new ArgumentException("Preço informado é inválido.");
    }
}