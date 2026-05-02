using System;
using System.Reflection.Metadata;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Events;

namespace OficinaApi.Domain.Entities;

public class ServiceOrder : BaseEntity
{
    public Customer Customer { get; private set; }
    public Guid CustomerId { get; private set; }
    public Vehicle Vehicle { get; private set; }
    public Guid VehicleId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<ServiceOrderStatus> StatusHistory { get; private set; } = [];
    public ICollection<ServiceOrderService> ServicesUsed { get; private set; } = [];
    public ICollection<ServiceOrderPart> PartsUsed { get; private set; } = [];
    public decimal Budget { get; private set; }

    // For EF Core
    protected ServiceOrder() { }

    public ServiceOrder(Customer customer, Vehicle vehicle, IEnumerable<Service> servicesUserd)
    {
        Id = Guid.NewGuid();
        Customer = customer;
        CustomerId = customer.Id;
        Vehicle = vehicle;
        VehicleId = vehicle.Id;
        ServicesUsed = servicesUserd.Select(s => new ServiceOrderService(this, s)).ToList();
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
        AddDomainEvent(new ServiceOrderApprovedEvent(Id));
    }

    public void FinishExecution()
    {
        var pendingStocks = GetPendingStocks();
        if(pendingStocks.Any())
            throw new InvalidOperationException($"Não é possível finalizar a execução de uma ordem de serviço que possui peças pendentes. Por favor verifique as peças: {string.Join(", ", pendingStocks.Select(p => p.Part.Name))}");

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

    public void AddPart(Part part, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero.");

        if (!HasPermissionToUpdatePartsAndServices())
            throw new InvalidOperationException("Não é permitido adicionar peças neste status da ordem de serviço.");

        PartsUsed.Add(new ServiceOrderPart(this, part, quantity));
    }

    public void AddService(Service service)
    {
        if (!HasPermissionToUpdatePartsAndServices())
            throw new InvalidOperationException("Não é permitido adicionar serviços neste status da ordem de serviço.");
            
        ServicesUsed.Add(new ServiceOrderService(this, service));
    }

    private bool HasPermissionToUpdatePartsAndServices()
    {
        var currentStatus = StatusHistory.OrderByDescending(s => s.CreatedAt).FirstOrDefault()?.Status;
        return currentStatus == OrderStatus.Received || currentStatus == OrderStatus.InDiagnostics || currentStatus == OrderStatus.WaitingApproval;
    }

    public ServiceOrderStatus GetLastStatusHistory()
    {
        return StatusHistory
            .OrderByDescending(s => s.CreatedAt)
            .First();
    }

    public ICollection<ServiceOrderPart> GetPendingStocks()
    {
        return PartsUsed.Where(p => !p.StockQuantityWasEnsured).ToList();
    }
}
