using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Services
{
    public class GetAllServicesUseCase : IUseCase<NoInput, IEnumerable<ServiceDto>>
    {
        private readonly IServiceRepository _serviceRepository;

        public GetAllServicesUseCase(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<UseCaseResponse<IEnumerable<ServiceDto>>> ExecuteAsync(NoInput input)
        {
            var services = await _serviceRepository.GetAllAsync();
            var dtos = services.Select(x => new ServiceDto(x));
            return UseCaseResponse<IEnumerable<ServiceDto>>.Success(dtos);
        }
    }
}
