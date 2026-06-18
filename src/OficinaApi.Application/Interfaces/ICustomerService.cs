using OficinaApi.Application.DTOs;

namespace OficinaApi.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto?>> GetAllCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(Guid id);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
        Task DeleteCustomerAsync(Guid id);
        Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerDto dto);
    }
}
