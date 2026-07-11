using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OficinaApi.Domain.Entities;

namespace OficinaApi.Domain.Interfaces;

public interface IServiceOrderRepository
{
    Task<IEnumerable<ServiceOrder>> GetAllAsync();
    Task<ServiceOrder?> GetByIdAsync(Guid id);
    Task<ServiceOrder?> GetByIdWithPartsDetailsAsync(Guid id);
    Task<ServiceOrder?> GetByIdForUpdateAsync(Guid id);
    Task AddAsync(ServiceOrder order);
    Task SaveChangesAsync(ServiceOrder order);
    Task<double> GetAverageDurationInDaysAsync();
    Task<ServiceOrder?> GetServiceOrderByIdToGetPeddingStocksAsync(Guid id);
    Task<ServiceOrder?> GetByStatus(Guid id);
}
