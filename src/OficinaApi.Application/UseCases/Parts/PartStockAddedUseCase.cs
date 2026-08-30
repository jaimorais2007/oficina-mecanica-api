using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Parts;

public class PartStockAddedUseCase : IUseCase<PartStockAddedEvent, bool>
{
    private readonly ILogger<PartStockAddedUseCase> _logger;
    private readonly IPartRepository _partRepository;
    private readonly IServiceOrderPartRepository _serviceOrderPartRepository;

    public PartStockAddedUseCase(
        ILogger<PartStockAddedUseCase> logger,
        IPartRepository partRepository,
        IServiceOrderPartRepository serviceOrderPartRepository)
    {
        _logger = logger;
        _partRepository = partRepository;
        _serviceOrderPartRepository = serviceOrderPartRepository;
    }

    public async Task<UseCaseResponse<bool>> ExecuteAsync(PartStockAddedEvent domainEvent)
    {
        var part = await _partRepository.GetByIdWithServiceOrderDetailsAsync(domainEvent.PartId);
        if (part == null)
        {
            _logger.LogWarning("Peça com ID {PartId} não encontrada.", domainEvent.PartId);
            return UseCaseResponse<bool>.Failure($"Peça com ID {domainEvent.PartId} não encontrada.");
        }

        var serviceOrderPartsToEnsure = part.ServiceOrdersParts.Where(sop => sop.StockQuantityShouldBeEnsured()).ToList();
        if (!serviceOrderPartsToEnsure.Any())
        {
            _logger.LogDebug("Nenhuma ordem de serviço pendente encontrada para a peça com ID {PartId}.", domainEvent.PartId);
            return UseCaseResponse<bool>.Success(true);
        }

        var ensured = new List<ServiceOrderPart>();
        foreach (ServiceOrderPart serviceOrderPart in serviceOrderPartsToEnsure)
        {
            try
            {
                serviceOrderPart.EnsureStockQuantity();
                ensured.Add(serviceOrderPart);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Erro ao garantir a quantidade de estoque para a peça com ID {PartId}.", domainEvent.PartId);
            }
        }

        if (ensured.Count > 0)
        {
            await _serviceOrderPartRepository.UpdateRangeAsync(ensured);
            _logger.LogInformation("Estoque descontado automaticamente para a peça com ID {PartId}.", domainEvent.PartId);
        }

        return UseCaseResponse<bool>.Success(true);
    }
}
