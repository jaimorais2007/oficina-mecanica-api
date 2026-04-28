using System;

namespace OficinaApi.Application.DTOs;

public class ServiceOrderDto
{
    public Guid Id { get;  set; }
    public string ClientCpf { get;  set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public DateTime CreatedAt { get;  set; }
    public DateTime? StartedExecutionAt { get;  set; }
    public DateTime? FinishedExecutionAt { get;  set; }
}

public class CreateServiceOrderDto
{
    public Guid Id { get;  set; }
    public string ClientCpf { get;  set; } = string.Empty;
    public string VehiclePlate { get;  set; } = string.Empty;
    public DateTime CreatedAt { get;  set; }
    public DateTime? StartedExecutionAt { get;  set; }
    public DateTime? FinishedExecutionAt { get;  set; }
}
