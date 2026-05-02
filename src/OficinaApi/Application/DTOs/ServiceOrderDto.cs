using System;
using OficinaApi.Domain.Entities;

namespace OficinaApi.Application.DTOs;

public class ServiceOrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public string CustomerDocument { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedExecutionAt { get; set; }
    public decimal Budget { get; set; }
    public DateTime? FinishedExecutionAt { get; set; }

    public ServiceOrderDto(ServiceOrder serviceOrder)
    {
        Id = serviceOrder.Id;
        CustomerId = serviceOrder.Customer.Id;
        VehicleId = serviceOrder.Vehicle.Id;
        CustomerDocument = serviceOrder.Customer.Document.Value;
        VehiclePlate = serviceOrder.Vehicle.Plate.Value;
        CreatedAt = serviceOrder.CreatedAt;
        Budget = serviceOrder.Budget;
    }
}

public class CreateServiceOrderDto
{
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public List<Guid> ServicesUsed { get; set; } = [];
}
