using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Vehicles
{
    public class GetVehicleByIdUseCase : IUseCase<Guid, VehicleDto?>
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<GetVehicleByIdUseCase> _logger;

        public GetVehicleByIdUseCase(IVehicleRepository vehicleRepository, ILogger<GetVehicleByIdUseCase> logger)
        {
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<VehicleDto?>> ExecuteAsync(Guid input)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(input);
            if (vehicle == null)
            {
                _logger.LogInformation("Vehicle with ID {VehicleId} was not found.", input);
                return UseCaseResponse<VehicleDto?>.Success(null);
            }

            return UseCaseResponse<VehicleDto?>.Success(new VehicleDto(vehicle));
        }
    }
}
