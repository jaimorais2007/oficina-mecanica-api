using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Parts
{
    public class GetAllPartsUseCase : IUseCase<NoInput, IEnumerable<PartDto>>
    {
        private readonly IPartRepository _partRepository;

        public GetAllPartsUseCase(IPartRepository partRepository)
        {
            _partRepository = partRepository;
        }

        public async Task<UseCaseResponse<IEnumerable<PartDto>>> ExecuteAsync(NoInput input)
        {
            var parts = await _partRepository.GetAllAsync();
            var dtos = parts.Select(p => new PartDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                QuantityInStock = p.QuantityInStock,
                Price = p.Price
            });
            return UseCaseResponse<IEnumerable<PartDto>>.Success(dtos);
        }
    }
}
