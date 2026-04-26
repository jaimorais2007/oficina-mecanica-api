using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.Services
{
    public class ServiceManagementService : IServiceManagementService
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceManagementService(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<IEnumerable<ServiceDto?>> GetAllServicesAsync()
        {
            var service = await _serviceRepository.GetAllAsync();
            return service.Select(x => new ServiceDto
            {
                Id = x.Id,
                Name = x.Name,
                DefaultPrice = x.DefaultPrice,
                Description = x.Description
            });
        }

        public async Task<ServiceDto?> GetServiceByIdAsync(Guid id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            if (service == null) return null;

            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                DefaultPrice = service.DefaultPrice,
                Description = service.Description
            };
        }

        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto)
        {
            var service = new Service(dto.Name, dto.Description, dto.DefaultPrice);
            await _serviceRepository.AddAsync(service);

            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                DefaultPrice = service.DefaultPrice,
                Description = service.Description
            };
        }

        public async Task DeleteServiceAsync(Guid id)
        {
            await _serviceRepository.DeleteAsync(id);
        }

        public async Task<ServiceDto> UpdateServiceAsync(Guid id, UpdateServiceDto dto)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
                throw new Exception("Serviço não encontrado.");

            service.Update(
                dto.Name,
                dto.Description,
                dto.DefaultPrice
            );

            await _serviceRepository.UpdateAsync(service);

            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                DefaultPrice = service.DefaultPrice,
                Description = service.Description
            };
        }
    }
}
