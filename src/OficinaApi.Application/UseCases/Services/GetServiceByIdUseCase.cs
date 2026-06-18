using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Services
{
    public class GetServiceByIdUseCase : IUseCase<Guid, ServiceDto?>
    {
        private readonly IServiceRepository _serviceRepository;

        public GetServiceByIdUseCase(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<UseCaseResponse<ServiceDto?>> ExecuteAsync(Guid input)
        {
            var service = await _serviceRepository.GetByIdAsync(input);
            if (service == null) return UseCaseResponse<ServiceDto?>.Success(null);

            return UseCaseResponse<ServiceDto?>.Success(new ServiceDto(service));
        }
    }
}
