using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OficinaApi.Domain.Entities;

namespace OficinaApi.Domain.Interfaces;

public interface IPartRepository
{
    Task<Part?> GetByIdAsync(Guid id);
    Task<IEnumerable<Part>> GetAllAsync();
    Task AddAsync(Part part);
    Task UpdateAsync(Part part);
    Task DeleteAsync(Guid id);
    Task<Part?> GetByIdWithServiceOrderDetailsAsync(Guid id);
}
