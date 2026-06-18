using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Parts
{
    public class RemoveStockUseCase : IUseCase<RemoveStockRequest, bool>
    {
        private readonly IPartRepository _partRepository;

        public RemoveStockUseCase(IPartRepository partRepository)
        {
            _partRepository = partRepository;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(RemoveStockRequest input)
        {
            try
            {
                var part = await _partRepository.GetByIdAsync(input.Id);
                if (part == null)
                    throw new Exception("Peça não encontrada.");

                part.RemoveStock(input.Quantity);
                await _partRepository.UpdateAsync(part);

                return UseCaseResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
