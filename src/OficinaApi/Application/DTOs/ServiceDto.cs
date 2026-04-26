namespace OficinaApi.Application.DTOs
{
    public class ServiceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }
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
