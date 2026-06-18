using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class StartDiagnosticsUseCase : IUseCase<StartDiagnosticsRequest, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;

        public StartDiagnosticsUseCase(IServiceOrderRepository serviceOrderRepository)
        {
            _serviceOrderRepository = serviceOrderRepository;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(StartDiagnosticsRequest input)
        {
            try
            {
                var serviceOrder = await _serviceOrderRepository.GetByIdForUpdateAsync(input.Id);
                if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

                serviceOrder.StartDiagnostics();
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
