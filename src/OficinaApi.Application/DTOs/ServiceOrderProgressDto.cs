using System;
using OficinaApi.Domain.Enums;

namespace OficinaApi.Application.DTOs;

public class ServiceOrderProgressDto
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusDescription => Status.ToString();
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedExecutionAt { get; set; }
    public DateTime? FinishedExecutionAt { get; set; }
}

public class AverageExecutionTimeDto
{
    public double AverageExecutionTimeInHours { get; set; }
    public int TotalFinishedOrders { get; set; }
}
