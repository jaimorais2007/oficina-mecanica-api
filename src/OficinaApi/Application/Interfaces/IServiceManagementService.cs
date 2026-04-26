using OficinaApi.Application.DTOs;

namespace OficinaApi.Application.Interfaces
{
    public interface IServiceManagementService
    {
        Task<IEnumerable<ServiceDto?>> GetAllServicesAsync();
        Task<ServiceDto?> GetServiceByIdAsync(Guid id);
        Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto);
        Task DeleteServiceAsync(Guid id);
        Task<ServiceDto> UpdateServiceAsync(Guid id, UpdateServiceDto dto);
    }
}
