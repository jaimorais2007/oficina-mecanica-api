using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Parts
{
    public class DeletePartUseCase : IUseCase<Guid, bool>
    {
        private readonly IPartRepository _partRepository;
        private readonly ILogger<DeletePartUseCase> _logger;

        public DeletePartUseCase(IPartRepository partRepository, ILogger<DeletePartUseCase> logger)
        {
            _partRepository = partRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(Guid input)
        {
            try
            {
                await _partRepository.DeleteAsync(input);
                return UseCaseResponse<bool>.Success(true);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Erro ao deletar peça com ID '{PartId}' em DeletePartUseCase.", input);
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
