using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Vehicles
{
    public class DeleteVehicleUseCase : IUseCase<Guid, bool>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public DeleteVehicleUseCase(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<UseCaseResponse<bool>> ExecuteAsync(Guid input)
        {
            try
            {
                await _vehicleRepository.DeleteAsync(input);
                return UseCaseResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return UseCaseResponse<bool>.Failure(ex.Message);
            }
        }
    }
}
