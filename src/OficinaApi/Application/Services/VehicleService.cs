using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<IEnumerable<VehicleDto?>> GetAllVehiclesAsync()
        {
            var vehicle = await _vehicleRepository.GetAllAsync();
            return vehicle.Select(x => new VehicleDto
            {
                Id = x.Id,
                Brand = x.Brand,
                Model = x.Model,
                Plate = x.Plate.Value,
                Year = x.Year,
            });
        }

        public async Task<VehicleDto?> GetVehicleByIdAsync(Guid id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return null;

            return new VehicleDto
            {
                Id = vehicle.Id,
                Name = vehicle.Name,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Plate = vehicle.Plate.Value,
                Year = vehicle.Year,
            };
        }

        public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto)
        {
            var vehicle = new Vehicle(dto.Name, dto.Plate, dto.Brand, dto.Model, dto.Year);
            await _vehicleRepository.AddAsync(vehicle);

            return new VehicleDto
            {
                Id = vehicle.Id,
                Name = vehicle.Name,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Plate = vehicle.Plate.Value,
                Year = vehicle.Year,
            };
        }

        public async Task DeleteVehicleAsync(Guid id)
        {
            await _vehicleRepository.DeleteAsync(id);
        }

        public async Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleDto dto)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);

            if (vehicle == null)
                throw new Exception("Veículo não encontrado.");

            vehicle.Update(
                dto.Name,
                dto.Plate,
                dto.Brand,
                dto.Model,
                dto.Year
            );

            await _vehicleRepository.UpdateAsync(vehicle);

            return new VehicleDto
            {
                Id = vehicle.Id,
                Name = vehicle.Name,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Plate = vehicle.Plate.Value,
                Year = vehicle.Year,
            };
        }
    }
}
