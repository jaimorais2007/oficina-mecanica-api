using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Vehicles
{
    public class UpdateVehicleUseCase : IUseCase<UpdateVehicleRequest, VehicleDto>
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<UpdateVehicleUseCase> _logger;

        public UpdateVehicleUseCase(IVehicleRepository vehicleRepository, ILogger<UpdateVehicleUseCase> logger)
        {
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<VehicleDto>> ExecuteAsync(UpdateVehicleRequest input)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(input.Id);

                if (vehicle == null)
                {
                    _logger.LogInformation("Failed to update vehicle: Vehicle {VehicleId} not found.", input.Id);
                    throw new DomainException("Veículo não encontrado.");
                }

                vehicle.Update(
                    input.Dto.Plate,
                    input.Dto.Brand,
                    input.Dto.Model,
                    input.Dto.Year
                );

                await _vehicleRepository.UpdateAsync(vehicle);

                return UseCaseResponse<VehicleDto>.Success(new VehicleDto(vehicle));
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "An error occurred while updating vehicle with ID {VehicleId}.", input.Id);
                return UseCaseResponse<VehicleDto>.Failure(ex.Message);
            }
        }
    }
}
