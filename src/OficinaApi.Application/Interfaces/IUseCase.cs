using System.Threading.Tasks;
using OficinaApi.Application.DTOs;

namespace OficinaApi.Application.Interfaces
{
    public interface IUseCase<in TInput, TOutput>
    {
        Task<UseCaseResponse<TOutput>> ExecuteAsync(TInput input);
    }
}
