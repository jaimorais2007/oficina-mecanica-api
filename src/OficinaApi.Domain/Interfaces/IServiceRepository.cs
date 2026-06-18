using OficinaApi.Domain.Entities;

namespace OficinaApi.Domain.Interfaces
{
    public interface IServiceRepository
    {
        Task AddAsync(Service service);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Service>> GetAllAsync();
        Task<IEnumerable<Service>> GetByIdListAsync(IEnumerable<Guid> ids);
        Task<Service?> GetByIdAsync(Guid id);
        Task UpdateAsync(Service service);
    }
}
