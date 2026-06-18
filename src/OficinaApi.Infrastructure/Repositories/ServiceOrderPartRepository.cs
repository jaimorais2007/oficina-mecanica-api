using System.Collections.Generic;
using System.Threading.Tasks;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Infrastructure.Data;

namespace OficinaApi.Infrastructure.Repositories;

public class ServiceOrderPartRepository : IServiceOrderPartRepository
{
    private readonly OficinaDbContext _context;

    public ServiceOrderPartRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task UpdateRangeAsync(IEnumerable<ServiceOrderPart> serviceOrderParts)
    {
        _context.ServiceOrderParts.UpdateRange(serviceOrderParts);
        await _context.SaveChangesAsync();
    }
}
