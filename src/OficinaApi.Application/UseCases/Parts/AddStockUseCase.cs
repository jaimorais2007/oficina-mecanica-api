using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Parts
{
    public class AddStockUseCase : IUseCase<AddStockRequest, bool>
    {
        private readonly IPartRepository _partRepository;

        public AddStockUseCase(IPartRepository partRepository)
        {
            _partRepository = partRepository;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(AddStockRequest input)
        {
            try
            {
                var part = await _partRepository.GetByIdAsync(input.Id);
                if (part == null)
                    throw new KeyNotFoundException($"Peça com ID '{input.Id}' não encontrada.");

                part.AddStock(input.Quantity);
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
