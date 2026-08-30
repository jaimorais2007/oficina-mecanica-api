using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Vehicles
{
    public class DeleteVehicleUseCase : IUseCase<Guid, bool>
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<DeleteVehicleUseCase> _logger;

        public DeleteVehicleUseCase(IVehicleRepository vehicleRepository, ILogger<DeleteVehicleUseCase> logger)
        {
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(Guid input)
        {
            try
            {
                await _vehicleRepository.DeleteAsync(input);
                return UseCaseResponse<bool>.Success(true);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "An error occurred while deleting vehicle with ID {VehicleId}.", input);
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
