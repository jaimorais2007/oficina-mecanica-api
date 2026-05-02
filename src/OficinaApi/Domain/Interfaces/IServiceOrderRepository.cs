using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OficinaApi.Domain.Entities;

namespace OficinaApi.Domain.Interfaces;

public interface IServiceOrderRepository
{
    Task<ServiceOrder?> GetByIdAsync(Guid id);
    Task<IEnumerable<ServiceOrder>> GetAllFinishedOrdersAsync();
    Task<ServiceOrder?> GetByIdWithPartsDetailsAsync(Guid id);
    Task AddAsync(ServiceOrder order);
    Task UpdateAsync(ServiceOrder order);
}
