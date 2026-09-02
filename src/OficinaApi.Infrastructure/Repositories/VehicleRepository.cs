using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Domain.ValueObjects;
using OficinaApi.Infrastructure.Data;

namespace OficinaApi.Infrastructure.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly OficinaDbContext _context;
        private readonly ILogger<VehicleRepository> _logger;

        public VehicleRepository(OficinaDbContext context, ILogger<VehicleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(Vehicle vehicle)
        {
            if (await GetVehicleAsync(vehicle.Plate) is not null)
            {
                _logger.LogInformation("Attempted to add a vehicle with an existing plate: {Plate}", vehicle.Plate.Value);
                throw new DomainException("Já existe um veículo cadastrado com essa placa.");
            }

            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var vehicle = await GetByIdAsync(id);
            if (vehicle != null)
            {
                _logger.LogInformation("Deleting vehicle with ID: {Id}", id);
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            _logger.LogInformation("Searching all vehicles");
            return await _context.Vehicles.ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleAsync(Plate plate)
        {
            _logger.LogInformation("Searching vehicle by plate: {Plate}", plate.Value);
            return await _context.Vehicles.FirstOrDefaultAsync(vehicle => vehicle.Plate.Value == plate.Value);
        }

        public async Task<Vehicle?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Searching vehicle by ID: {Id}", id);
            return await _context.Vehicles.FindAsync(id);
        }

        public async Task UpdateAsync(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
        }
    }
}
