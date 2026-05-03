using Microsoft.EntityFrameworkCore;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Infrastructure.Data;

namespace OficinaApi.Infrastructure.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly OficinaDbContext _context;

        public VehicleRepository(OficinaDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Vehicle vehicle)
        {
            if (await GetVehicleAsync(vehicle.Plate) is not null)
                throw new Exception("Já existe um veículo cadastrado com essa placa.");

            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var vehicle = await GetByIdAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            return await _context.Vehicles.ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleAsync(Plate plate)
        {
            return await _context.Vehicles.FirstOrDefaultAsync(vehicle => vehicle.Plate == plate);
        }

        public async Task<Vehicle?> GetByIdAsync(Guid id)
        {
            return await _context.Vehicles.FindAsync(id);
        }

        public async Task UpdateAsync(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
        }
    }
}
