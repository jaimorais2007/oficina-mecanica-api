using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Infrastructure.Data;

namespace OficinaApi.Infrastructure.Repositories;

public class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly OficinaDbContext _context;
    private readonly ILogger<ServiceOrderRepository> _logger;

    public ServiceOrderRepository(OficinaDbContext context, ILogger<ServiceOrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ServiceOrder>> GetAllAsync()
    {
        return await _context.ServiceOrders
            .Include(so => so.StatusHistory)
            .Include(so => so.Customer)
            .Include(so => so.Vehicle)
            .Include(so => so.ServicesUsed)
                .ThenInclude(s => s.Service)
            .Include(so => so.PartsUsed)
                .ThenInclude(p => p.Part)
            .Where(so =>
                so.StatusHistory
                    .OrderByDescending(sh => sh.CreatedAt)
                    .Select(sh => sh.Status)
                    .First() != OrderStatus.Finished &&
                so.StatusHistory
                    .OrderByDescending(sh => sh.CreatedAt)
                    .Select(sh => sh.Status)
                    .First() != OrderStatus.Delivered)
            .OrderBy(so =>
                so.StatusHistory
                    .OrderByDescending(sh => sh.CreatedAt)
                    .Select(sh => sh.Status == OrderStatus.Executing ? 0 :
                                  sh.Status == OrderStatus.WaitingApproval ? 1 :
                                  sh.Status == OrderStatus.InDiagnostics ? 2 :
                                  sh.Status == OrderStatus.Received ? 3 : 4)
                    .First())
            .ThenBy(so => so.CreatedAt)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task AddAsync(ServiceOrder order)
    {
        await _context.ServiceOrders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task<ServiceOrder?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting ServiceOrder by ID: {Id}", id);
        return await _context.ServiceOrders
            .Include(so => so.StatusHistory)
            .Include(so => so.Customer)
            .Include(so => so.Vehicle)
            .Include(so => so.ServicesUsed).ThenInclude(s => s.Service)
            .Include(so => so.PartsUsed).ThenInclude(p => p.Part)
            .AsSplitQuery()
            .FirstOrDefaultAsync(so => so.Id == id);
    }

    public async Task<ServiceOrder?> GetByStatus(Guid id)
    {
        _logger.LogInformation("Getting ServiceOrder by ID: {Id}", id);

        return await _context.ServiceOrders
            .Include(so => so.StatusHistory)
            .FirstOrDefaultAsync(so => so.Id == id);
    }

    public async Task<ServiceOrder?> GetServiceOrderByIdToGetPeddingStocksAsync(Guid id)
    {
        _logger.LogInformation("Getting ServiceOrder by ID to get pending stocks: {Id}", id);
        var serviceOrder = await _context.ServiceOrders
            .Include(so => so.PartsUsed)
            .ThenInclude(sop => sop.Part)
            .AsSplitQuery()
            .FirstOrDefaultAsync(so => so.Id == id);
        return serviceOrder;
    }

    public async Task<ServiceOrder?> GetByIdWithPartsDetailsAsync(Guid id)
    {
        _logger.LogInformation("Getting ServiceOrder by ID with parts details: {Id}", id);
        return await _context.ServiceOrders
            .Include(so => so.PartsUsed)
            .ThenInclude(sop => sop.Part)
            .FirstOrDefaultAsync(so => so.Id == id);
    }

    public async Task<ServiceOrder?> GetByIdForUpdateAsync(Guid id)
    {
        _logger.LogInformation("Getting ServiceOrder by ID for update: {Id}", id);
        return await _context.ServiceOrders
            .Include(so => so.StatusHistory)
            .Include(so => so.PartsUsed)
                .ThenInclude(sop => sop.Part)
            .Include(so => so.ServicesUsed)
                .ThenInclude(sos => sos.Service)
            .Include(so => so.Customer)
            .Include(so => so.Vehicle)
            .AsSplitQuery()
            .FirstOrDefaultAsync(so => so.Id == id);
    }

    public async Task SaveChangesAsync(ServiceOrder order)
    {
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
