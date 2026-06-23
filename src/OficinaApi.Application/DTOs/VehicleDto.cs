using OficinaApi.Domain.Entities;
using OficinaApi.Domain.ValueObjects;

namespace OficinaApi.Application.DTOs
{
    public class VehicleDto
    {
        public Guid Id { get; private set; }
        public string Plate { get; private set; } = string.Empty;
        public string Brand { get; private set; } = string.Empty;
        public string Model { get; private set; } = string.Empty;
        public int Year { get; private set; }

        public VehicleDto(Vehicle vehicle)
        {
            Id = vehicle.Id;
            Plate = vehicle.Plate.Value;
            Brand = vehicle.Brand;
            Model = vehicle.Model;
            Year = vehicle.Year;
        }
    }

    public class CreateVehicleDto
    {
        public Guid CustomerId { get; set; }
        public string Plate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
    }

    public class UpdateVehicleDto
    {
        public string Plate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
    }
}
