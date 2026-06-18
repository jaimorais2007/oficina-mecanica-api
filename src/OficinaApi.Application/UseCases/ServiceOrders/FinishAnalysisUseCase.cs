using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class FinishAnalysisUseCase : IUseCase<FinishAnalysisRequest, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly IEmailService _emailService;

        public FinishAnalysisUseCase(IServiceOrderRepository serviceOrderRepository, IEmailService emailService)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _emailService = emailService;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(FinishAnalysisRequest input)
        {
            try
            {
                var serviceOrder = await _serviceOrderRepository.GetByIdAsync(input.Id);
                if (serviceOrder == null) throw new ArgumentException("Ordem de serviço não encontrada.");

                serviceOrder.FinishAnalysis();
                await _serviceOrderRepository.SaveChangesAsync(serviceOrder);

                if (!string.IsNullOrWhiteSpace(serviceOrder.Customer.Email))
                {
                    await _emailService.SendAsync(
                        serviceOrder.Customer.Email,
                        "Ordem de Serviço - Aguardando Aprovação",
                        $"Olá, sua ordem de serviço {serviceOrder.Id} foi analisada.\n" +
                        $"Orçamento: R$ {serviceOrder.Budget}\n" +
                        $"Por favor, aprove para continuarmos.");
                }

                return UseCaseResponse<ServiceOrderDto>.Success(new ServiceOrderDto(serviceOrder));
            }
            catch (Exception ex)
            {
                return UseCaseResponse<ServiceOrderDto>.Failure(ex.Message);
            }
        }
    }
}
