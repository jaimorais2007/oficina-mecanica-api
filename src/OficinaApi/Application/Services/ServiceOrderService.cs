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
        if (dto.VehicleUsed is null) throw new ArgumentException("Veículo da Ordem de Serviço não informado.");
        if (dto.ServicesUsed.IsNullOrEmpty()) throw new ArgumentException("Serviços que serão feitos não foram informados.");

        var serviceOrder = new ServiceOrder(dto.ClientCpf, dto.VehicleUsed.Plate);
        await _serviceOrderRepository.AddAsync(serviceOrder);

        var existingVehicle = await _vehicleRepository.GetVehicleAsync(new Plate(dto.VehicleUsed.Plate));
        if (existingVehicle is null)
            await _vehicleRepository.AddAsync(new Vehicle(dto.VehicleUsed.Plate, dto.VehicleUsed.Brand, dto.VehicleUsed.Model, dto.VehicleUsed.Year));

        var servicesUsed = new List<Service>();
        var partsUsed = new List<Part>();

        foreach (var service in dto.ServicesUsed)
        {
            if (service.Id.HasValue)
                servicesUsed.Add(await _serviceRepository.GetByIdAsync(service.Id.Value));
            else
            {
                var serviceEntity = new Service(service.Name, service.Description, service.DefaultPrice);
                await _serviceRepository.AddAsync(serviceEntity);
                servicesUsed.Add(serviceEntity);
            }
        }
        
        if (!dto.PartsUsed.IsNullOrEmpty())
            foreach(var part in dto.PartsUsed)
            {
                if (part.Id.HasValue)
                    partsUsed.Add(await _partRepository.GetByIdAsync(part.Id.Value));
                else
                {
                    var partEntity = new Part(part.Name, part.Code, part.InitialQuantity, part.Price);
                    await _partRepository.AddAsync(partEntity);
                    partsUsed.Add(partEntity);
                }
            }

        return new ServiceOrderDto
        {
            VehiclePlate = serviceOrder.VehiclePlate,
            ClientCpf = serviceOrder.ClientCpf,
            CreatedAt = serviceOrder.CreatedAt,
            FinishedExecutionAt = serviceOrder.FinishedExecutionAt,
            StartedExecutionAt = serviceOrder.StartedExecutionAt,
            Id = serviceOrder.Id,
            Budget = servicesUsed.Sum(s => s.DefaultPrice) + partsUsed.Sum(p => p.Price)
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
