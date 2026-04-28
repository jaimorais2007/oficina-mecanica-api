using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.Services;

public class ServiceOrderService : IServiceOrderService
{
    private readonly IServiceOrderRepository _serviceOrderRepository;

    public ServiceOrderService(IServiceOrderRepository serviceOrderRepository)
    {
        _serviceOrderRepository = serviceOrderRepository;
    }

    public async Task<ServiceOrderDto> CreateServiceOrderAsync(CreateServiceOrderDto dto)
    {
        var serviceOrder = new ServiceOrder(dto.ClientCpf, dto.VehiclePlate);
        await _serviceOrderRepository.AddAsync(serviceOrder);

        return new ServiceOrderDto
        {
            VehiclePlate = serviceOrder.VehiclePlate,
            ClientCpf = serviceOrder.ClientCpf,
            CreatedAt = serviceOrder.CreatedAt,
            FinishedExecutionAt = serviceOrder.FinishedExecutionAt,
            StartedExecutionAt = serviceOrder.StartedExecutionAt,
            Id = serviceOrder.Id
        };
    }

    public async Task<ServiceOrderDto?> GetServiceOrderByIdAsync(Guid id)
    {
        var serviceOrder = await _serviceOrderRepository.GetByIdAsync(id);
        if (serviceOrder == null) return null;

        return new ServiceOrderDto
        {
            VehiclePlate = serviceOrder.VehiclePlate,
            ClientCpf = serviceOrder.ClientCpf,
            CreatedAt = serviceOrder.CreatedAt,
            FinishedExecutionAt = serviceOrder.FinishedExecutionAt,
            StartedExecutionAt = serviceOrder.StartedExecutionAt,
            Id = serviceOrder.Id
        };
    }
}
