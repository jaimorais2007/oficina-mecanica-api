using OficinaApi.Domain.Entities;
using OficinaApi.Domain.ValueObjects;

namespace OficinaApi.Domain.Interfaces
{
    public interface IVehicleRepository
    {
        Task AddAsync(Vehicle vehicle);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Vehicle>> GetAllAsync();
        Task<Vehicle?> GetByIdAsync(Guid id);
        Task UpdateAsync(Vehicle vehicle);
        Task<Vehicle?> GetVehicleAsync(Plate plate);
    }
}
