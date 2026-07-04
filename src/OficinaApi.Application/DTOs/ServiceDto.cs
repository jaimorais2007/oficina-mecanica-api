using OficinaApi.Domain.Entities;

namespace OficinaApi.Application.DTOs
{
    public class ServiceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }

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
        public Guid? Id { get; set; }
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

    public class AddServiceDto
    {
        public Guid ServiceId { get; set; }
    }
}
