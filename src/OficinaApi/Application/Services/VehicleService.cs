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
            return vehicle.Select(x => new VehicleDto(x));
        }

        public async Task<VehicleDto?> GetVehicleByIdAsync(Guid id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return null;

            return new VehicleDto(vehicle);
        }

        public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto)
        {
            var vehicle = new Vehicle(dto.Plate, dto.Brand, dto.Model, dto.Year);

            await _vehicleRepository.AddAsync(vehicle);

            return new VehicleDto(vehicle);
        }

        public async Task DeleteVehicleAsync(Guid id)
        {
            await _vehicleRepository.DeleteAsync(id);
        }

        public async Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleDto dto)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);

            if (vehicle == null)
                throw new ArgumentException("Veículo não encontrado.");

            vehicle.Update(
                dto.Plate,
                dto.Brand,
                dto.Model,
                dto.Year
            );

            await _vehicleRepository.UpdateAsync(vehicle);

            return new VehicleDto(vehicle);
        }
    }
}
