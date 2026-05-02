using System;
using OficinaApi.Domain.Enums;

namespace OficinaApi.Domain.Entities;

public class ServiceOrder
{
    public Guid Id { get; private set; }
    public string ClientCpf { get; private set; }
    public string VehiclePlate { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedExecutionAt { get; private set; }
    public DateTime? FinishedExecutionAt { get; private set; }
    public ICollection<ServiceOrderStatus> StatusHistory { get; set; } = [];

    // For EF Core
    protected ServiceOrder() 
    {
        ClientCpf = string.Empty;
        VehiclePlate = string.Empty;
    }

    public ServiceOrder(string clientCpf, string vehiclePlate)
    {
        Id = Guid.NewGuid();
        ClientCpf = clientCpf;
        VehiclePlate = vehiclePlate;
        Status = OrderStatus.Received;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        // Record times for metrics regarding execution length
        if (newStatus == OrderStatus.Executing && !StartedExecutionAt.HasValue)
        {
            StartedExecutionAt = DateTime.UtcNow;
        }
        else if (newStatus == OrderStatus.Finished && !FinishedExecutionAt.HasValue)
        {
            FinishedExecutionAt = DateTime.UtcNow;
        }

        Status = newStatus;
    }

    // A helper method for Persona 3 metric: Execution time
    public TimeSpan? GetExecutionTime()
    {
        if (StartedExecutionAt.HasValue && FinishedExecutionAt.HasValue)
            return FinishedExecutionAt.Value - StartedExecutionAt.Value;
        return null;
    }
}
