using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;
using OficinaApi.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace OficinaApi.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly OficinaDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(OficinaDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Searching for user with ID: {Id}", id);
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        _logger.LogInformation("Searching for user with Email: {Email}", email);
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        _logger.LogInformation("Searching for all users.");
        return await _context.Users.ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
