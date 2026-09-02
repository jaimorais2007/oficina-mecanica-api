using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class GetAllServiceOrdersUseCase : IUseCase<NoInput, IEnumerable<ServiceOrderDto>>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly ILogger<GetAllServiceOrdersUseCase> _logger;

        public GetAllServiceOrdersUseCase(
            IServiceOrderRepository serviceOrderRepository,
            ILogger<GetAllServiceOrdersUseCase> logger)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<IEnumerable<ServiceOrderDto>>> ExecuteAsync(NoInput input)
        {
            var serviceOrders = await _serviceOrderRepository.GetAllAsync();
            var dtos = serviceOrders.Select(so => new ServiceOrderDto(so));
            return UseCaseResponse<IEnumerable<ServiceOrderDto>>.Success(dtos);
        }
    }
}
