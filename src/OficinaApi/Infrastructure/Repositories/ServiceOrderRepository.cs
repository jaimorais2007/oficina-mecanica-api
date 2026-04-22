using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
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

    public async Task<IEnumerable<ServiceOrder>> GetAllFinishedOrdersAsync()
    {
        return await _context.ServiceOrders
            .Where(o => o.Status == OrderStatus.Finished || o.Status == OrderStatus.Delivered)
            .ToListAsync();
    }

    public async Task<ServiceOrder?> GetByIdAsync(Guid id)
    {
        return await _context.ServiceOrders.FindAsync(id);
    }

    public async Task UpdateAsync(ServiceOrder order)
    {
        _context.ServiceOrders.Update(order);
        await _context.SaveChangesAsync();
    }
}
