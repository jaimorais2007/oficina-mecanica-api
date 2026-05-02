using System;
using OficinaApi.Domain.Entities;

namespace OficinaApi.Application.DTOs;

public class ServiceOrderDto
{
    public Guid Id { get; set; }
    public string ClientCpf { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedExecutionAt { get; set; }
    public decimal Budget { get; set; }
    public DateTime? FinishedExecutionAt { get; set; }

    public ServiceOrderDto(ServiceOrder serviceOrder)
    {
        Id = serviceOrder.Id;
        ClientCpf = serviceOrder.ClientCpf;
        VehiclePlate = serviceOrder.VehiclePlate.Value;
        CreatedAt = serviceOrder.CreatedAt;
        StartedExecutionAt = serviceOrder.StartedExecutionAt;
        FinishedExecutionAt = serviceOrder.FinishedExecutionAt;
        Budget = serviceOrder.Budget;
    }
}

public class CreateServiceOrderDto
{
    public string ClientCpf { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public List<Guid> ServicesUsed { get; set; } = [];
}
