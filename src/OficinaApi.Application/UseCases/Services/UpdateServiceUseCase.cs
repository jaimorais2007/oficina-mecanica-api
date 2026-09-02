using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Services
{
    public class UpdateServiceUseCase : IUseCase<UpdateServiceRequest, ServiceDto>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<UpdateServiceUseCase> _logger;

        public UpdateServiceUseCase(IServiceRepository serviceRepository, ILogger<UpdateServiceUseCase> logger)
        {
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<ServiceDto>> ExecuteAsync(UpdateServiceRequest input)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(input.Id);

                if (service == null)
                {
                    _logger.LogInformation("Service with ID {ServiceId} was not found for update.", input.Id);
                    return UseCaseResponse<ServiceDto>.Failure("Serviço não encontrado.");
                }

                service.Update(
                    input.Dto.Name,
                    input.Dto.Description,
                    input.Dto.DefaultPrice
                );

                await _serviceRepository.UpdateAsync(service);

                return UseCaseResponse<ServiceDto>.Success(new ServiceDto(service));
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error updating service with ID {ServiceId}", input.Id);
                return UseCaseResponse<ServiceDto>.Failure(ex.Message);
            }
        }
    }
}
