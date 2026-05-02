using System;
using OficinaApi.Domain.Enums;

namespace OficinaApi.Domain.Entities;

public class ServiceOrder
{
    public Guid Id { get; private set; }
    public string ClientCpf { get; private set; }
    public Plate VehiclePlate { get; private set; }
    public DateTime CreatedAt { get; private set; }
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

    public void CalculateBudget()
    {
        Budget = ServicesUsed.Select(s => s.Service).Sum(s => s.DefaultPrice) + PartsUsed.Select(p => p.Part).Sum(p => p.Price);
    }

    public void StartDiagnostics()
    {
        var lastStatus = GetLastStatusHistory();
        if (lastStatus.Status != OrderStatus.Received)
            throw new InvalidOperationException("A ordem de serviço deve estar no status 'Recebida' para iniciar a análise técnica.");
        StatusHistory.Add(new ServiceOrderStatus(this, OrderStatus.InDiagnostics));
    }

    public void FinishAnalysis()
    {
        var lastStatus = GetLastStatusHistory();
        if (lastStatus.Status != OrderStatus.InDiagnostics)
            throw new InvalidOperationException("A ordem de serviço deve estar no status 'Em Análise' para finalizar a análise técnica.");
        StatusHistory.Add(new ServiceOrderStatus(this, OrderStatus.WaitingApproval));
        CalculateBudget();
    }

    public void ApproveServiceOrder()
    {
        var lastStatus = GetLastStatusHistory();
        if (lastStatus.Status != OrderStatus.WaitingApproval)
            throw new InvalidOperationException("A ordem de serviço deve estar no status 'Aguardando Aprovação' para ser aprovada.");
        StatusHistory.Add(new ServiceOrderStatus(this, OrderStatus.Executing));
    }

    public void FinishExecution()
    {
        var lastStatus = GetLastStatusHistory();
        if(lastStatus.Status != OrderStatus.Executing)
            throw new InvalidOperationException("A ordem de serviço deve estar no status 'Em Execução' para finalizar a execução.");
        StatusHistory.Add(new ServiceOrderStatus(this, OrderStatus.Finished));
    }

    public void Deliver()
    {
        var lastStatus = GetLastStatusHistory();
        if(lastStatus.Status != OrderStatus.Finished)
            throw new InvalidOperationException("A ordem de serviço deve estar no status 'Finalizada' para ser entregue.");
        StatusHistory.Add(new ServiceOrderStatus(this, OrderStatus.Delivered));
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

    public ServiceOrderStatus GetLastStatusHistory()
    {
        return StatusHistory
            .OrderByDescending(s => s.CreatedAt)
            .First();
    }
}
