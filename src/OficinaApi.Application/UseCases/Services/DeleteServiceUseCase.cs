using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Services
{
    public class DeleteServiceUseCase : IUseCase<Guid, bool>
    {
        private readonly IServiceRepository _serviceRepository;

        public DeleteServiceUseCase(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(Guid input)
        {
            try
            {
                await _serviceRepository.DeleteAsync(input);
                return UseCaseResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
