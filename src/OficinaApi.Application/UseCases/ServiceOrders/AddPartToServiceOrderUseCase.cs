using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class AddPartToServiceOrderUseCase : IUseCase<AddPartToServiceOrderRequest, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly IPartRepository _partRepository;

        public AddPartToServiceOrderUseCase(
            IServiceOrderRepository serviceOrderRepository,
            IPartRepository partRepository)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _partRepository = partRepository;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(AddPartToServiceOrderRequest input)
        {
            try
            {
                if (input.Dto.Quantity <= 0) throw new ArgumentException("A quantidade deve ser maior que zero.");

                ServiceOrder? serviceOrder = await _serviceOrderRepository.GetByIdForUpdateAsync(input.Id);
                if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

                Part? part = await _partRepository.GetByIdAsync(input.Dto.PartId);
                if (part == null) throw new ArgumentException("Peça não encontrada.");

                serviceOrder.AddPart(part, input.Dto.Quantity);
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
