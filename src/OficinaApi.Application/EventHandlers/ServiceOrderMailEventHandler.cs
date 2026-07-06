using Microsoft.Extensions.Logging;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.EventHandlers;

public class ServiceOrderMailEventHandler : IDomainEventHandler<ServiceOrderStatusChangedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<ServiceOrderMailEventHandler> _logger;

    public ServiceOrderMailEventHandler(
        IEmailService emailService,
        ILogger<ServiceOrderMailEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(ServiceOrderStatusChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        var serviceOrder = domainEvent.ServiceOrder;

        if (string.IsNullOrWhiteSpace(serviceOrder.Customer?.Email))
            return;

        var (subject, body) = BuildEmail(serviceOrder);

        _logger.LogInformation(
            "Sending status email to {Email}",
            serviceOrder.Customer.Email);

        await _emailService.SendAsync(
            serviceOrder.Customer.Email,
            subject,
            body);
    }

    private static (string Subject, string Body) BuildEmail(ServiceOrder serviceOrder)
    {
        var status = serviceOrder.GetLastStatusHistory().Status;

        switch (status)
        {
            case OrderStatus.Received:
                return (
                    "Ordem de Serviço Recebida",
                    $"Olá!\n\n" +
                    $"Recebemos sua ordem de serviço {serviceOrder.Id}.\n" +
                    $"Ela foi cadastrada com sucesso e em breve iniciaremos o atendimento."
                );

            case OrderStatus.InDiagnostics:
                return (
                    "Diagnóstico Iniciado",
                    $"Olá!\n\n" +
                    $"Sua ordem de serviço {serviceOrder.Id} entrou em diagnóstico."
                );

            case OrderStatus.WaitingApproval:
                return (
                    "Ordem de Serviço - Aguardando Aprovação",
                    $"Olá!\n\n" +
                    $"Sua ordem de serviço {serviceOrder.Id} foi analisada.\n" +
                    $"Orçamento: R$ {serviceOrder.Budget:N2}\n\n" +
                    $"Por favor, aprove o orçamento para continuarmos."
                );

            case OrderStatus.Executing:
                return (
                    "Serviço em Execução",
                    $"Olá!\n\n" +
                    $"O serviço referente à ordem {serviceOrder.Id} já está sendo executado."
                );

            case OrderStatus.Finished:
                return (
                    "Serviço Finalizado",
                    $"Olá!\n\n" +
                    $"Sua ordem de serviço {serviceOrder.Id} foi finalizada e está pronta para retirada."
                );

            case OrderStatus.Delivered:
                return (
                    "Veículo Entregue",
                    $"Olá!\n\n" +
                    $"O veículo da ordem de serviço {serviceOrder.Id} foi entregue.\n\n" +
                    $"Agradecemos pela preferência!"
                );

            case OrderStatus.Refused:
                return (
                    "Orçamento Recusado",
                    $"Olá!\n\n" +
                    $"A ordem de serviço {serviceOrder.Id} foi encerrada após a recusa do orçamento."
                );

            default:
                return (
                    "Atualização da Ordem de Serviço",
                    $"O status da ordem de serviço {serviceOrder.Id} foi atualizado."
                );
        }
    }
}
