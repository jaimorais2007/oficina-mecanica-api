using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Parts
{
    public class DeletePartUseCase : IUseCase<Guid, bool>
    {
        private readonly IPartRepository _partRepository;

        public DeletePartUseCase(IPartRepository partRepository)
        {
            _partRepository = partRepository;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(Guid input)
        {
            try
            {
                await _partRepository.DeleteAsync(input);
                return UseCaseResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
