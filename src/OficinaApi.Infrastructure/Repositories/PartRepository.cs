using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Infrastructure.Data;

namespace OficinaApi.Infrastructure.Repositories;

public class PartRepository : IPartRepository
{
    private readonly OficinaDbContext _context;
    private readonly ILogger<PartRepository> _logger;

    public PartRepository(OficinaDbContext context, ILogger<PartRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(Part part)
    {
        await _context.Parts.AddAsync(part);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var part = await GetByIdAsync(id);
        if (part != null)
        {
            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Part>> GetAllAsync()
    {
        _logger.LogInformation("Executando GetAllAsync em PartRepository. Sem parâmetros de busca.");
        return await _context.Parts.ToListAsync();
    }

    public async Task<Part?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Executando GetByIdAsync em PartRepository com o parâmetro Id: '{PartId}'", id);
        return await _context.Parts.FindAsync(id);
    }

    public async Task<Part?> GetByIdWithServiceOrderDetailsAsync(Guid id)
    {
        _logger.LogInformation("Executando GetByIdWithServiceOrderDetailsAsync em PartRepository com o parâmetro Id: '{PartId}'", id);
        return await _context.Parts
            .Include(p => p.ServiceOrdersParts)
            .ThenInclude(sop => sop.ServiceOrder)
            .ThenInclude(sop => sop.StatusHistory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task UpdateAsync(Part part)
    {
        _context.Parts.Update(part);
        await _context.SaveChangesAsync();
    }
}
