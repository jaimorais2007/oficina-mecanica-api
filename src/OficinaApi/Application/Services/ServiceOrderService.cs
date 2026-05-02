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
        if (string.IsNullOrEmpty(dto.VehiclePlate)) throw new ArgumentException("Veículo da Ordem de Serviço não informado.");
        if (dto.ServicesUsed.IsNullOrEmpty()) throw new ArgumentException("Serviços que serão feitos não foram informados.");

        var serviceOrder = new ServiceOrder(dto.ClientCpf, dto.VehiclePlate);
        await _serviceOrderRepository.AddAsync(serviceOrder);

        var existingVehicle = await _vehicleRepository.GetVehicleAsync(new Plate(dto.VehiclePlate));
        if (existingVehicle is null)
            throw new ArgumentException("Placa de veiculo não identificada.");

        var servicesFounds = await _serviceRepository.GetByIdListAsync(dto.ServicesUsed);
        if(servicesFounds.Count() != dto.ServicesUsed.Count())
            throw new ArgumentException("Algum dos serviços informados não foi encontrado.");
        
        return new ServiceOrderDto(serviceOrder);
    }

    public async Task<ServiceOrderDto?> GetServiceOrderByIdAsync(Guid id)
    {
        var serviceOrder = await _serviceOrderRepository.GetByIdAsync(id);
        if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

        return new ServiceOrderDto(serviceOrder);
    }

    public async Task<ServiceOrderDto> StartDiagnosticsAsync(Guid id)
    {
        var serviceOrder = await _serviceOrderRepository.GetByIdAsync(id);
        if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

        serviceOrder.StartDiagnostics();
        await _serviceOrderRepository.UpdateAsync(serviceOrder);

        return new ServiceOrderDto(serviceOrder);
    }

    public async Task<ServiceOrderDto> AddPartToServiceOrderAsync(Guid id, AddPartDto dto)
    {
        ServiceOrder? serviceOrder = await _serviceOrderRepository.GetByIdAsync(id);
        if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

        Part? part = await _partRepository.GetByIdAsync(dto.PartId);
        if (part == null) throw new ArgumentException("Peça não encontrada.");

        serviceOrder.AddPart(part, dto.Quantity);
        await _serviceOrderRepository.UpdateAsync(serviceOrder);

        return new ServiceOrderDto(serviceOrder);
    }

    public async Task<ServiceOrderDto> AddServiceToServiceOrderAsync(Guid id, AddServiceDto dto)
    {
        ServiceOrder? serviceOrder = await _serviceOrderRepository.GetByIdAsync(id);
        if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

        Service? service = await _serviceRepository.GetByIdAsync(dto.ServiceId);
        if (service == null) throw new ArgumentException("Serviço não encontrado.");

        serviceOrder.AddService(service);
        await _serviceOrderRepository.UpdateAsync(serviceOrder);

        return new ServiceOrderDto(serviceOrder);
    }

}
