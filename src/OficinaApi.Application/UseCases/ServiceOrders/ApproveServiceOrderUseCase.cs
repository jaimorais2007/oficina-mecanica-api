using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class ApproveServiceOrderUseCase : IUseCase<ApproveServiceOrderRequest, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;

        public ApproveServiceOrderUseCase(IServiceOrderRepository serviceOrderRepository)
        {
            _serviceOrderRepository = serviceOrderRepository;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(ApproveServiceOrderRequest input)
        {
            try
            {
                var serviceOrder = await _serviceOrderRepository.GetByIdAsync(input.Id);
                if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

                serviceOrder.ApproveServiceOrder();
                await _serviceOrderRepository.SaveChangesAsync(serviceOrder);

                return UseCaseResponse<ServiceOrderDto>.Success(new ServiceOrderDto(serviceOrder));
            }
            catch (Exception ex)
            {
                return UseCaseResponse<ServiceOrderDto>.Failure(ex.Message);
            }
        }
    }
}
