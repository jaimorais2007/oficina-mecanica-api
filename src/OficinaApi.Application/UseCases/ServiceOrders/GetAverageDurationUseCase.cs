using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.ServiceOrders
{
    public class GetAverageDurationUseCase : IUseCase<NoInput, double>
    {
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly ILogger<GetAverageDurationUseCase> _logger;

        public GetAverageDurationUseCase(
            IServiceOrderRepository serviceOrderRepository,
            ILogger<GetAverageDurationUseCase> logger)
        {
            _serviceOrderRepository = serviceOrderRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<double>> ExecuteAsync(NoInput input)
        {
            try
            {
                var average = await _serviceOrderRepository.GetAverageDurationInDaysAsync();
                return UseCaseResponse<double>.Success(average);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Error getting average duration of service orders");
                return UseCaseResponse<double>.Failure(ex.Message);
            }
        }
    }
}
