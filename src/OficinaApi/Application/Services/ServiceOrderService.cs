using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.Services;

public class ServiceOrderService : IServiceOrderService
{
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IPartRepository _partRepository;

    public ServiceOrderService(
        IServiceOrderRepository serviceOrderRepository, 
        IVehicleRepository vehicleRepository, 
        IServiceRepository serviceRepository,
        IPartRepository partRepository
    )
    {
        _serviceOrderRepository = serviceOrderRepository;
        _vehicleRepository = vehicleRepository;
        _serviceRepository = serviceRepository;
        _partRepository = partRepository;
    }

    public async Task<ServiceOrderDto> CreateServiceOrderAsync(CreateServiceOrderDto dto)
    {
        if (dto.Vehicle == null) throw new ArgumentException("Veículo não informado.");
        if (dto.Services.IsNullOrEmpty()) throw new ArgumentException("Serviços não informado.");

        var serviceOrder = new ServiceOrder(dto.ClientCpf, dto.Vehicle.Plate);
        await _serviceOrderRepository.AddAsync(serviceOrder);
        await _vehicleRepository.AddAsync(new Vehicle(dto.Vehicle.Plate, dto.Vehicle.Brand, dto.Vehicle.Model, dto.Vehicle.Year));

        foreach(var service in dto.Services)
            await _serviceRepository.AddAsync(new Service(service.Name, service.Description, service.DefaultPrice));
        
        if (!dto.PartsUsed.IsNullOrEmpty())
            foreach(var part in dto.PartsUsed)
                await _partRepository.AddAsync(new Part(part.Name, part.Code, part.InitialQuantity, part.Price));

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
