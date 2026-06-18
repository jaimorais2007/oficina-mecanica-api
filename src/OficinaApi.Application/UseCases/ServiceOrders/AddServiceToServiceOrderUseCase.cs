using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class AddServiceToServiceOrderUseCase : IUseCase<AddServiceToServiceOrderRequest, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly IServiceRepository _serviceRepository;

        public AddServiceToServiceOrderUseCase(
            IServiceOrderRepository serviceOrderRepository,
            IServiceRepository serviceRepository)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(AddServiceToServiceOrderRequest input)
        {
            try
            {
                ServiceOrder? serviceOrder = await _serviceOrderRepository.GetByIdAsync(input.Id);
                if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

                Service? service = await _serviceRepository.GetByIdAsync(input.Dto.ServiceId);
                if (service == null) throw new ArgumentException("Serviço não encontrado.");

                serviceOrder.AddService(service);
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
