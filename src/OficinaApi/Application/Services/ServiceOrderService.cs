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
    private readonly ICustomerRepository _customerRepository;

    public ServiceOrderService(
        IServiceOrderRepository serviceOrderRepository, 
        IVehicleRepository vehicleRepository, 
        IServiceRepository serviceRepository,
        IPartRepository partRepository,
        ICustomerRepository customerRepository
    )
    {
        _serviceOrderRepository = serviceOrderRepository;
        _vehicleRepository = vehicleRepository;
        _serviceRepository = serviceRepository;
        _partRepository = partRepository;
        _customerRepository = customerRepository;
    }

    public async Task<ServiceOrderDto> CreateServiceOrderAsync(CreateServiceOrderDto dto)
    {
        if (dto.VehicleId == Guid.Empty) throw new ArgumentException("Veículo da Ordem de Serviço não informado.");
        if (dto.ServicesUsed.IsNullOrEmpty()) throw new ArgumentException("Serviços que serão feitos não foram informados.");

        var existingVehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
        if (existingVehicle is null)
            throw new ArgumentException("Veículo não encontrado.");

        var existingCustomer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (existingCustomer is null)
            throw new ArgumentException("Cliente não encontrado.");

        var servicesFounds = await _serviceRepository.GetByIdListAsync(dto.ServicesUsed);
        if(servicesFounds.Count() != dto.ServicesUsed.Count())
            throw new ArgumentException("Algum dos serviços informados não foi encontrado.");

        var serviceOrder = new ServiceOrder(existingCustomer, existingVehicle, servicesFounds);
        await _serviceOrderRepository.AddAsync(serviceOrder);
        
        return new ServiceOrderDto(serviceOrder);
    }

    public async Task<ServiceOrderDto?> GetServiceOrderByIdAsync(Guid id)
    {
        var serviceOrder = await _serviceOrderRepository.GetByIdAsync(id);
        if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

        return new ServiceOrderDto(serviceOrder);
    }

    public async Task<IEnumerable<ServiceOrderDto>> GetAllServiceOrdersAsync()
    {
        var serviceOrders = await _serviceOrderRepository.GetAllAsync();
        return serviceOrders.Select(so => new ServiceOrderDto(so));
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
        if (dto.Quantity <= 0) throw new ArgumentException("A quantidade deve ser maior que zero.");

        ServiceOrder? serviceOrder = await _serviceOrderRepository.GetByIdForUpdateAsync(id);
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

    public async Task<ServiceOrderDto> FinishAnalysisAsync(Guid id)
    {
        var serviceOrder = await _serviceOrderRepository.GetByIdAsync(id);
        if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

        serviceOrder.FinishAnalysis();
        await _serviceOrderRepository.UpdateAsync(serviceOrder);

        return new ServiceOrderDto(serviceOrder);
    }

    public async Task<ServiceOrderDto> ApproveServiceOrderAsync(Guid id)
    {
        var serviceOrder = await _serviceOrderRepository.GetByIdAsync(id);
        if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

        serviceOrder.ApproveServiceOrder();
        await _serviceOrderRepository.UpdateAsync(serviceOrder);

        return new ServiceOrderDto(serviceOrder);
    }

    public async Task<ServiceOrderDto> FinishExecutionAsync(Guid id)
    {
        var serviceOrder = await _serviceOrderRepository.GetByIdAsync(id);
        if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

        serviceOrder.FinishExecution();
        await _serviceOrderRepository.UpdateAsync(serviceOrder);

        return new ServiceOrderDto(serviceOrder);
    }

    public async Task<ServiceOrderDto> DeliverServiceOrderAsync(Guid id)
    {
        var serviceOrder = await _serviceOrderRepository.GetByIdAsync(id);
        if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

        serviceOrder.Deliver();
        await _serviceOrderRepository.UpdateAsync(serviceOrder);

        return new ServiceOrderDto(serviceOrder);
    }

    public async Task<IEnumerable<ServiceOrderPeddingStockDto>> GetServiceOrderPeddingStocksAsync(Guid id)
    {
        var serviceOrder = await _serviceOrderRepository.GetServiceOrderByIdToGetPeddingStocksAsync(id);
        if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

        return serviceOrder.GetPendingStocks().Select(a => new ServiceOrderPeddingStockDto(a.PartId, a.Part.Name, a.Quantity));

    }

    public async Task<double> GetAverageDurationInDaysAsync()
    {
        return await _serviceOrderRepository.GetAverageDurationInDaysAsync();
    }
}
