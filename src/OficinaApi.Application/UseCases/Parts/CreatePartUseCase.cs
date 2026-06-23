using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Parts
{
    public class CreatePartUseCase : IUseCase<CreatePartDto, PartDto>
    {
        private readonly IPartRepository _partRepository;

        public CreatePartUseCase(IPartRepository partRepository)
        {
            _partRepository = partRepository;
        }

        public async Task<UseCaseResponse<PartDto>> ExecuteAsync(CreatePartDto input)
        {
            var part = new Part(input.Name, input.Code, input.InitialQuantity, input.Price);
            await _partRepository.AddAsync(part);

            var dto = new PartDto
            {
                Id = part.Id,
                Name = part.Name,
                Code = part.Code,
                QuantityInStock = part.QuantityInStock,
                Price = part.Price
            };
            return UseCaseResponse<PartDto>.Success(dto);
        }
    }
}
