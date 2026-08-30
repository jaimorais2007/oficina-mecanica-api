using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Parts
{
    public class AddStockUseCase : IUseCase<AddStockRequest, bool>
    {
        private readonly IPartRepository _partRepository;
        private readonly ILogger<AddStockUseCase> _logger;

        public AddStockUseCase(IPartRepository partRepository, ILogger<AddStockUseCase> logger)
        {
            _partRepository = partRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(AddStockRequest input)
        {
            try
            {
                var part = await _partRepository.GetByIdAsync(input.Id);
                if (part == null)
                {
                    _logger.LogInformation("Falha na validação em AddStockUseCase: Peça com ID '{PartId}' não encontrada.", input.Id);
                    throw new DomainException($"Peça com ID '{input.Id}' não encontrada.");
                }

                part.AddStock(input.Quantity);
                await _partRepository.UpdateAsync(part);

                return UseCaseResponse<bool>.Success(true);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Erro inesperado em AddStockUseCase para a peça com ID '{PartId}'.", input.Id);
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
