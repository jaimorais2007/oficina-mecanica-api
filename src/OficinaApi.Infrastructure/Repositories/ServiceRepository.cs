using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Infrastructure.Data;

namespace OficinaApi.Infrastructure.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly OficinaDbContext _context;
        private readonly ILogger<ServiceRepository> _logger;

        public ServiceRepository(OficinaDbContext context, ILogger<ServiceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(Service service)
        {
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var service = await GetByIdAsync(id);
            if (service != null)
            {
                _logger.LogInformation("Deleting service with ID {ServiceId}", id);
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Service>> GetByIdListAsync(IEnumerable<Guid> ids)
        {
            _logger.LogInformation("Searching for services with IDs: {ServiceIds}", ids);
            return await _context.Services.Where(s => ids.Contains(s.Id)).ToListAsync();
        }

        public async Task<IEnumerable<Service>> GetAllAsync()
        {
            _logger.LogInformation("Searching for all services");
            return await _context.Services.ToListAsync();
        }

        public async Task<Service?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Searching for service with ID {ServiceId}", id);
            return await _context.Services.FindAsync(id);
        }

        public async Task UpdateAsync(Service service)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }
    }
}
