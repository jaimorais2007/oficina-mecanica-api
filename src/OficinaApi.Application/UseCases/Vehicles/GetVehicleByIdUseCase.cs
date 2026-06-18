using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Vehicles
{
    public class GetVehicleByIdUseCase : IUseCase<Guid, VehicleDto?>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public GetVehicleByIdUseCase(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<UseCaseResponse<VehicleDto?>> ExecuteAsync(Guid input)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(input);
            if (vehicle == null) return UseCaseResponse<VehicleDto?>.Success(null);

            return UseCaseResponse<VehicleDto?>.Success(new VehicleDto(vehicle));
        }
    }
}
