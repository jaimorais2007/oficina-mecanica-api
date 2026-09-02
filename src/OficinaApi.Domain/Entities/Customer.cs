using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.ValueObjects;

namespace OficinaApi.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public PersonType PersonType { get; private set; }
        public Document Document { get; private set; }
        public DateTime CreatedAt { get; private set; } = default;
        public DateTime? DateOfBirth { get; private set; }
        public string Email { get; private set; }
        public ICollection<ServiceOrder> ServiceOrders { get; private set; } = [];
        public ICollection<Vehicle> Vehicles { get; private set; } = [];
        public bool Inactive { get; private set; } = false;

        protected Customer() {}

        public Customer(string name, PersonType personType, string document, DateTime? dateOfBirth, string email)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;

            ApplyChanges(name, personType, document, dateOfBirth, email);
        }

        public void Update(string name, PersonType personType, string document, DateTime? dateOfBirth, string email)
        {
            ApplyChanges(name, personType, document, dateOfBirth, email);
        }

        private void ApplyChanges(string name, PersonType personType, string document, DateTime? dateOfBirth, string email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("E-mail é obrigatório.");

            if (!Enum.IsDefined(typeof(PersonType), personType))
                throw new DomainException("Tipo de pessoa inválido.");

            if (personType == PersonType.Individual && dateOfBirth == null)
                throw new DomainException("Data de nascimento é obrigatória para pessoa física.");

            Name = name.Trim();
            PersonType = personType;

            Document = new Document(document, personType);

            DateOfBirth = dateOfBirth.GetValueOrDefault();
            Email = email;
        }

        public void SetInactive(bool inactive)
        {
            Inactive = inactive;
        }
    }
}
