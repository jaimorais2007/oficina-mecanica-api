using Microsoft.EntityFrameworkCore;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Infrastructure.Data;

namespace OficinaApi.Infrastructure.Repositories;

public class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly OficinaDbContext _context;

    public ServiceOrderRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ServiceOrder order)
    {
        await _context.ServiceOrders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task<ServiceOrder?> GetByIdAsync(Guid id)
    {
        return await _context.ServiceOrders.FindAsync(id);
    }

    public async Task<ServiceOrder?> GetServiceOrderByIdToGetPeddingStocksAsync(Guid id)
    {
        var serviceOrder = await _context.ServiceOrders
            .Include(so => so.PartsUsed)
            .ThenInclude(sop => sop.Part)
            .FirstOrDefaultAsync(so => so.Id == id);
        return serviceOrder;
    }

    public async Task<ServiceOrder?> GetByIdWithPartsDetailsAsync(Guid id)
    {
        return await _context.ServiceOrders
            .Include(so => so.PartsUsed)
            .ThenInclude(sop => sop.Part)
            .FirstOrDefaultAsync(so => so.Id == id);
    }

    public async Task UpdateAsync(ServiceOrder order)
    {
        _context.ServiceOrders.Update(order);
        await _context.SaveChangesAsync();
    }
}
