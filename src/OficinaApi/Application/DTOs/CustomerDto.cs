using OficinaApi.Domain.Enums;

namespace OficinaApi.Application.DTOs
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public PersonType PersonType { get; set; }
        public string Document { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
    }
    public class CreateCustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public PersonType PersonType { get; set; }
        public string Document { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
    }
    public class UpdateCustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public PersonType PersonType { get; set; }
        public string Document { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
    }
    
}
