namespace OficinaApi.Application.DTOs
{
    public class VehicleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
    }

    public class CreateVehicleDto
    {
        public string Name { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
    }

    public class UpdateVehicleDto
    {
        public string Name { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
    }
}
