using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class GetServiceOrderByIdUseCase : IUseCase<Guid, ServiceOrderDto?>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;

        public GetServiceOrderByIdUseCase(IServiceOrderRepository serviceOrderRepository)
        {
            _serviceOrderRepository = serviceOrderRepository;
        }

        public async Task<UseCaseResponse<ServiceOrderDto?>> ExecuteAsync(Guid input)
        {
            try
            {
                var serviceOrder = await _serviceOrderRepository.GetByIdAsync(input);
                if (serviceOrder == null)
                    return UseCaseResponse<ServiceOrderDto?>.Failure("Ordem de serviço não encontrada.");

                return UseCaseResponse<ServiceOrderDto?>.Success(new ServiceOrderDto(serviceOrder));
            }
            catch (Exception ex)
            {
                return UseCaseResponse<ServiceOrderDto?>.Failure(ex.Message);
            }
        }
    }
}
