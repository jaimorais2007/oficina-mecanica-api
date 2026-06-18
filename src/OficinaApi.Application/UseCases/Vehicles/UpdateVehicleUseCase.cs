using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Vehicles
{
    public class UpdateVehicleUseCase : IUseCase<UpdateVehicleRequest, VehicleDto>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public UpdateVehicleUseCase(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<UseCaseResponse<VehicleDto>> ExecuteAsync(UpdateVehicleRequest input)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(input.Id);

                if (vehicle == null)
                    throw new ArgumentException("Veículo não encontrado.");

                vehicle.Update(
                    input.Dto.Plate,
                    input.Dto.Brand,
                    input.Dto.Model,
                    input.Dto.Year
                );

                await _vehicleRepository.UpdateAsync(vehicle);

                return UseCaseResponse<VehicleDto>.Success(new VehicleDto(vehicle));
            }
            catch (Exception ex)
            {
                return UseCaseResponse<VehicleDto>.Failure(ex.Message);
            }
        }
    }
}
