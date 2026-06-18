using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Vehicles
{
    public class GetAllVehiclesUseCase : IUseCase<NoInput, IEnumerable<VehicleDto?>>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public GetAllVehiclesUseCase(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<UseCaseResponse<IEnumerable<VehicleDto?>>> ExecuteAsync(NoInput input)
        {
            var vehicles = await _vehicleRepository.GetAllAsync();
            var dtos = vehicles.Select(x => new VehicleDto(x));
            return UseCaseResponse<IEnumerable<VehicleDto?>>.Success(dtos);
        }
    }
}
