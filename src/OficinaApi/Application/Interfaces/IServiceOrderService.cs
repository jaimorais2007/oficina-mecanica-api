using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;

namespace OficinaApi.Application.Interfaces;

public interface IServiceOrderService
{
    Task<ServiceOrderDto> CreateServiceOrderAsync(CreateServiceOrderDto dto);
    Task<ServiceOrderDto?> GetServiceOrderByIdAsync(Guid id);
    Task<ServiceOrderDto> StartDiagnosticsAsync(Guid id);
    Task<ServiceOrderDto> AddPartToServiceOrderAsync(Guid id, AddPartDto dto);
    Task<ServiceOrderDto> AddServiceToServiceOrderAsync(Guid id, AddServiceDto dto);
    Task<ServiceOrderDto> FinishAnalysisAsync(Guid id);
    Task<ServiceOrderDto> ApproveServiceOrderAsync(Guid id);
    Task<ServiceOrderDto> FinishExecutionAsync(Guid id);
    Task<ServiceOrderDto> DeliverServiceOrderAsync(Guid id);
    Task<IEnumerable<ServiceOrderPeddingStockDto>> GetServiceOrderPeddingStocksAsync(Guid id);
    Task<double> GetAverageDurationInDaysAsync();

}
