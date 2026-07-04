using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Parts
{
    public class GetPartByIdUseCase : IUseCase<Guid, PartDto?>
    {
        private readonly IPartRepository _partRepository;
        private readonly ILogger<GetPartByIdUseCase> _logger;

        public GetPartByIdUseCase(IPartRepository partRepository, ILogger<GetPartByIdUseCase> logger)
        {
            _partRepository = partRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<PartDto?>> ExecuteAsync(Guid input)
        {
            var part = await _partRepository.GetByIdAsync(input);
            if (part == null)
            {
                _logger.LogInformation("Nenhuma peça encontrada com ID '{PartId}' em GetPartByIdUseCase. Retornando nulo.", input);
                return UseCaseResponse<PartDto?>.Success(null);
            }

            var dto = new PartDto
            {
                Id = part.Id,
                Name = part.Name,
                Code = part.Code,
                QuantityInStock = part.QuantityInStock,
                Price = part.Price
            };
            return UseCaseResponse<PartDto?>.Success(dto);
        }
    }
}
