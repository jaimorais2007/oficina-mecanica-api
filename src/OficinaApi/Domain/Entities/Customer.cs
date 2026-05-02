using OficinaApi.Domain.Enums;
using OficinaApi.Domain.ValueObjects;

namespace OficinaApi.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public PersonType PersonType { get; private set; }
        public Document Document { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public ICollection<ServiceOrder> ServiceOrders { get; private set; } = [];
        public ICollection<Vehicle> Vehicles { get; private set; } = [];

        // For EF Core
        protected Customer()
        {
            Name = string.Empty;
            Document = null!;
        }

        public Customer(string name, PersonType personType, string document, DateTime? dateOfBirth)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;

            ApplyChanges(name, personType, document, dateOfBirth);
        }

        public void Update(string name, PersonType personType, string document, DateTime? dateOfBirth)
        {
            ApplyChanges(name, personType, document, dateOfBirth);
        }

        private void ApplyChanges(string name, PersonType personType, string document, DateTime? dateOfBirth)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome é obrigatório.");

            if (!Enum.IsDefined(typeof(PersonType), personType))
                throw new ArgumentException("Tipo de pessoa inválido.");

            if (personType == PersonType.Individual && dateOfBirth == null)
                throw new ArgumentException("Data de nascimento é obrigatória para pessoa física.");

            Name = name.Trim();
            PersonType = personType;

            // Validações do documento informado.
            Document = new Document(document, personType);

            DateOfBirth = dateOfBirth.GetValueOrDefault();
        }
    }
}