using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.UseCases.Vehicles;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Services
{
    public class LogicalDeletionServiceUseCase : IUseCase<Guid, NoInput>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<LogicalDeletionVehicleUseCase> _logger;

        public LogicalDeletionServiceUseCase(IServiceRepository serviceRepository,ILogger<LogicalDeletionVehicleUseCase> logger)
        {
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<NoInput>> ExecuteAsync(Guid input)
        {
            var service = await _serviceRepository.GetByIdAsync(input);

            if (service == null)
            {
                _logger.LogInformation(
                    "Service with ID {ServiceId} was not found.",
                    input);

                return UseCaseResponse<NoInput>.Success(new NoInput());
            }

            service.SetInactive(!service.Inactive);

            await _serviceRepository.UpdateAsync(service);
            return UseCaseResponse<NoInput>.Success(new NoInput());
        }
    }
}
