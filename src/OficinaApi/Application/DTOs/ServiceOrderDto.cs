using System;

namespace OficinaApi.Application.DTOs;

public class ServiceOrderDto
{
    public Guid Id                          { get; set; }
    public string ClientCpf                 { get; set; } = string.Empty;
    public string VehiclePlate              { get; set; } = string.Empty;
    public DateTime CreatedAt               { get; set; }
    public DateTime? StartedExecutionAt     { get; set; }
    public decimal Budget                   { get; set; }
    public DateTime? FinishedExecutionAt    { get; set; }
}

public class CreateServiceOrderDto
{
    public Guid Id                          { get; set; }
    public string ClientCpf                 { get; set; } = string.Empty;
    public DateTime CreatedAt               { get; set; }
    public CreateVehicleDto Vehicle         { get; set; }
    public List<CreateServiceDto> Services  { get; set; }
    public List<CreatePartDto> PartsUsed    { get; set; }
    public DateTime? StartedExecutionAt     { get; set; }
    public DateTime? FinishedExecutionAt    { get; set; }
}
