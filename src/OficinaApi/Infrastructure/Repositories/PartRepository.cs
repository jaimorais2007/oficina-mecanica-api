using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Infrastructure.Data;

namespace OficinaApi.Infrastructure.Repositories;

public class PartRepository : IPartRepository
{
    private readonly OficinaDbContext _context;

    public PartRepository(OficinaDbContext context)
    {
        _context = context;
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
        return await _context.Parts.ToListAsync();
    }

    public async Task<Part?> GetByIdAsync(Guid id)
    {
        return await _context.Parts.FindAsync(id);
    }

    public async Task UpdateAsync(Part part)
    {
        _context.Parts.Update(part);
        await _context.SaveChangesAsync();
    }
}
