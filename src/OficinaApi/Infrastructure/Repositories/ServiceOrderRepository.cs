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
        return await _context.ServiceOrders
            .Include(so => so.StatusHistory)
            .Include(so => so.Customer)
            .Include(so => so.Vehicle)
            .Include(so => so.ServicesUsed).ThenInclude(s => s.Service)
            .Include(so => so.PartsUsed).ThenInclude(p => p.Part)
            .FirstOrDefaultAsync(so => so.Id == id);
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

    public async Task<double> GetAverageDurationInDaysAsync()
    {
        var result = await _context.Database
            .SqlQuery<double>($"""
                SELECT COALESCE(AVG(duracao_segundos) / 86400.0, 0) AS "Value"
                FROM (
                    SELECT
                        so."Id",
                        EXTRACT(EPOCH FROM (
                            MIN(CASE WHEN sos."Status" = 'Finished' THEN sos."CreatedAt" END) -
                            MIN(CASE WHEN sos."Status" = 'Received'  THEN sos."CreatedAt" END)
                        )) AS duracao_segundos
                    FROM "ServiceOrders" so
                    INNER JOIN "ServiceOrderStatuses" sos ON sos."ServiceOrderId" = so."Id"
                    WHERE sos."Status" IN ('Received', 'Finished')
                    GROUP BY so."Id"
                    HAVING
                        MIN(CASE WHEN sos."Status" = 'Received'  THEN sos."CreatedAt" END) IS NOT NULL
                        AND MIN(CASE WHEN sos."Status" = 'Finished' THEN sos."CreatedAt" END) IS NOT NULL
                ) duracoes
                """)
            .ToListAsync();

        return result.FirstOrDefault();
    }
}
