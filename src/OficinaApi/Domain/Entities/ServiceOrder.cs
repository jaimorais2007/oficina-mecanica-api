using System;
using OficinaApi.Domain.Enums;

namespace OficinaApi.Domain.Entities;

public class ServiceOrder
{
    public Guid Id { get; private set; }
    public string ClientCpf { get; private set; }
    public Plate VehiclePlate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedExecutionAt { get; private set; }
    public DateTime? FinishedExecutionAt { get; private set; }
    public ICollection<ServiceOrderStatus> StatusHistory { get; set; } = [];
    public ICollection<ServiceOrderServices> ServicesUsed { get; set; } = [];
    public ICollection<ServiceOrderParts> PartsUsed { get; set; } = [];
    public decimal Budget { get; private set; }

    // For EF Core
    protected ServiceOrder() 
    {
        ClientCpf = string.Empty;
        VehiclePlate = default!;
    }

    public ServiceOrder(string clientCpf, string vehiclePlate)
    {
        Id = Guid.NewGuid();
        ClientCpf = clientCpf;
        VehiclePlate = new Plate(vehiclePlate);
        StatusHistory.Add(new ServiceOrderStatus(this, OrderStatus.Received));
        CreatedAt = DateTime.UtcNow;
    }

    public void CalculateBudget(IEnumerable<Service> servicesUsed, IEnumerable<Part> partsUsed)
    {
        Budget = servicesUsed.Sum(s => s.DefaultPrice) + partsUsed.Sum(p => p.Price);
    }

    public void StartDiagnostics()
    {
        StatusHistory.Add(new ServiceOrderStatus(this, OrderStatus.InDiagnostics));
    }

    public void FinishAnalysis()
    {
        StatusHistory.Add(new ServiceOrderStatus(this, OrderStatus.WaitingApproval));
    }

    public void ApproveServiceOrder()
    {
        StatusHistory.Add(new ServiceOrderStatus(this, OrderStatus.Executing));
    }

    public void FinishServiceOrder()
    {
        StatusHistory.Add(new ServiceOrderStatus(this, OrderStatus.Finished));
    }

    public DateTime GetStartedExecutionAt(){
        var startedStatus = StatusHistory.FirstOrDefault(s => s.Status == OrderStatus.Received);
        return startedStatus?.CreatedAt ?? DateTime.MinValue;
    }

    public DateTime GetFinishedExecutionAt(){
        var finishedStatus = StatusHistory.FirstOrDefault(s => s.Status == OrderStatus.Finished);
        return finishedStatus?.CreatedAt ?? DateTime.MinValue;
    }

    public void AddPart(Part part, int quantity)
    {
        if (!HasPermissionToUpdatePartsAndServices())
            throw new InvalidOperationException("Não é permitido adicionar peças neste status da ordem de serviço.");

        PartsUsed.Add(new ServiceOrderParts(this, part, quantity));
    }

    public void AddService(Service service)
    {
        if (!HasPermissionToUpdatePartsAndServices())
            throw new InvalidOperationException("Não é permitido adicionar serviços neste status da ordem de serviço.");
            
        ServicesUsed.Add(new ServiceOrderServices(this, service));
    }

    private bool HasPermissionToUpdatePartsAndServices()
    {
        var currentStatus = StatusHistory.LastOrDefault()?.Status;
        return currentStatus == OrderStatus.Received || currentStatus == OrderStatus.InDiagnostics || currentStatus == OrderStatus.WaitingApproval;
    }
}
