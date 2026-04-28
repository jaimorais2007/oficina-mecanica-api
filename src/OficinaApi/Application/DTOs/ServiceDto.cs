using OficinaApi.Domain.Entities;

namespace OficinaApi.Application.DTOs
{
    public class ServiceDto
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal DefaultPrice { get; private set; }

        public ServiceDto(Service service)
        {
            Id = service.Id;
            Name = service.Name;
            Description = service.Description;
            DefaultPrice = service.DefaultPrice;
        }
    }

    public class CreateServiceDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }
    }

    public class UpdateServiceDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }
    }
}