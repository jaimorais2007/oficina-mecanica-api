using System;
using System.Threading.Tasks;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.UseCases.Vehicles
{
    public class CreateVehicleUseCase : IUseCase<CreateVehicleDto, VehicleDto>
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICustomerRepository _customerRepository;

        public CreateVehicleUseCase(IVehicleRepository vehicleRepository, ICustomerRepository customerRepository)
        {
            _vehicleRepository = vehicleRepository;
            _customerRepository = customerRepository;
        }

        public async Task<UseCaseResponse<VehicleDto>> ExecuteAsync(CreateVehicleDto input)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(input.CustomerId);
                if (customer == null)
                    throw new ArgumentException("Cliente não encontrado.");

                var vehicle = new Vehicle(customer, input.Plate, input.Brand, input.Model, input.Year);

                await _vehicleRepository.AddAsync(vehicle);

                return UseCaseResponse<VehicleDto>.Success(new VehicleDto(vehicle));
            }
            catch (Exception ex)
            {
                return UseCaseResponse<VehicleDto>.Failure(ex.Message);
            }
        }
    }
}
