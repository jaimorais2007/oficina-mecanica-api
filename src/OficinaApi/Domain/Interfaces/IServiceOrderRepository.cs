using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OficinaApi.Domain.Entities;

namespace OficinaApi.Domain.Interfaces;

public interface IServiceOrderRepository
{
    Task<ServiceOrder?> GetByIdAsync(Guid id);
    // Needed to calculate average execution time of finished services
    Task<IEnumerable<ServiceOrder>> GetAllFinishedOrdersAsync();
    
    // Stub to add new orders (useful for integration tests)
    Task AddAsync(ServiceOrder order);
    Task UpdateAsync(ServiceOrder order);
}
