using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Parts
{
    public class GetPartByIdUseCase : IUseCase<Guid, PartDto?>
    {
        private readonly IPartRepository _partRepository;

        public GetPartByIdUseCase(IPartRepository partRepository)
        {
            _partRepository = partRepository;
        }

        public async Task<UseCaseResponse<PartDto?>> ExecuteAsync(Guid input)
        {
            var part = await _partRepository.GetByIdAsync(input);
            if (part == null) return UseCaseResponse<PartDto?>.Success(null);

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
