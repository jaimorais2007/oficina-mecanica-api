using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;

namespace OficinaApi.Application.DTOs
{
    public class CustomerDto
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public PersonType PersonType { get; private set; }
        public string Document { get; private set; } = string.Empty;
        public DateTime DateOfBirth { get; private set; }
        public string Email { get; private set; } = string.Empty;

        public CustomerDto(Customer customer)
        {
            Id = customer.Id;
            Name = customer.Name;
            PersonType = customer.PersonType;
            Document = customer.Document.Value; 
            DateOfBirth = customer.DateOfBirth.GetValueOrDefault();
            Email = customer.Email;
        }
    }

    public class CreateCustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public PersonType PersonType { get; set; }
        public string Document { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;

    }

    public class UpdateCustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public PersonType PersonType { get; set; }
        public string Document { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;

    }
}
