using System;
using System.Linq;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.Services;

public class ExternalQueryService : IExternalQueryService
{
    private readonly IServiceOrderRepository _orderRepository;

    public ExternalQueryService(IServiceOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<AverageExecutionTimeDto> GetAverageExecutionTimeAsync()
    {
        var finishedOrders = await _orderRepository.GetAllFinishedOrdersAsync();
        var finishedOrdersList = finishedOrders.ToList();

        if (!finishedOrdersList.Any())
        {
            return new AverageExecutionTimeDto 
            { 
                AverageExecutionTimeInHours = 0, 
                TotalFinishedOrders = 0 
            };
        }

        double totalHours = 0;
        int validOrders = 0;

        foreach (var order in finishedOrdersList)
        {
            var executionTime = order.GetExecutionTime();
            if (executionTime.HasValue)
            {
                totalHours += executionTime.Value.TotalHours;
                validOrders++;
            }
        }

        return new AverageExecutionTimeDto
        {
            AverageExecutionTimeInHours = validOrders > 0 ? totalHours / validOrders : 0,
            TotalFinishedOrders = validOrders
        };
    }

    public async Task<ServiceOrderProgressDto?> GetOrderProgressAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return null;

        return new ServiceOrderProgressDto
        {
            Id = order.Id,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            StartedExecutionAt = order.StartedExecutionAt,
            FinishedExecutionAt = order.FinishedExecutionAt
        };
    }
}
