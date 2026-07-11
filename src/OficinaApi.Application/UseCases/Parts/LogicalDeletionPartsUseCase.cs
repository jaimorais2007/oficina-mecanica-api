using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.UseCases.Vehicles;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Parts
{
    public class LogicalDeletionPartsUseCase : IUseCase<Guid, NoInput>
    {
        private readonly IPartRepository _partRepository;
        private readonly ILogger<LogicalDeletionVehicleUseCase> _logger;

        public LogicalDeletionPartsUseCase(IPartRepository partRepository, ILogger<LogicalDeletionVehicleUseCase> logger)
        {
            _partRepository = partRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<NoInput>> ExecuteAsync(Guid input)
        {
            var part = await _partRepository.GetByIdAsync(input);

            if (part == null)
            {
                _logger.LogInformation("Nenhuma peça encontrada com ID '{PartId}'.", input);
                return UseCaseResponse<NoInput>.Success(new NoInput());
            }

            part.SetInactive(!part.Inactive);

            await _partRepository.UpdateAsync(part);
            return UseCaseResponse<NoInput>.Success(new NoInput());
        }
    }
}
