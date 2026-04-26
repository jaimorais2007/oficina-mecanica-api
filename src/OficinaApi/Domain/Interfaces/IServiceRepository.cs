namespace OficinaApi.Domain.Interfaces
{
    public interface IServiceRepository
    {
        Task AddAsync(Service service);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Service>> GetAllAsync();
        Task<Service?> GetByIdAsync(Guid id);
        Task UpdateAsync(Service service);
    }
}
