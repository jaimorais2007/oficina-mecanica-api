using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Services
{
    public class UpdateServiceUseCase : IUseCase<UpdateServiceRequest, ServiceDto>
    {
        private readonly IServiceRepository _serviceRepository;

        public UpdateServiceUseCase(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<UseCaseResponse<ServiceDto>> ExecuteAsync(UpdateServiceRequest input)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(input.Id);

                if (service == null)
                    return UseCaseResponse<ServiceDto>.Failure("Serviço não encontrado.");

                service.Update(
                    input.Dto.Name,
                    input.Dto.Description,
                    input.Dto.DefaultPrice
                );

                await _serviceRepository.UpdateAsync(service);

                return UseCaseResponse<ServiceDto>.Success(new ServiceDto(service));
            }
            catch (Exception ex)
            {
                return UseCaseResponse<ServiceDto>.Failure(ex.Message);
            }
        }
    }
}
