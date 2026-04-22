using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;

namespace OficinaApi.Application.Interfaces;

public interface IExternalQueryService
{
    Task<ServiceOrderProgressDto?> GetOrderProgressAsync(Guid orderId);
    Task<AverageExecutionTimeDto> GetAverageExecutionTimeAsync();
}
