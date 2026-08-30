using System;
using System.Reflection.Metadata;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Exceptions;

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
    public bool Inactive { get; private set; } = false;

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
        AddDomainEvent(new ServiceOrderStatusChangedEvent(this));
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
            throw new DomainException("A ordem de serviço deve estar no status 'Recebida' para iniciar a análise técnica.");
        ChangeStatus(OrderStatus.InDiagnostics);
    }

    public void FinishAnalysis()
    {
        var lastStatus = GetLastStatusHistory();
        if (lastStatus.Status != OrderStatus.InDiagnostics)
            throw new DomainException("A ordem de serviço deve estar no status 'Em Análise' para finalizar a análise técnica.");
        ChangeStatus(OrderStatus.WaitingApproval);
        CalculateBudget();
    }

    public void ApproveServiceOrder()
    {
        var lastStatus = GetLastStatusHistory();
        if (lastStatus.Status != OrderStatus.WaitingApproval)
            throw new DomainException("A ordem de serviço deve estar no status 'Aguardando Aprovação' para ser aprovada.");
       
        ChangeStatus(OrderStatus.Executing);
        AddDomainEvent(new ServiceOrderApprovedEvent(Id));
    }

    public void FinishExecution()
    {
        var lastStatus = GetLastStatusHistory();
        if(lastStatus.Status != OrderStatus.Executing)
            throw new DomainException("A ordem de serviço deve estar no status 'Em Execução' para finalizar a execução.");

        var pendingStocks = GetPendingStocks();
        if(pendingStocks.Any())
            throw new DomainException($"Não é possível finalizar a execução de uma ordem de serviço que possui peças pendentes. Por favor verifique as peças: {string.Join(", ", pendingStocks.Select(p => p.Part.Name))}");
        
        ChangeStatus(OrderStatus.Finished);
    }

    public void Deliver()
    {
        var lastStatus = GetLastStatusHistory();
        if(lastStatus.Status != OrderStatus.Finished)
            throw new DomainException("A ordem de serviço deve estar no status 'Finalizada' para ser entregue.");
        
        ChangeStatus(OrderStatus.Delivered);

    }

    public void Refuse()
    {
        var lastStatus = GetLastStatusHistory();
        if (lastStatus.Status != OrderStatus.WaitingApproval)
            throw new DomainException("A ordem de serviço deve estar no status 'Aguardando Aprovação' para ser recusada.");

        ChangeStatus(OrderStatus.Refused);
    }

    public void AddPart(Part part, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.");

        if (!HasPermissionToUpdatePartsAndServices())
            throw new DomainException("Não é permitido adicionar peças neste status da ordem de serviço.");

        PartsUsed.Add(new ServiceOrderPart(this, part, quantity));
    }

    public void AddService(Service service)
    {
        if (!HasPermissionToUpdatePartsAndServices())
            throw new DomainException("Não é permitido adicionar serviços neste status da ordem de serviço.");
            
        ServicesUsed.Add(new ServiceOrderService(this, service));
    }

    private bool HasPermissionToUpdatePartsAndServices()
    {
        var currentStatus = StatusHistory.OrderByDescending(s => s.CreatedAt).FirstOrDefault()?.Status;
        return currentStatus == OrderStatus.Received || currentStatus == OrderStatus.InDiagnostics;
    }

    public ServiceOrderStatus GetLastStatusHistory()
    {
        return StatusHistory
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefault()
            ?? throw new DomainException($"Ordem de serviço {Id} não possui histórico de status.");
    }

    public ICollection<ServiceOrderPart> GetPendingStocks()
    {
        if (GetLastStatusHistory().Status != OrderStatus.Executing)
            throw new DomainException("A ordem de serviço deve estar no status 'Em Execução' para verificar os estoques pendentes.");
        return PartsUsed.Where(p => !p.StockQuantityWasEnsured).ToList();
    }
    private void ChangeStatus(OrderStatus status)
    {
        StatusHistory.Add(new ServiceOrderStatus(this, status));
        AddDomainEvent(new ServiceOrderStatusChangedEvent(this));
    }
}
