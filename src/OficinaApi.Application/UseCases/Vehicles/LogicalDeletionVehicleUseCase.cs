using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Vehicles
{
    public class LogicalDeletionVehicleUseCase : IUseCase<Guid, NoInput>
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<LogicalDeletionVehicleUseCase> _logger;

        public LogicalDeletionVehicleUseCase(
            IVehicleRepository vehicleRepository,
            ILogger<LogicalDeletionVehicleUseCase> logger)
        {
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<NoInput>> ExecuteAsync(Guid input)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(input);

            if (vehicle == null)
            {
                _logger.LogInformation(
                    "Vehicle with ID {VehicleId} was not found.",
                    input);

                return UseCaseResponse<NoInput>.Success(new NoInput());
            }

            vehicle.SetInactive(!vehicle.Inactive);

            await _vehicleRepository.UpdateAsync(vehicle);
            return UseCaseResponse<NoInput>.Success(new NoInput());
        }
    }
}