using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Services
{
    public class CreateServiceUseCase : IUseCase<CreateServiceDto, ServiceDto>
    {
        private readonly IServiceRepository _serviceRepository;

        public CreateServiceUseCase(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<UseCaseResponse<ServiceDto>> ExecuteAsync(CreateServiceDto input)
        {
            var service = new Service(input.Name, input.Description, input.DefaultPrice);
            await _serviceRepository.AddAsync(service);

            return UseCaseResponse<ServiceDto>.Success(new ServiceDto(service));
        }
    }
}
