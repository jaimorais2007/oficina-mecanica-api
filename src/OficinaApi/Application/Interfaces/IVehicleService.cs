using OficinaApi.Application.DTOs;

namespace OficinaApi.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDto?>> GetAllVehiclesAsync();
        Task<VehicleDto?> GetVehicleByIdAsync(Guid id);
        Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto);
        Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleDto dto);
        Task DeleteVehicleAsync(Guid id);
    }
}
