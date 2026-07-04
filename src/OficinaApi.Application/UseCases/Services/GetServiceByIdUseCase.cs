using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Services
{
    public class GetServiceByIdUseCase : IUseCase<Guid, ServiceDto?>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<GetServiceByIdUseCase> _logger;

        public GetServiceByIdUseCase(IServiceRepository serviceRepository, ILogger<GetServiceByIdUseCase> logger)
        {
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<ServiceDto?>> ExecuteAsync(Guid input)
        {
            var service = await _serviceRepository.GetByIdAsync(input);
            if (service == null)
            {
                _logger.LogInformation("Service with ID {ServiceId} was not found.", input);
                return UseCaseResponse<ServiceDto?>.Success(null);
            }

            return UseCaseResponse<ServiceDto?>.Success(new ServiceDto(service));
        }
    }
}
