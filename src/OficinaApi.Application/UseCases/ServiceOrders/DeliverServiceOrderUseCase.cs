using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class DeliverServiceOrderUseCase : IUseCase<DeliverServiceOrderRequest, ServiceOrderDto>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<DeliverServiceOrderUseCase> _logger;

        public DeliverServiceOrderUseCase(
            IServiceOrderRepository serviceOrderRepository,
            IEmailService emailService,
            ILogger<DeliverServiceOrderUseCase> logger)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<UseCaseResponse<ServiceOrderDto>> ExecuteAsync(DeliverServiceOrderRequest input)
        {
            try
            {
                var serviceOrder = await _serviceOrderRepository.GetByIdAsync(input.Id);
                if (serviceOrder == null)
                {
                    _logger.LogInformation("Service Order not found for delivery. Id: {Id}", input.Id);
                    throw new ArgumentException("Ordem de serviço não encontrada.");
                }

                serviceOrder.Deliver();
                await _serviceOrderRepository.SaveChangesAsync(serviceOrder);

                if (!string.IsNullOrWhiteSpace(serviceOrder.Customer.Email))
                {
                    _logger.LogInformation("Sending delivery email to customer: {Email}", serviceOrder.Customer.Email);
                    await _emailService.SendAsync(
                        serviceOrder.Customer.Email,
                        "Ordem de Serviço Finalizada",
                        $"Olá, sua ordem de serviço {serviceOrder.Id} foi concluída e está pronta para retirada.");
                }

                return UseCaseResponse<ServiceOrderDto>.Success(new ServiceOrderDto(serviceOrder));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering service order");
                return UseCaseResponse<ServiceOrderDto>.Failure(ex.Message);
            }
        }
    }
}
