using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OficinaApi.Domain.Entities;

namespace OficinaApi.Domain.Interfaces;

public interface IServiceOrderRepository
{
    Task<ServiceOrder?> GetByIdAsync(Guid id);
    Task<ServiceOrder?> GetByIdWithPartsDetailsAsync(Guid id);
    Task AddAsync(ServiceOrder order);
    Task UpdateAsync(ServiceOrder order);
    Task<double> GetAverageDurationInDaysAsync();
    Task<ServiceOrder?> GetServiceOrderByIdToGetPeddingStocksAsync(Guid id);
}
